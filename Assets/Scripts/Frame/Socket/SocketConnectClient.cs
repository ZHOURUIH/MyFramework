using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

// 当前程序作为客户端时使用
public abstract class SocketConnectClient : CommandReceiver, ISocketConnect
{
	protected DoubleBuffer<SocketPacket> mReceiveBuffer;		// 在主线程中执行的消息列表
	protected List<SocketPacket> mThreadReceiveBuffer;			// 在子线程中执行的消息列表
	protected DoubleBuffer<byte[]> mOutputBuffer;				// 使用双缓冲提高发送消息的效率
	protected Queue<string> mReceivePacketHistory;
	protected HashSet<ushort> mThreadPacketList;				// 标记需要在子线程中执行的消息类型列表
	protected StreamBuffer mInputBuffer;
	protected ThreadLock mConnectStateLock;
	protected ThreadLock mSocketLock;
	protected IPAddress mIPAddress;
	protected MyThread mReceiveThread;
	protected MyThread mSendThread;
	protected MyTimer mHeartBeatTimer;
	protected Socket mSocket;
	protected byte[] mRecvBuff;
	protected byte mHeartBeatTimes;
	protected int mPort;
	protected bool mConnectDestroy;
	protected NET_STATE mNetState;
	public SocketConnectClient()
	{
		mThreadReceiveBuffer = new List<SocketPacket>();
		mReceiveBuffer = new DoubleBuffer<SocketPacket>();
		mOutputBuffer = new DoubleBuffer<byte[]>();
		mThreadPacketList = new HashSet<ushort>();
		mReceiveThread = new MyThread("SocketReceive");
		mSendThread = new MyThread("SocketSend");
		mRecvBuff = new byte[8 * 1024];
		mInputBuffer = new StreamBuffer(1024 * 1024);
		mConnectStateLock = new ThreadLock();
		mSocketLock = new ThreadLock();
		mReceivePacketHistory = new Queue<string>();
		mHeartBeatTimer = new MyTimer();
		mNetState = NET_STATE.NONE;
	}
	public virtual void init(IPAddress ip, int port, float heartBeatTimeOut)
	{
		mIPAddress = ip;
		mPort = port;
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut, false);
		mSendThread.start(sendSocket);
		mReceiveThread.start(receiveSocket);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mThreadReceiveBuffer.Clear();
		mReceiveBuffer.clear();
		mOutputBuffer.clear();
		mReceivePacketHistory.Clear();
		mInputBuffer.clear();
		// mConnectStateLock,mSocketLock不需要重置
		//mConnectStateLock;
		//mSocketLock;
		mIPAddress = null;
		mReceiveThread.stop();
		mSendThread.stop();
		mHeartBeatTimer.stop();
		mSocket = null;
		memset(mRecvBuff, (byte)0);
		mHeartBeatTimes = 0;
		mPort = 0;
		mConnectDestroy = false;
		mNetState = NET_STATE.NONE;
	}
	public bool isUnconnected() { return mNetState != NET_STATE.CONNECTED && mNetState != NET_STATE.CONNECTING; }
	public void startConnect(bool async)
	{
		if (!isUnconnected())
		{
			return;
		}
		notifyNetState(NET_STATE.CONNECTING);
		// 创建socket
		mSocketLock.waitForUnlock();
		if (mSocket != null)
		{
			logError("当前Socket不为空");
		}
		mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		mSocketLock.unlock();
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
				log("init socket exception : " + e.Message, LOG_LEVEL.FORCE);
				socketException(e);
				return;
			}
			notifyNetState(NET_STATE.CONNECTED);
		}
	}
	public virtual void update(float elapsedTime)
	{
		if (mHeartBeatTimer.tickTimer(elapsedTime))
		{
			heartBeat();
		}
		// 解析所有已经收到的消息包
		var readList = mReceiveBuffer.get();
		int count = readList.Count;
		for (int i = 0; i < count; ++i)
		{
			// 此处为空的原因未知
			if(readList[i] == null)
			{
				continue;
			}
			readList[i].execute();
			mSocketFactoryThread.destroyPacket(readList[i]);
		}
		readList.Clear();
	}
	public override void destroy()
	{
		base.destroy();
		mConnectDestroy = true;
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
	}
	public void notifyHeartBeatRet(byte heartBeatTimes)
	{
		if (heartBeatTimes != mHeartBeatTimes)
		{
			logError("心跳错误!");
			return;
		}
		mHeartBeatTimer.start();
	}
	public void sendPacket(Type type)
	{
		sendPacket(mSocketFactory.createSocketPacket(type));
	}
	public void sendPacket(SocketPacket packet)
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			mSocketFactory.destroyPacket(packet);
			return;
		}
		// 需要先序列化消息,同时获得包体实际的长度,提前获取包类型
		// 包类型的高2位表示了当前包体是用几个字节存储的
		ushort packetType = packet.getPacketType();
		ARRAY_MAIN(out byte[] bodyBuffer, getGreaterPow2(packet.generateSize(true)));
		int realPacketSize = packet.write(bodyBuffer);
		// 序列化完以后立即销毁消息包
		mSocketFactory.destroyPacket(packet);

		if (realPacketSize < 0)
		{
			UN_ARRAY_MAIN(bodyBuffer);
			logError("消息序列化失败!");
			return;
		}

		// 将消息包中的数据准备好,然后放入发送列表中
		// 开始的2个字节仅用于发送数据长度标记,不会真正发出去
		// 接下来的数据是包头,包头的长度为2~4个字节
		ushort packetSize = (ushort)realPacketSize;
		// 包中实际的包头大小
		int headerSize = sizeof(ushort);
		// 包体长度为0,则包头不包含包体长度,包类型高2位全部设置为0
		if (packetSize == 0)
		{
			setBit(ref packetType, sizeof(ushort) * 8 - 1, 0);
			setBit(ref packetType, sizeof(ushort) * 8 - 2, 0);
		}
		// 包体长度可以使用1个字节表示,则包体长度使用1个字节表示
		else if (packetSize <= 0xFF)
		{
			headerSize += sizeof(byte);
			setBit(ref packetType, sizeof(ushort) * 8 - 1, 0);
			setBit(ref packetType, sizeof(ushort) * 8 - 2, 1);
		}
		// 否则包体长度使用2个字节表示
		else
		{
			headerSize += sizeof(ushort);
			setBit(ref packetType, sizeof(ushort) * 8 - 1, 1);
			setBit(ref packetType, sizeof(ushort) * 8 - 2, 0);
		}
		ARRAY_MAIN_THREAD(out byte[] packetData, getGreaterPow2(sizeof(ushort) + headerSize + packetSize));
		int index = 0;
		// 本次消息包的数据长度,因为byte[]本身的长度并不代表要发送的实际的长度,所以将数据长度保存下来
		writeUShort(packetData, ref index, (ushort)(headerSize + packetSize));
		// 消息类型
		writeUShort(packetData, ref index, packetType);
		// 消息长度,按实际消息长度写入长度的字节内容
		if (headerSize == sizeof(ushort) + sizeof(byte))
		{
			writeByte(packetData, ref index, (byte)packetSize);
		}
		else if (headerSize == sizeof(ushort) + sizeof(ushort))
		{
			writeUShort(packetData, ref index, packetSize);
		}
		// 写入包体数据
		writeBytes(packetData, ref index, bodyBuffer, -1, -1, realPacketSize);
		UN_ARRAY_MAIN(bodyBuffer);
		// 添加到写缓冲中
		mOutputBuffer.add(packetData);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		log("已发送 : " + IToS(packet.getPacketType()) + ", 字节数:" + IToS(packetSize), LOG_LEVEL.LOW);
