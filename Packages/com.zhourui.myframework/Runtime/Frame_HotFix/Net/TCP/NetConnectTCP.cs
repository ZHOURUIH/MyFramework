using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using static StringUtility;
using static UnityUtility;
using static SerializeBitUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseUtility;

// 当前程序作为客户端时使用,表示一个与TCP服务器的连接,WebGL无法使用
// 提供双缓冲收发、消息缓冲区、序列号管理和网络状态回调,按bit或byte传输由子类决定
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
	protected int mConnectionGeneration;							// 连接代次,每次主动断开/重新连接都会递增,用于丢弃旧Socket线程/异步回调
	protected bool mManualDisconnect;										// 是否正在主动断开连接
	protected bool mManualSendReceive;										// 是否手动去调用doSend和doReceive
	protected NET_STATE mNetState;											// 网络连接状态
	public virtual void init(IPAddress ip, int port)
	{
		mIPAddress = ip;
		mPort = port;
		if (!mManualSendReceive)
		{
			// 发送线程由MyThread事件模式负责休眠/唤醒。
			// 接收线程同样使用事件模式:连接成功时只唤醒一次,之后阻塞在Socket.Receive等待网络数据。
			mSendThread.setBackground(false);
			mSendThread.startEvent(sendThread);
			mReceiveThread.startEvent(receiveThread);
		}
		// 每2秒发出一个ping包
		mPingTimer.init(0.0f, 2.0f, false);
		mPingTimer.setEnsureInterval(true);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		// Receive线程可能阻塞在Socket.Receive,先让当前连接代次失效,再关闭Socket解除阻塞。
		// 旧Receive/Send/BeginConnect回调即使稍后才返回,也不能再影响下一次连接。
		mManualDisconnect = true;
		Interlocked.Increment(ref mConnectionGeneration);
		clearSocket();
		mSendThread.stop();
		mReceiveThread.stop();
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
		mSocket = null;
		mRecvBuff.setAllDefault();
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
	public int getConnectionGeneration()							{ return Volatile.Read(ref mConnectionGeneration); }
	public bool isConnectionGenerationCurrent(int generation)		{ return generation == Volatile.Read(ref mConnectionGeneration); }
	public void startConnect(Action<bool> callback)
	{
		if (isConnected() || isConnecting())
		{
			callback?.Invoke(false);
			return;
		}

		// 新连接开始前切换到新的连接代次。
		// disconnect关闭旧Socket后,旧Receive线程可能还没从Socket.Receive返回;
		// 旧线程后续收到Interrupted/OperationAborted时必须被识别为旧连接事件。
		int connectionGeneration = Interlocked.Increment(ref mConnectionGeneration);
		mManualDisconnect = false;
		notifyNetState(NET_STATE.CONNECTING, SocketError.Success, connectionGeneration);

		// 创建Socket。异步连接回调必须绑定本次创建的Socket,
		// 不能在回调里访问可能已经被下一次重连替换的mSocket。
		Socket connectSocket;
		using (new ThreadLockScope(mSocketLock))
		{
			if (!isConnectionGenerationCurrent(connectionGeneration))
			{
				callback?.Invoke(false);
				return;
			}
			if (mSocket != null)
			{
				callback?.Invoke(false);
				logError("当前Socket不为空");
				return;
			}
			connectSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			connectSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
			mSocket = connectSocket;
		}

		if (isDevOrEditor())
		{
			log("开始连接服务器:" + mIPAddress + ", Generation:" + connectionGeneration);
		}

		connectSocket.BeginConnect(mIPAddress, mPort, (IAsyncResult ar) =>
		{
			Socket callbackSocket = ar.AsyncState as Socket;
			try
			{
				callbackSocket?.EndConnect(ar);
			}
			catch (ObjectDisposedException)
			{
				// 主动切服/断开时旧Socket被Dispose后回调仍可能晚到。
				if (isDevOrEditor() && !isCurrentConnection(callbackSocket, connectionGeneration))
				{
					log("忽略旧TCP连接BeginConnect回调(ObjectDisposed), Generation:" + connectionGeneration +
						", CurrentGeneration:" + getConnectionGeneration(), LOG_LEVEL.LOW);
				}
				return;
			}
			catch (SocketException e)
			{
				// 旧连接的迟到异常不能影响已经建立/正在建立的新连接。
				if (!isCurrentConnection(callbackSocket, connectionGeneration))
				{
					if (isDevOrEditor())
					{
						log("忽略旧TCP连接BeginConnect异常:" + e.SocketErrorCode +
							", Generation:" + connectionGeneration +
							", CurrentGeneration:" + getConnectionGeneration(), LOG_LEVEL.LOW);
					}
					return;
				}
				delayCall(callback, false);
				log("init socket exception : " + e.Message);
				socketException(e, callbackSocket, connectionGeneration);
				return;
			}

			// EndConnect成功时也必须再次确认这还是当前连接。
			if (!isCurrentConnection(callbackSocket, connectionGeneration) || !callbackSocket.Connected)
			{
				if (isDevOrEditor())
				{
					log("忽略旧TCP连接成功回调, Generation:" + connectionGeneration +
						", CurrentGeneration:" + getConnectionGeneration(), LOG_LEVEL.LOW);
				}
				return;
			}

			notifyNetState(NET_STATE.CONNECTED, SocketError.Success, connectionGeneration, callbackSocket);
			if (!isCurrentConnection(callbackSocket, connectionGeneration))
			{
				return;
			}
			log("连接服务器成功, Generation:" + connectionGeneration);
			delayCall(callback, true);
		}, connectSocket);
	}
	public void disconnect()
	{
		mManualDisconnect = true;
		// 先使旧连接代次失效。旧Receive线程可能在clearSocket之后才从阻塞Receive返回,
		// 即使下一次startConnect已经把mManualDisconnect重新设为false,旧线程也不能再影响新连接。
		int connectionGeneration = Interlocked.Increment(ref mConnectionGeneration);
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
		// 主动关闭时,网络状态应该是无状态。
		// 如果同一帧马上startConnect,这个NONE命令会因为Generation过期而在主线程被忽略。
		notifyNetState(NET_STATE.NONE, SocketError.Success, connectionGeneration);
	}
	public virtual void update(float elapsedTime)
	{
		if (mNetState == NET_STATE.CONNECTED && mPingCallback != null && mPingTimer.tickTimer(elapsedTime))
		{
			mPingStartTime = DateTime.Now;
			mPingCallback.Invoke();
		}
		// 解析所有已经收到的消息包。
		// 单包异常时必须回收当前Packet和当前/剩余原始byte[],否则DoubleBufferReader.Dispose只会Clear列表,不会归还数组池。
		using var a = new DoubleBufferReader<PacketReceiveInfo>(mReceiveBuffer);
		List<PacketReceiveInfo> readList = a.mReadList;
		if (readList == null)
		{
			return;
		}
		int readCount = readList.Count;
		for (int i = 0; i < readCount; ++i)
		{
			PacketReceiveInfo info = readList[i];
			NetPacket packet = null;
			bool packetFailed = false;
			try
			{
				packet = parsePacket(info.mType, info.mPacketData, info.mPacketSize, info.mSequence, info.mFieldFlag, info.mHasSign);
				if (packet != null)
				{
					using var b = new ProfilerScope(packet.GetType().Name);
					packet.execute();
				}
			}
			catch (Exception e)
			{
				packetFailed = true;
				logException(e, "socket packet error, packetType:" + info.mType + ", sequence:" + info.mSequence + ", packetSize:" + info.mPacketSize);
			}
			finally
			{
				UN_ARRAY_BYTE_THREAD(info.mPacketData);
				if (packet != null)
				{
					mNetPacketFactory.destroyPacket(packet);
				}
			}
			if (!packetFailed)
			{
				continue;
			}
			// 保持旧行为:当前批次遇到执行异常后不再继续执行后续消息。
			// 但后续消息的原始buffer必须全部归还数组池,避免异常后产生额外内存泄漏。
			for (int j = i + 1; j < readCount; ++j)
			{
				PacketReceiveInfo remainInfo = readList[j];
				UN_ARRAY_BYTE_THREAD(remainInfo.mPacketData);
			}
			break;
		}
	}
	public override void destroy()
	{
		base.destroy();
		mManualDisconnect = true;
		Interlocked.Increment(ref mConnectionGeneration);
		// 先关闭Socket解除Receive阻塞,再销毁线程。
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
	// 有新的待发送数据时立即唤醒事件发送线程。
	protected void notifySendPending() { mSendThread.wake(); }
	public void notifyReceivePing()
	{
		mPing = (int)(DateTime.Now - mPingStartTime).TotalMilliseconds;
		mPingTimer.start();
	}
	public int getPing() { return mPing; }
	public abstract void sendNetPacket(NetPacket packet);
	public NET_STATE getNetState() { return mNetState; }
	protected bool isCurrentConnection(Socket socket, int connectionGeneration)
	{
		return socket != null &&
			connectionGeneration == Volatile.Read(ref mConnectionGeneration) &&
			ReferenceEquals(socket, mSocket);
	}
	// 仅清理指定Socket。旧连接的异常/回调不能误关已经建立的新Socket。
	protected void clearSocket(Socket expectedSocket)
	{
		using (new ThreadLockScope(mSocketLock))
		{
			if (expectedSocket != null && !ReferenceEquals(expectedSocket, mSocket))
			{
				return;
			}
			Socket socket = mSocket;
			if (socket == null)
			{
				return;
			}
			// 先摘掉当前Socket,其它线程随后即可识别它已经过期。
			mSocket = null;
			try
			{
				if (socket.Connected)
				{
					socket.Shutdown(SocketShutdown.Both);
					socket.Disconnect(false);
				}
				socket.Close();
				socket.Dispose();
			}
			catch (Exception e)
			{
				log("关闭连接时异常：" + e.Message);
			}
		}
	}
	public virtual void clearSocket()
	{
		clearSocket(null);
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
		// 缓冲状态全部准备完成后再启动阻塞接收。断线后Receive线程会回到MyThread事件等待,重连后在这里再次唤醒。
		if (!mManualSendReceive)
		{
			mReceiveThread.wake();
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
		// 手动收包模式保持原语义:没有数据时立即返回。
		receiveSocketData(true);
	}
	// 返回true表示当前连接仍然有效,自动接收线程可以继续阻塞等待下一批数据。
	// 返回false表示连接已经结束或当前无数据(手动模式),调用方应结束本轮接收。
	protected bool receiveSocketData(bool checkAvailable)
	{
		// 不能持有mSocketLock执行阻塞Receive,否则clearSocket无法取得锁来关闭Socket并解除阻塞。
		Socket socket = mSocket;
		int connectionGeneration = Volatile.Read(ref mConnectionGeneration);
		if (!isCurrentConnection(socket, connectionGeneration) || !socket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			return false;
		}
		try
		{
			if (checkAvailable && socket.Available == 0)
			{
				return false;
			}
			// 自动接收模式直接阻塞在Receive。Socket关闭后这里会立即返回/抛异常。
			int nRecv = socket.Receive(mRecvBuff);
			// Receive返回时连接可能已经切换。旧连接的数据不能再写入新连接共用的输入缓冲。
			if (!isCurrentConnection(socket, connectionGeneration))
			{
				return false;
			}
			if (nRecv == 0)
			{
				if (!mManualDisconnect && isCurrentConnection(socket, connectionGeneration))
				{
					notifyNetState(NET_STATE.SERVER_ABORT, SocketError.NotConnected, connectionGeneration, socket);
				}
				return false;
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
											out ushort packetType, out int packetSize, out uint sequence, out ulong fieldFlag, out bool hasSign);
					if (result != PARSE_RESULT.SUCCESS)
					{
						if (result == PARSE_RESULT.ERROR)
						{
							debugHistoryPacket();
							mInputBuffer.clear();
						}
						break;
					}
					mReceiveBuffer.add(new(packetData, fieldFlag, packetSize, sequence, packetType, hasSign));

					if (!mInputBuffer.removeData(0, bitCountToByteCount(bitIndex)))
					{
						logError("移除数据失败");
					}
					if (isDevOrEditor())
					{
						string info = "已接收 : " + packetType.IToS() + ", 字节数:" + bitCountToByteCount(bitIndex).IToS();
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
			return true;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
		catch (SocketException e)
		{
			// 主动切服关闭旧Socket后,旧Receive线程可能晚于下一次startConnect才收到Interrupted。
			// mManualDisconnect此时可能已经被新连接改回false,所以必须同时校验Socket和Generation。
			if (!mManualDisconnect && isCurrentConnection(socket, connectionGeneration))
			{
				socketException(e, socket, connectionGeneration);
			}
			else if (isDevOrEditor())
			{
				log("忽略旧TCP接收线程异常:" + e.SocketErrorCode +
					", Generation:" + connectionGeneration +
					", CurrentGeneration:" + getConnectionGeneration(), LOG_LEVEL.LOW);
			}
			return false;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract NetPacket parsePacket(ushort packetType, byte[] buffer, int size, uint sequence, ulong fieldFlag, bool hasSign);
	// 发送Socket消息。等待/唤醒由MyThread负责,这里只处理真正的发送。
	protected void sendThread(ref bool run)
	{
		if (mManualSendReceive)
		{
			return;
		}
		doSend();
	}
	// 接收线程只在连接成功时由MyThread.wake启动一次。启动后持续阻塞Receive,直到连接断开才返回事件等待状态。
	protected void receiveThread(ref bool run)
	{
		if (mManualSendReceive)
		{
			return;
		}
		while (run && !mManualDisconnect && mNetState == NET_STATE.CONNECTED)
		{
			if (!receiveSocketData(false))
			{
				break;
			}
		}
	}
	protected void sendTotalData()
	{
		int allLength = mTotalBuffer.getDataLength();
		if (allLength == 0)
		{
			return;
		}
		Socket socket = mSocket;
		int connectionGeneration = Volatile.Read(ref mConnectionGeneration);
		if (!isCurrentConnection(socket, connectionGeneration))
		{
			mTotalBuffer.clear();
			return;
		}
		try
		{
			byte[] allBytes = mTotalBuffer.getData();
			int allSendCount = 0;
			while (allSendCount < allLength)
			{
				int thisSendCount = socket.Send(allBytes, allSendCount, allLength - allSendCount, SocketFlags.None);
				if (thisSendCount == 0)
				{
					if (!mManualDisconnect && isCurrentConnection(socket, connectionGeneration))
					{
						notifyNetState(NET_STATE.SERVER_ABORT, SocketError.NotConnected, connectionGeneration, socket);
					}
					break;
				}
				else if (thisSendCount < 0)
				{
					if (!mManualDisconnect && isCurrentConnection(socket, connectionGeneration))
					{
						notifyNetState(NET_STATE.SERVER_CLOSE, SocketError.NotConnected, connectionGeneration, socket);
					}
					break;
				}
				allSendCount += thisSendCount;
			}
		}
		catch (ObjectDisposedException) { }
		catch (SocketException e)
		{
			if (!mManualDisconnect && isCurrentConnection(socket, connectionGeneration))
			{
				socketException(e, socket, connectionGeneration);
			}
			else if (isDevOrEditor())
			{
				log("忽略旧TCP发送线程异常:" + e.SocketErrorCode +
					", Generation:" + connectionGeneration +
					", CurrentGeneration:" + getConnectionGeneration(), LOG_LEVEL.LOW);
			}
		}
		mTotalBuffer.clear();
	}
	protected abstract PARSE_RESULT preParsePacket(byte[] buffer, int size, out int bitIndex, out byte[] outPacketData, 
													out ushort packetType, out int packetSize, out uint sequence, out ulong fieldFlag, out bool hasSign);
	protected void debugHistoryPacket()
	{
		using var a = new ClassThreadScope<MyStringBuilder>(out var info);
		info.add("最后接收的消息:\n");
		foreach (string item in mReceivePacketHistory)
		{
			info.add(item, "\n");
		}
		logError(info.ToString());
	}
	protected void socketException(SocketException e, Socket socket, int connectionGeneration)
	{
		if (!isCurrentConnection(socket, connectionGeneration))
		{
			return;
		}
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
		notifyNetState(state, e.SocketErrorCode, connectionGeneration, socket);
	}
	protected void notifyNetState(
		NET_STATE state,
		SocketError errorCode = SocketError.Success,
		int connectionGeneration = -1,
		Socket sourceSocket = null)
	{
		using (new ThreadLockScope(mConnectStateLock))
		{
			int currentGeneration = Volatile.Read(ref mConnectionGeneration);
			int eventGeneration = connectionGeneration >= 0 ? connectionGeneration : currentGeneration;
			// 旧连接事件不能覆盖当前连接状态。
			if (eventGeneration != currentGeneration)
			{
				return;
			}
			if (sourceSocket != null && !ReferenceEquals(sourceSocket, mSocket))
			{
				return;
			}

			NET_STATE lastState = mNetState;
			mNetState = state;
			if (!isConnected() && !isConnecting())
			{
				// 来自Socket线程的终止状态只能关闭它自己的Socket,不能误关下一次连接。
				clearSocket(sourceSocket);
			}
			CMD_DELAY_THREAD(out CmdNetConnectTCPState cmd, LOG_LEVEL.FORCE);
			if (cmd != null)
			{
				cmd.mErrorCode = errorCode;
				cmd.mNetState = mNetState;
				cmd.mLastNetState = lastState;
				cmd.mConnectionGeneration = eventGeneration;
				pushDelayCommand(cmd, this);
			}
		}
	}
}