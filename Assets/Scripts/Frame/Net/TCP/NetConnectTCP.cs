using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.Profiling;
using static StringUtility;
#endif
using static UnityUtility;
using static BinaryUtility;
using static FrameUtility;
using static FrameBase;
using static FrameDefine;

public struct OutputDataInfo
{
	public byte[] mData;
	public int mDataSize;
	public OutputDataInfo(byte[] data, int size)
	{
		mData = data;
		mDataSize = size;
	}
}

// 当前程序作为客户端时使用,表示一个与TCP服务器的连接
public abstract class NetConnectTCP : NetConnect
{
	protected DoubleBuffer<PacketSimpleInfo> mReceiveBuffer;// 在主线程中执行的消息列表
	protected DoubleBuffer<OutputDataInfo> mOutputBuffer;	// 使用双缓冲提高发送消息的效率
	protected Queue<string> mReceivePacketHistory;          // 接收过的包的缓冲列表
	protected NetStateCallback mNetStateCallback;			// 网络状态改变的回调
	protected StreamBuffer mInputBuffer;                    // 接收消息的缓冲区
	protected StreamBuffer mTotalBuffer;					// 最终发出的大的缓冲区
	protected ThreadLock mConnectStateLock;					// mNetState的锁
	protected ThreadLock mOutputBufferLock;					// mOutputBuffer的锁
	protected ThreadLock mSocketLock;						// mSocket的锁
	protected IPAddress mIPAddress;							// 服务器地址
	protected DateTime mPingStartTime;						// ping开始的时间
	protected MyThread mReceiveThread;						// 接收线程
	protected MyThread mSendThread;							// 发送线程
	protected MyTimer1 mPingTimer;							// ping计时器
	protected Action mPingCallback;							// 外部设置的用于发送ping包的函数
	protected Socket mSocket;								// 套接字实例
	protected byte[] mRecvBuff;								// 从Socket接收时使用的缓冲区
	protected int mPing;									// 网络延迟,计算方式是从发出一个ping包到接收到一个回复包的间隔时间
	protected int mPort;									// 服务器端口
	protected bool mManualDisconnect;                       // 是否正在主动断开连接
	protected NET_STATE mNetState;							// 网络连接状态
	public NetConnectTCP()
	{
		mReceiveBuffer = new DoubleBuffer<PacketSimpleInfo>();
		mOutputBuffer = new DoubleBuffer<OutputDataInfo>();
		mReceiveThread = new MyThread("SocketReceiveTCP");
		mSendThread = new MyThread("SocketSendTCP");
		mRecvBuff = new byte[TCP_RECEIVE_BUFFER];
		mInputBuffer = new StreamBuffer();
		mTotalBuffer = new StreamBuffer(CLIENT_MAX_PACKET_SIZE * 8);
		mConnectStateLock = new ThreadLock();
		mOutputBufferLock = new ThreadLock();
		mSocketLock = new ThreadLock();
		mReceivePacketHistory = new Queue<string>();
		mPingTimer = new MyTimer1();
		mNetState = NET_STATE.NONE;
	}
	public virtual void init(IPAddress ip, int port)
	{
		mIPAddress = ip;
		mPort = port;
		mInputBuffer.init(TCP_INPUT_BUFFER);
		mSendThread.start(sendThread);
		mReceiveThread.start(receiveThread);
		// 每2秒发出一个ping包
		mPingTimer.init(0.0f, 2.0f, false);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiveBuffer.clear();
		mOutputBuffer.clear();
		mReceivePacketHistory.Clear();
		mInputBuffer.clear();
		//mConnectStateLock.unlock();
		//mOutputBufferLock.unlock();
		//mSocketLock.unlock();
		mIPAddress = null;
		mReceiveThread.stop();
		mSendThread.stop();
		mSocket = null;
		memset(mRecvBuff, (byte)0);
		mPort = 0;
		mManualDisconnect = false;
		mNetState = NET_STATE.NONE;
		mPingStartTime = default;
		mPingTimer.stop();
		mPing = 0;
		mPingCallback = null;
		mNetStateCallback = null;
	}
	public bool isConnected() { return mNetState == NET_STATE.CONNECTED; }
	public bool isConnecting() { return mNetState == NET_STATE.CONNECTING; }
	public void setNetStateCallback(NetStateCallback callback) { mNetStateCallback = callback; }
	public NetStateCallback getNetStateCallback() { return mNetStateCallback; }
	public void startConnect(bool async)
	{
		if (isConnected() || isConnecting())
		{
			return;
		}
		mManualDisconnect = false;
		notifyNetState(NET_STATE.CONNECTING);
		// 创建socket
		using (new ThreadLockScope(mSocketLock))
		{
			if (mSocket != null)
			{
				logError("当前Socket不为空");
				return;
			}
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			mSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		logForce("开始连接服务器:" + mIPAddress);
#endif
		if (async)
		{
			mSocket.BeginConnect(mIPAddress, mPort, onConnectEnd, mSocket);
		}
		else
		{
			try
			{
				mSocket.Connect(mIPAddress, mPort);
			}
			catch (SocketException e)
			{
				logForce("init socket exception : " + e.Message);
				socketException(e);
				return;
			}
			notifyNetState(NET_STATE.CONNECTED);
		}
	}
	public void disconnect()
	{
		mManualDisconnect = true;
		clearSocket();
		mPingTimer.stop(false);
		using (new DoubleBufferReader<PacketSimpleInfo>(mReceiveBuffer, out var readList))
		{
			if (readList != null)
			{
				try
				{
					foreach (var item in readList)
					{
						byte[] data = item.mPacketData;
						UN_ARRAY_THREAD(ref data);
					}
				}
				catch (Exception e)
				{
					logException(e, "使用读列表中错误");
				}
			}
		}
		mReceiveBuffer.clear();
		// 主动关闭时,网络状态应该是无状态
		notifyNetState(NET_STATE.NONE);
	}
	public virtual void update(float elapsedTime)
	{
		if (mNetState == NET_STATE.CONNECTED && mPingCallback != null)
		{
			if (mPingTimer.tickTimer())
			{
				mPingStartTime = DateTime.Now;
				mPingCallback.Invoke();
			}
		}
		// 解析所有已经收到的消息包
		using (new DoubleBufferReader<PacketSimpleInfo>(mReceiveBuffer, out var readList))
		{
			if (readList != null)
			{
				try
				{
					int count = readList.Count;
					for (int i = 0; i < count; ++i)
					{
						PacketSimpleInfo info = readList[i];
						NetPacket packet = parsePacket(info.mType, info.mPacketData, info.mPacketSize, info.mSequence, info.mFieldFlag);
						UN_ARRAY_THREAD(ref info.mPacketData);
						if (packet == null)
						{
							continue;
						}
						(packet as NetPacketFrame).setSequenceValid(true);
						if (packet.canExecute())
						{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
							Profiler.BeginSample(packet.GetType().ToString());
#endif
							packet.execute();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
							Profiler.EndSample();
#endif
						}
						mSocketFactory.destroyPacket(packet);
					}
				}
				catch (Exception e)
				{
					logException(e, "socket packet error");
				}
			}
		}
	}
	public override void destroy()
	{
		base.destroy();
		mManualDisconnect = true;
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
	}
	public void setPort(int port) { mPort = port; }
	public void setIPAddress(IPAddress ip) { mIPAddress = ip; }
	public void setPingAction(Action callback) { mPingCallback = callback; }
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
				logForce("关闭连接时异常：" + e.Message);
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
			var bufferList = mOutputBuffer.getBufferList();
			for (int i = 0; i < bufferList.Length; ++i)
			{
				var list = bufferList[i];
				for (int j = 0; j < list.Count; ++j)
				{
					byte[] data = list[j].mData;
					UN_ARRAY_THREAD(ref data);
				}
			}
			mOutputBuffer.clear();
		}
		// 开始心跳计时
		mPingTimer.start();
		mInputBuffer.clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract NetPacket parsePacket(ushort packetType, byte[] buffer, int size, int sequence, ulong fieldFlag);
	// 发送Socket消息
	protected void sendThread(ref bool run)
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			return;
		}
		mTotalBuffer.clear();
		// 获取输出数据的读缓冲区,手动拼接到大的缓冲区中
		using (new ThreadLockScope(mOutputBufferLock))
		{
			using (new DoubleBufferReader<OutputDataInfo>(mOutputBuffer, out var readList))
			{
				// 获取不到数据,则没有任何数据需要发送,直接返回即可
				if (readList == null)
				{
					return;
				}
				int count = readList.Count;
				for (int i = 0; i < count; ++i)
				{
					OutputDataInfo item = readList[i];
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
					UN_ARRAY_THREAD(ref item.mData);
				}
			}
		}
		sendTotalData();
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
				if (thisSendCount <= 0)
				{
					// 服务器异常
					notifyNetState(NET_STATE.SERVER_CLOSE, SocketError.NotConnected);
					break;
				}
				allSendedCount += thisSendCount;
			}
		}
		catch (SocketException e)
		{
			socketException(e);
		}
		mTotalBuffer.clear();
	}
	// 接收Socket消息
	protected void receiveThread(ref bool run)
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
			if (nRecv <= 0)
			{
				// 服务器异常
				notifyNetState(NET_STATE.SERVER_CLOSE, SocketError.NotConnected);
				return;
			}
			if (!mInputBuffer.addData(mRecvBuff, nRecv))
			{
				logError("添加数据到缓冲区失败!数量:" + nRecv + ",当前缓冲区中数据:" + mInputBuffer.getDataLength() + ",缓冲区大小:" + mInputBuffer.getBufferSize());
			}
			// 解析接收到的数据
			while (parseInputBuffer() == PARSE_RESULT.SUCCESS) { }
		}
		catch (SocketException e)
		{
			socketException(e);
		}
	}
	protected abstract PARSE_RESULT preParsePacket(byte[] buffer, int size, ref int bitIndex, out byte[] outPacketData, 
													out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag);
	protected PARSE_RESULT parseInputBuffer()
	{
		int bitIndex = 0;
		PARSE_RESULT result = preParsePacket(mInputBuffer.getData(), mInputBuffer.getDataLength(), ref bitIndex, out byte[] packetData, 
											out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag);
		if (result != PARSE_RESULT.SUCCESS)
		{
			if (result == PARSE_RESULT.ERROR)
			{
				debugHistoryPacket();
				mInputBuffer.clear();
			}
			return result;
		}
		mReceiveBuffer.add(new PacketSimpleInfo(packetData, fieldFlag, packetSize, sequence, packetType));

		if (!mInputBuffer.removeData(0, bitCountToByteCount(bitIndex)))
		{
			logError("移除数据失败");
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		string info = "已接收 : " + IToS(packetType) + ", 字节数:" + IToS(bitCountToByteCount(bitIndex));
		log(info, LOG_LEVEL.LOW);
		mReceivePacketHistory.Enqueue(info);
		if (mReceivePacketHistory.Count > 10)
		{
			mReceivePacketHistory.Dequeue();
		}
#endif
		return PARSE_RESULT.SUCCESS;
	}
	protected void debugHistoryPacket()
	{
		using (new ClassThreadScope<MyStringBuilder>(out var info))
		{
			info.append("最后接收的消息:\n");
			foreach (var item in mReceivePacketHistory)
			{
				info.append(item, "\n");
			}
			logError(info.ToString());
		}
	}
	protected void onConnectEnd(IAsyncResult ar)
	{
		try
		{
			mSocket.EndConnect(ar);
		}
		catch (SocketException e)
		{
			logForce("init socket exception : " + e.Message);
			socketException(e);
			return;
		}
		logForce("连接服务器成功");
		notifyNetState(NET_STATE.CONNECTED);
	}
	protected void socketException(SocketException e)
	{
		// 本地网络异常
		NET_STATE state = NET_STATE.NET_CLOSE;
		if (e.SocketErrorCode == SocketError.NetworkUnreachable)
		{
			state = NET_STATE.NET_CLOSE;
		}
		else if (e.SocketErrorCode == SocketError.ConnectionRefused)
		{
			state = NET_STATE.SERVER_CLOSE;
		}
		notifyNetState(state, e.SocketErrorCode);
	}
	protected void notifyNetState(NET_STATE state, SocketError errorCode = SocketError.Success)
	{
		using (new ThreadLockScope(mConnectStateLock))
		{
			mNetState = state;
			if (!isConnected() && !isConnecting())
			{
				clearSocket();
			}
			if (!mManualDisconnect)
			{
				CMD_DELAY_THREAD(out CmdNetConnectTCPState cmd);
				if (cmd != null)
				{
					cmd.mErrorCode = errorCode;
					cmd.mNetState = mNetState;
					pushDelayCommand(cmd, this);
				}
			}
		}
	}
}