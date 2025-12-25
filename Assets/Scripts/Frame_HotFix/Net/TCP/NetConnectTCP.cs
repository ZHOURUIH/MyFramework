using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static StringUtility;
using static UnityUtility;
using static BinaryUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseUtility;

// 当前程序作为客户端时使用,表示一个与TCP服务器的连接,WebGL无法使用
public abstract class NetConnectTCP : NetConnect
{
	protected DoubleBuffer<PacketReceiveInfo> mReceiveBuffer = new();		// 在主线程中执行的消息列表
	protected DoubleBuffer<PacketSendInfo> mOutputBuffer = new();			// 使用双缓冲提高发送消息的效率
	protected Queue<string> mReceivePacketHistory = new();					// 接收过的包的缓冲列表
	protected NetStateCallback mNetStateCallback;							// 网络状态改变的回调
	protected StreamBuffer mInputBuffer = new(TCP_INPUT_BUFFER);			// 接收消息的缓冲区
	protected StreamBuffer mTotalBuffer = new(CLIENT_MAX_PACKET_SIZE * 8);	// 最终发出的大的缓冲区
	protected ThreadLock mConnectStateLock = new();							// mNetState的锁
	protected ThreadLock mOutputBufferLock = new();							// mOutputBuffer的锁
	protected ThreadLock mSocketLock = new();								// mSocket的锁
	protected ThreadLock mInputBufferLock = new();                          // mInputBuffer的锁
	protected IPAddress mIPAddress;											// 服务器地址
	protected DateTime mPingStartTime;                                      // ping开始的时间
	protected MyThread mReceiveThread = new("SocketReceiveTCP");			// 接收线程
	protected MyThread mSendThread = new("SocketSendTCP");					// 发送线程
	protected MyTimer mPingTimer = new();									// ping计时器
	protected Action mPingCallback;											// 外部设置的用于发送ping包的函数
	protected Socket mSocket;												// 套接字实例
	protected byte[] mRecvBuff = new byte[TCP_RECEIVE_BUFFER];				// 从Socket接收时使用的缓冲区
	protected int mPing;													// 网络延迟,计算方式是从发出一个ping包到接收到一个回复包的间隔时间
	protected int mPort;													// 服务器端口
	protected bool mManualDisconnect;										// 是否正在主动断开连接
	protected bool mManualSendReceive;										// 是否手动去调用doSend和doReceive
	protected NET_STATE mNetState;											// 网络连接状态
	public virtual void init(IPAddress ip, int port)
	{
		mIPAddress = ip;
		mPort = port;
		if (!mManualSendReceive)
		{
			mSendThread.setBackground(false);
			mSendThread.start(sendThread);
			mReceiveThread.start(receiveThread);
		}
		// 每2秒发出一个ping包
		mPingTimer.init(0.0f, 2.0f, false);
		mPingTimer.setEnsureInterval(true);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiveBuffer.destroy();
		mOutputBuffer.destroy();
		mReceivePacketHistory.Clear();
		mNetStateCallback = null;
		mInputBuffer.clear();
		mTotalBuffer.clear();
		// mConnectStateLock.unlock();
		// mOutputBufferLock.unlock();
		// mSocketLock.unlock();
		// mInputBufferLock.unlock();
		mIPAddress = null;
		mReceiveThread.stop();
		mSendThread.stop();
		mSocket = null;
		memset(mRecvBuff, (byte)0);
		mPort = 0;
		mManualDisconnect = false;
		mManualSendReceive = false;
		mNetState = NET_STATE.NONE;
		mPingStartTime = default;
		mPingTimer.stop();
		mPing = 0;
		mPingCallback = null;
	}
	public bool isConnected()									{ return mNetState == NET_STATE.CONNECTED; }
	public bool isConnecting()									{ return mNetState == NET_STATE.CONNECTING; }
	public bool isDisconnected()								{ return mNetState != NET_STATE.CONNECTED && mNetState != NET_STATE.CONNECTING; }
	public bool isManualDisconnect()							{ return mManualDisconnect; }
	public NetStateCallback getNetStateCallback()				{ return mNetStateCallback; }
	public void setNetStateCallback(NetStateCallback callback)	{ mNetStateCallback = callback; }
	public void startConnect(Action<bool> callback)
	{
		if (isConnected() || isConnecting())
		{
			callback?.Invoke(false);
			return;
		}
		mManualDisconnect = false;
		notifyNetState(NET_STATE.CONNECTING);
		// 创建socket
		using (new ThreadLockScope(mSocketLock))
		{
			if (mSocket != null)
			{
				callback?.Invoke(false);
				logError("当前Socket不为空");
				return;
			}
			mSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			mSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
		}
		if (isDevOrEditor())
		{
			log("开始连接服务器:" + mIPAddress);
		}
		mSocket.BeginConnect(mIPAddress, mPort, (IAsyncResult ar) =>
		{
			try
			{
				mSocket.EndConnect(ar);
			}
			catch (SocketException e)
			{
				delayCall(callback, false);
				log("init socket exception : " + e.Message);
				socketException(e);
				return;
			}
			log("连接服务器成功");
			notifyNetState(NET_STATE.CONNECTED);
			delayCall(callback, true);
		}, mSocket);
	}
	public void disconnect()
	{
		mManualDisconnect = true;
		clearSocket();
		mPingTimer.stop(false);
		using (var a = new DoubleBufferReader<PacketReceiveInfo>(mReceiveBuffer))
		{
			try
			{
				foreach (PacketReceiveInfo item in a.mReadList.safe())
				{
					UN_ARRAY_BYTE_THREAD(item.mPacketData);
				}
			}
			catch (Exception e)
			{
				logException(e, "使用读列表中错误");
			}
		}
		mReceiveBuffer.clear();
		// 主动关闭时,网络状态应该是无状态
		notifyNetState(NET_STATE.NONE);
	}
	public virtual void update(float elapsedTime)
	{
		if (mNetState == NET_STATE.CONNECTED && mPingCallback != null && mPingTimer.tickTimer(elapsedTime))
		{
			mPingStartTime = DateTime.Now;
			mPingCallback.Invoke();
		}
		// 解析所有已经收到的消息包
		using var a = new DoubleBufferReader<PacketReceiveInfo>(mReceiveBuffer);
		try
		{
			foreach (PacketReceiveInfo info in a.mReadList.safe())
			{
				NetPacket packet = parsePacket(info.mType, info.mPacketData, info.mPacketSize, info.mSequence, info.mFieldFlag);
				UN_ARRAY_BYTE_THREAD(info.mPacketData);
				if (packet == null)
				{
					continue;
				}
				using var b = new ProfilerScope(packet.GetType().ToString());
				packet.execute();
				mNetPacketFactory.destroyPacket(packet);
			}
		}
		catch (Exception e)
		{
			logException(e, "socket packet error");
		}
	}
	public override void destroy()
	{
		base.destroy();
		mManualDisconnect = true;
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
		mOutputBuffer.destroy();
		mReceiveBuffer.destroy();
		mConnectStateLock.destroy();
		mOutputBufferLock.destroy();
		mSocketLock.destroy();
		mInputBufferLock.destroy();
	}
	public void setPort(int port) { mPort = port; }
	public void setIPAddress(IPAddress ip) { mIPAddress = ip; }
	public void setPingAction(Action callback) { mPingCallback = callback; }
	public void setManualSendReceive(bool manual) { mManualSendReceive = manual; }
	public void notifyReceivePing()
	{
		mPing = (int)(DateTime.Now - mPingStartTime).TotalMilliseconds;
		mPingTimer.start();
	}
	public int getPing() { return mPing; }
	public abstract void sendNetPacket(NetPacket packet);
	public NET_STATE getNetState() { return mNetState; }
	public virtual void clearSocket()
	{
		using (new ThreadLockScope(mSocketLock))
		{
			try
			{
				if (mSocket != null)
				{
					if (mSocket.Connected)
					{
						mSocket.Shutdown(SocketShutdown.Both);
						mSocket.Disconnect(false);
					}
					mSocket.Close();
					mSocket.Dispose();
					mSocket = null;
				}
			}
			catch (Exception e)
			{
				log("关闭连接时异常：" + e.Message);
				mSocket = null;
			}
		}
	}
	// 由于连接成功操作可能不在主线程,所以只能是外部在主线程通知网络管理器连接成功
	public void notifyConnected()
	{
		// 建立连接后将消息列表中残留的消息清空,双缓冲中的读写列表都要清空
		using (new ThreadLockScope(mOutputBufferLock))
		{
			foreach (var list in mOutputBuffer.getBufferList())
			{
				foreach (PacketSendInfo item in list)
				{
					UN_ARRAY_BYTE_THREAD(item.mData);
				}
			}
			mOutputBuffer.clear();
		}
		// 开始心跳计时
		mPingTimer.start();
		using (new ThreadLockScope(mInputBufferLock))
		{
			mInputBuffer.clear();
		}
	}
	// doSend和doReceive开放出来方便外部自己处理发送和接收线程,而不是在内部固定在单独的线程中运行
	public void doSend()
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			return;
		}
		mTotalBuffer.clear();
		// 获取输出数据的读缓冲区,手动拼接到大的缓冲区中
		using (new ThreadLockScope(mOutputBufferLock))
		{
			using var a = new DoubleBufferReader<PacketSendInfo>(mOutputBuffer);
			// 获取不到数据,则没有任何数据需要发送,直接返回即可
			foreach (PacketSendInfo item in a.mReadList.safe())
			{
				if (item.mData == null)
				{
					continue;
				}
				// 如果加入失败,则说明这一帧要发送的数据太多,先发送出去
				if (!mTotalBuffer.addData(item.mData, item.mDataSize))
				{
					sendTotalData();
					// 再重新合并,如果这一次还加入失败,说明真的数据太大了
					mTotalBuffer.addData(item.mData, item.mDataSize);
				}
				// 回收缓冲区的内存
				if (item.mDataNeedDestroy)
				{
					UN_ARRAY_BYTE_THREAD(item.mData);
				}
			}
		}
		sendTotalData();
	}
	public void doReceive()
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			return;
		}
		try
		{
			// 在Receive之前先判断SocketBuffer中有没有数据可以读,因为如果不判断直接调用的话,可能会出现即使SocketBuffer中有数据,
			// Receive仍然获取不到的问题,具体原因未知,且出现几率也比较小,但是仍然可能会出现.所以先判断再Receive就不会出现这个问题
			while (mSocket.Available == 0)
			{
				return;
			}
			int nRecv = mSocket.Receive(mRecvBuff);
			if (nRecv == 0)
			{
				// 服务器关闭了连接
				notifyNetState(NET_STATE.SERVER_ABORT, SocketError.NotConnected);
				return;
			}
			else if (nRecv < 0)
			{
				// 服务器异常
				notifyNetState(NET_STATE.SERVER_CLOSE, SocketError.NotConnected);
				return;
			}
			using (new ThreadLockScope(mInputBufferLock))
			{
				if (!mInputBuffer.addData(mRecvBuff, nRecv))
				{
					logError("添加数据到缓冲区失败!数量:" + nRecv + ",当前缓冲区中数据:" + mInputBuffer.getDataLength() + ",缓冲区大小:" + mInputBuffer.getBufferSize());
				}
			}
			// 解析接收到的数据
			while (true)
			{
				using (new ThreadLockScope(mInputBufferLock))
				{
					PARSE_RESULT result = preParsePacket(mInputBuffer.getData(), mInputBuffer.getDataLength(), out int bitIndex, out byte[] packetData,
													out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag);
					if (result != PARSE_RESULT.SUCCESS)
					{
						if (result == PARSE_RESULT.ERROR)
						{
							debugHistoryPacket();
							mInputBuffer.clear();
						}
						break;
					}
					mReceiveBuffer.add(new(packetData, fieldFlag, packetSize, sequence, packetType));

					if (!mInputBuffer.removeData(0, bitCountToByteCount(bitIndex)))
					{
						logError("移除数据失败");
					}
					if (isDevOrEditor())
					{
						string info = "已接收 : " + IToS(packetType) + ", 字节数:" + IToS(bitCountToByteCount(bitIndex));
						log(info, LOG_LEVEL.LOW);
						mReceivePacketHistory.Enqueue(info);
						if (mReceivePacketHistory.Count > 10)
						{
							mReceivePacketHistory.Dequeue();
						}
					}
					if (mInputBuffer.getDataLength() <= 0)
					{
						break;
					}
				}
			}
		}
		catch (ObjectDisposedException) { }
		catch (SocketException e)
		{
			socketException(e);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract NetPacket parsePacket(ushort packetType, byte[] buffer, int size, int sequence, ulong fieldFlag);
	// 发送Socket消息
	protected void sendThread(ref bool run)
	{
		if (mManualSendReceive)
		{
			return;
		}
		doSend();
	}
	// 接收Socket消息
	protected void receiveThread(ref bool run)
	{
		if (mManualSendReceive)
		{
			return;
		}
		doReceive();
	}
	protected void sendTotalData()
	{
		int allLength = mTotalBuffer.getDataLength();
		if (allLength == 0)
		{
			return;
		}
		try
		{
			byte[] allBytes = mTotalBuffer.getData();
			int allSendedCount = 0;
			while (allSendedCount < allLength)
			{
				int thisSendCount = mSocket.Send(allBytes, allSendedCount, allLength - allSendedCount, SocketFlags.None);
				if (thisSendCount == 0)
				{
					// 服务器关闭了连接
					notifyNetState(NET_STATE.SERVER_ABORT, SocketError.NotConnected);
					break;
				}
				else if (thisSendCount < 0)
				{
					// 服务器异常
					notifyNetState(NET_STATE.SERVER_CLOSE, SocketError.NotConnected);
					break;
				}
				allSendedCount += thisSendCount;
			}
		}
		catch (ObjectDisposedException) { }
		catch (SocketException e)
		{
			socketException(e);
		}
		mTotalBuffer.clear();
	}
	protected abstract PARSE_RESULT preParsePacket(byte[] buffer, int size, out int bitIndex, out byte[] outPacketData, 
													out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag);
	protected void debugHistoryPacket()
	{
		using var a = new ClassThreadScope<MyStringBuilder>(out var info);
		info.append("最后接收的消息:\n");
		foreach (string item in mReceivePacketHistory)
		{
			info.append(item, "\n");
		}
		logError(info.ToString());
	}
	protected void socketException(SocketException e)
	{
		// 本地网络异常,基本全部都认为是网络问题,这样可以进行重连,而不是只提示服务器关闭
		// 如果服务器真的关了,那也只是会重连失败
		NET_STATE state = NET_STATE.NET_CLOSE;
		if (e.SocketErrorCode == SocketError.NetworkUnreachable)
		{
			state = NET_STATE.NET_CLOSE;
		}
		else if (e.SocketErrorCode == SocketError.ConnectionRefused)
		{
			state = NET_STATE.NET_CLOSE;
		}
		else if (e.SocketErrorCode == SocketError.ConnectionAborted)
		{
			state = NET_STATE.NET_CLOSE;
		}
		// 服务器关闭了连接,而客户端再向服务器发消息时,服务器就会返回ConnectionReset
		// 网络切换时也会返回ConnectionReset
		else if (e.SocketErrorCode == SocketError.ConnectionReset)
		{
			state = NET_STATE.NET_CLOSE;
		}
		notifyNetState(state, e.SocketErrorCode);
	}
	protected void notifyNetState(NET_STATE state, SocketError errorCode = SocketError.Success)
	{
		using (new ThreadLockScope(mConnectStateLock))
		{
			NET_STATE lastState = mNetState;
			mNetState = state;
			if (!isConnected() && !isConnecting())
			{
				clearSocket();
			}
			CMD_DELAY_THREAD(out CmdNetConnectTCPState cmd, LOG_LEVEL.FORCE);
			if (cmd != null)
			{
				cmd.mErrorCode = errorCode;
				cmd.mNetState = mNetState;
				cmd.mLastNetState = lastState;
				pushDelayCommand(cmd, this);
			}
		}
	}
}