#endif
	}
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
	public void notifyConnected()
	{
		// 建立连接后将消息列表中残留的消息清空
		mOutputBuffer.clear();
		// 开始心跳计时
		mHeartBeatTimer.start();
		mHeartBeatTimes = 0;
		mInputBuffer.clear();
	}
	// 标记指定类型的消息在子线程执行
	public void addExecuteThreadPacket(ushort packetType) { mThreadPacketList.Add(packetType); }
	//---------------------------------------------------------------------------------------------------------------------------------------
	protected abstract bool checkPacketType(int type);
	protected abstract void heartBeat();
	// 发送Socket消息
	protected void sendSocket(BOOL run)
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			return;
		}
		// 获取输出数据的读缓冲区
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
				// dataLength表示item的有效数据长度,包含dataLength本身占的2个字节
				ushort dataLength = (ushort)(readUShort(item, ref temp, out _) + sizeof(ushort));
				int sendedCount = sizeof(ushort);
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
			UN_ARRAY_MAIN_THREAD(readList[i]);
		}
		readList.Clear();
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
	protected PARSE_RESULT parsePacket()
	{
		// 可能还没有接收完全,等待下次接收
		if (mInputBuffer.getDataLength() < sizeof(ushort))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		int index = 0;
		ushort type = readUShort(mInputBuffer.getData(), ref index, out _);
		// 检查包头是否完整
		int bit0 = getBit(type, sizeof(ushort) * 8 - 1);
		int bit1 = getBit(type, sizeof(ushort) * 8 - 2);

		// 获得包体长度
		ushort packetSize = 0;
		// 包体长度为0
		if (bit0 == 0 && bit1 == 0)
		{
			packetSize = 0;
		}
		// 包体长度使用1个字节表示
		else if (bit0 == 0 && bit1 == 1)
		{
			if (mInputBuffer.getDataLength() < sizeof(ushort) + sizeof(byte))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readByte(mInputBuffer.getData(), ref index, out _);
		}
		// 包体长度使用2个字节表示
		else if (bit0 == 1 && bit1 == 0)
		{
			if (mInputBuffer.getDataLength() < sizeof(ushort) + sizeof(ushort))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readUShort(mInputBuffer.getData(), ref index, out _);
		}

		// 还原包类型
		setBit(ref type, sizeof(ushort) * 8 - 1, 0);
		setBit(ref type, sizeof(ushort) * 8 - 2, 0);
		// 客户端接收到的必须是SC类型的
		if (checkPacketType(type))
		{
			logError("包类型错误:" + type);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.ERROR;
		}

		// 验证包长度是否正确
		// 未接收完全,等待下次接收
		if (mInputBuffer.getDataLength() < index + packetSize)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}

		// 创建对应的消息包,并设置数据,然后放入列表中等待解析
		SocketPacket packetReply = mSocketFactoryThread.createSocketPacket(type);
		packetReply.setConnect(this);
		int readDataCount = packetReply.read(mInputBuffer.getData(), ref index);
		if (packetSize != 0 && readDataCount < 0)
		{
			logError("包解析错误:" + type + ", 实际接收字节数:" + packetSize);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.ERROR;
		}
		// 主线程执行的消息则放入消息缓冲区
		if(!mThreadPacketList.Contains(packetReply.getPacketType()))
		{
			mReceiveBuffer.add(packetReply);
		}
		// 子线程的消息则直接执行
		else
		{
			packetReply.execute();
			mSocketFactoryThread.destroyPacket(packetReply);
		}
		mInputBuffer.removeData(0, index);
		if (readDataCount != packetSize)
		{
			logError("接收字节数与解析后消息包字节数不一致:" + type + ",接收:" + packetSize + ", 解析:" + readDataCount);
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		string info = "已接收 : " + IToS(type) + ", 字节数:" + IToS(readDataCount);
		//log(info, LOG_LEVEL.LOW);
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
			log("init socket exception : " + e.Message, LOG_LEVEL.FORCE);
			socketException(e);
			return;
		}
		// 建立连接后将消息列表中残留的消息清空
		mOutputBuffer.clear();
		// 开始心跳计时
		mHeartBeatTimer.start();
		mHeartBeatTimes = 0;
		mInputBuffer.clear();
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
		if(!mConnectDestroy)
		{
			CMD_MAIN_DELAY(out CmdSocketConnectClientState cmd, false);
			if (cmd != null)
			{
				cmd.mErrorCode = errorCode;
				pushDelayCommand(cmd, this);
			}
		}
		mConnectStateLock.unlock();
	}
}