using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

// 当前程序作为客户端时使用
public abstract class NetConnectTCP : NetConnect
{
	protected DoubleBuffer<NetPacketTCP> mReceiveBuffer;		// 在主线程中执行的消息列表
	protected List<NetPacketTCP> mThreadReceiveBuffer;			// 在子线程中执行的消息列表
	protected DoubleBuffer<byte[]> mOutputBuffer;				// 使用双缓冲提高发送消息的效率
	protected Queue<string> mReceivePacketHistory;
	protected HashSet<ushort> mThreadPacketList;				// 标记需要在子线程中执行的消息类型列表
	protected StreamBuffer mInputBuffer;
	protected ThreadLock mConnectStateLock;
	protected ThreadLock mOutputBufferLock;
	protected ThreadLock mSocketLock;
	protected ThreadLock mTimerLock;
	protected IPAddress mIPAddress;
	protected DateTime mPingStartTime;
	protected MyThread mReceiveThread;
	protected MyThread mSendThread;
	protected MyTimer mHeartBeatTimer;
	protected MyTimer mPingTimer;
	protected Action mHeartBeatAction;			// 外部设置的用于发送心跳包的函数
	protected Action mPingCallback;				// 外部设置的用于发送ping包的函数
	protected Socket mSocket;
	protected byte[] mRecvBuff;
	protected long mPing;						// 网络延迟,计算方式是从发出一个ping包到接收到一个回复包的间隔时间
	protected int mMinPacketSize;				// 解析一个消息包所需的最少字节数
	protected int mPort;                        // 服务器端口
	protected bool mManualDisconnect;			// 是否正在主动断开连接
	protected NET_STATE mNetState;
	public NetConnectTCP()
	{
		mThreadReceiveBuffer = new List<NetPacketTCP>();
		mReceiveBuffer = new DoubleBuffer<NetPacketTCP>();
		mOutputBuffer = new DoubleBuffer<byte[]>();
		mThreadPacketList = new HashSet<ushort>();
		mReceiveThread = new MyThread("SocketReceive");
		mSendThread = new MyThread("SocketSend");
		mRecvBuff = new byte[FrameDefine.TCP_RECEIVE_BUFFER];
		mInputBuffer = new StreamBuffer();
		mConnectStateLock = new ThreadLock();
		mOutputBufferLock = new ThreadLock();
		mSocketLock = new ThreadLock();
		mReceivePacketHistory = new Queue<string>();
		mHeartBeatTimer = new MyTimer();
		mTimerLock = new ThreadLock();
		mPingTimer = new MyTimer();
		mNetState = NET_STATE.NONE;
	}
	public virtual void init(IPAddress ip, int port, float heartBeatTimeOut)
	{
		mIPAddress = ip;
		mPort = port;
		mInputBuffer.init(FrameDefine.TCP_INPUT_BUFFER);
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut, false);
		mSendThread.start(sendSocket);
		mReceiveThread.start(receiveSocket);
		// 每2秒发出一个ping包
		mTimerLock.waitForUnlock();
		mPingTimer.init(0.0f, 2.0f, false);
		mTimerLock.unlock();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mThreadReceiveBuffer.Clear();
		mReceiveBuffer.clear();
		mOutputBuffer.clear();
		mReceivePacketHistory.Clear();
		mInputBuffer.clear();
		mConnectStateLock.unlock();
		mOutputBufferLock.unlock();
		mSocketLock.unlock();
		mIPAddress = null;
		mReceiveThread.stop();
		mSendThread.stop();
		mHeartBeatTimer.stop();
		mSocket = null;
		memset(mRecvBuff, (byte)0);
		mPort = 0;
		mManualDisconnect = false;
		mNetState = NET_STATE.NONE;
		mHeartBeatAction = null;
		mPingStartTime = default;
		mPingTimer.stop();
		mTimerLock.unlock();
		Interlocked.Exchange(ref mPing, 0);
		mPingCallback = null;
		mMinPacketSize = 0;
		mThreadPacketList.Clear();
	}
	public bool isUnconnected() { return mNetState != NET_STATE.CONNECTED && mNetState != NET_STATE.CONNECTING; }
	public void startConnect(bool async)
	{
		if (!isUnconnected())
		{
			return;
		}
		mManualDisconnect = false;
		notifyNetState(NET_STATE.CONNECTING);
		// 创建socket
		mSocketLock.waitForUnlock();
		if (mSocket != null)
		{
			logError("当前Socket不为空");
		}
		mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		mSocketLock.unlock();
		logForce("开始连接服务器,IP:" + mIPAddress.ToString() + ", 端口:" + mPort);
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
			logForce("连接服务器成功,IP:" + mIPAddress.ToString() + ", 端口:" + mPort);
			notifyNetState(NET_STATE.CONNECTED);
		}
	}
	public void disconnect()
	{
		mManualDisconnect = true;
		clearSocket();
		mHeartBeatTimer.stop(false);
		var readList = mReceiveBuffer.get();
		foreach (var item in readList)
		{
			mSocketFactoryThread.destroyPacket(item);
		}
		mReceiveBuffer.endGet();
		mReceiveBuffer.clear();
		// 主动关闭时,网络状态应该是无状态
		notifyNetState(NET_STATE.NONE);
	}
	public virtual void update(float elapsedTime)
	{
		if (mNetState == NET_STATE.CONNECTED)
		{
			if (mHeartBeatAction != null && mHeartBeatTimer.tickTimer(elapsedTime))
			{
				mHeartBeatAction.Invoke();
			}
			if (mPingCallback != null)
			{
				mTimerLock.waitForUnlock();
				bool timeDone = mPingTimer.tickTimer(elapsedTime);
				mTimerLock.unlock();
				if (timeDone)
				{
					mPingStartTime = DateTime.Now;
					mPingCallback.Invoke();
				}
			}
		}
		// 解析所有已经收到的消息包
		var readList = mReceiveBuffer.get();
		try
		{
			int count = readList.Count;
			for (int i = 0; i < count; ++i)
			{
				// 此处为空的原因未知
				if (readList[i] == null)
				{
					continue;
				}
				readList[i].execute();
				mSocketFactoryThread.destroyPacket(readList[i]);
			}
		}
		catch(Exception e)
		{
			logError("socket packet error:" + e.Message + ", stack:" + e.StackTrace);
		}
		readList.Clear();
		mReceiveBuffer.endGet();
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
	public void setHeartBeatAction(Action callback) { mHeartBeatAction = callback; }
	public void notifyHeartBeatRet()
	{
		mHeartBeatTimer.start();
	}
	// 该函数是在子线程中被调用
	public void notifyReceivePing()
	{
		Interlocked.Exchange(ref mPing, (int)(DateTime.Now - mPingStartTime).TotalMilliseconds);
		mTimerLock.waitForUnlock();
		mPingTimer.start();
		mTimerLock.unlock();
	}
	public int getPing() { return (int)Interlocked.Read(ref mPing); }
	public void sendPacket(Type type)
	{
		sendPacket(mSocketFactory.createSocketPacket(type) as NetPacketTCP);
	}
	public abstract void sendPacket(NetPacketTCP packet);
	public NET_STATE getNetState() { return mNetState; }
	public void clearSocket()
	{
		mSocketLock.waitForUnlock();
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
		mSocketLock.unlock();
	}
	// 由于连接成功操作可能不在主线程,所以只能是外部在主线程通知网络管理器连接成功
	public void notifyConnected()
	{
		// 建立连接后将消息列表中残留的消息清空,双缓冲中的读写列表都要清空
		mOutputBufferLock.waitForUnlock();
		var bufferList = mOutputBuffer.getBufferList();
		for(int i = 0; i < bufferList.Length; ++i)
		{
			var buffer = bufferList[i];
			int count = buffer.Count;
			for(int j = 0; j < count; ++j)
			{
				UN_ARRAY_THREAD(buffer[j]);
			}
		}
		mOutputBuffer.clear();
		mOutputBufferLock.unlock();
		// 开始心跳计时
		mHeartBeatTimer.start();
		mInputBuffer.clear();
	}
	// 标记指定类型的消息在子线程执行
	public void addExecuteThreadPacket(ushort packetType) { mThreadPacketList.Add(packetType); }
	//------------------------------------------------------------------------------------------------------------------------------
	// 发送Socket消息
	protected void sendSocket(BOOL run)
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			return;
		}
		// 获取输出数据的读缓冲区
		mOutputBufferLock.waitForUnlock();
		var readList = mOutputBuffer.get();
		int count = readList.Count;
		try
		{
			for (int i = 0; i < count; ++i)
			{
				byte[] item = readList[i];
				// 此处为空的原因未知
				if(item == null)
				{
					continue;
				}
				int temp = 0;
				// dataLength表示item的有效数据长度,包含dataLength本身占的4个字节
				int dataLength = readInt(item, ref temp, out _) + sizeof(int);
				int sendedCount = sizeof(int);
				while (sendedCount < dataLength)
				{
					int thisSendCount = mSocket.Send(item, sendedCount, dataLength - sendedCount, SocketFlags.None);
					if (thisSendCount <= 0)
					{
						// 服务器异常
						notifyNetState(NET_STATE.SERVER_CLOSE, SocketError.NotConnected);
						i = count;
						break;
					}
					sendedCount += thisSendCount;
				}
			}
		}
		catch (SocketException e)
		{
			socketException(e);
		}
		// 回收缓冲区的内存
		count = readList.Count;
		for (int i = 0; i < count; ++i)
		{
			UN_ARRAY_THREAD(readList[i]);
		}
		readList.Clear();
		mOutputBuffer.endGet();
		mOutputBufferLock.unlock();
	}
	// 接收Socket消息
	protected void receiveSocket(BOOL run)
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
			mInputBuffer.addData(mRecvBuff, nRecv);
			// 解析接收到的数据
			while (parsePacket() == PARSE_RESULT.SUCCESS) { }
		}
		catch (SocketException e)
		{
			socketException(e);
		}
	}
	protected abstract PARSE_RESULT packetRead(byte[] buffer, int size, ref int index, out NetPacketTCP packet);
	protected PARSE_RESULT parsePacket()
	{
		// 可能还没有接收完全,等待下次接收
		if (mInputBuffer.getDataLength() < mMinPacketSize)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}

		int index = 0;
		PARSE_RESULT result = packetRead(mInputBuffer.getData(), mInputBuffer.getDataLength(), ref index, out NetPacketTCP packet);
		if (result == PARSE_RESULT.NOT_ENOUGH)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (result == PARSE_RESULT.ERROR)
		{
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.ERROR;
		}
		
		// 主线程执行的消息则放入消息缓冲区
		if(!mThreadPacketList.Contains(packet.getPacketType()))
		{
			mReceiveBuffer.add(packet);
		}
		// 子线程的消息则直接执行
		else
		{
			packet.execute();
			mSocketFactoryThread.destroyPacket(packet);
		}
		mInputBuffer.removeData(0, index);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		string info = "已接收 : " + IToS(packet.getPacketType()) + ", 字节数:" + IToS(index);
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
		MyStringBuilder info = STRING_THREAD("最后接收的消息:\n");
		foreach (var item in mReceivePacketHistory)
		{
			info.Append(item, "\n");
		}
		logError(END_STRING_THREAD(info));
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
		logForce("连接服务器成功,IP:" + mIPAddress.ToString() + ", 端口:" + mPort);
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
		mConnectStateLock.waitForUnlock();
		mNetState = state;
		if (isUnconnected())
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
		mConnectStateLock.unlock();
	}
}