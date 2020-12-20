using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

// 当前程序作为客户端时使用
public abstract class SocketConnectClient : CommandReceiver, ISocketConnect
{
	protected DoubleBuffer<SocketPacket> mReceiveBuffer;
	protected DoubleBuffer<byte[]> mOutputBuffer;   // 使用双缓冲提高发送消息的效率
	protected Queue<string> mReceivePacketHistory;
	protected AsyncCallback mConnectCallback;
	protected StreamBuffer mInputBuffer;
	protected ThreadLock mConnectStateLock;
	protected IPAddress mIP;
	protected EndPoint mRemoteEndPoint;
	protected MyThread mReceiveThread;
	protected MyThread mSendThread;
	protected MyTimer mHeartBeatTimer;
	protected Socket mServerSocket;
	protected CONNECT_STATE mConnectState;
	protected byte[] mRecvBuff;
	protected byte mHeartBeatTimes;
	protected int mMaxReceiveCount;
	protected int mPort;
	public SocketConnectClient()
	{
		mOutputBuffer = new DoubleBuffer<byte[]>();
		mMaxReceiveCount = 8 * 1024;
		mReceiveBuffer = new DoubleBuffer<SocketPacket>();
		mReceiveThread = new MyThread("SocketReceive");
		mSendThread = new MyThread("SocketSend");
		mRecvBuff = new byte[mMaxReceiveCount];
		mInputBuffer = new StreamBuffer(1024 * 1024);
		mRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
		mConnectStateLock = new ThreadLock();
		mReceivePacketHistory = new Queue<string>();
		mHeartBeatTimer = new MyTimer();
		mConnectState = CONNECT_STATE.NOT_CONNECT;
		mConnectCallback = connectCallback;
	}
	public virtual void init(IPAddress ip, int port, float heartBeatTimeOut)
	{
		mIP = ip;
		mPort = port;
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut, false);
		try
		{
			// 创建socket  
			mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			mServerSocket.Connect(mIP, mPort);
		}
		catch (SocketException e)
		{
			logInfo("init socket exception : " + e.Message, LOG_LEVEL.FORCE);
			mServerSocket.Close();
			mServerSocket = null;
			// 服务器连接失败也要开启接收和发送线程
			mSendThread.start(sendSocket);
			mReceiveThread.start(receiveSocket);
			NET_STATE state = NET_STATE.NET_CLOSE;
			if (e.ErrorCode == 10051)
			{
				state = NET_STATE.NET_CLOSE;
			}
			else if (e.ErrorCode == 10061)
			{
				state = NET_STATE.SERVER_CLOSE;
			}
			setNetState(state);
			return;
		}
		notifyConnectServer();
		mConnectState = CONNECT_STATE.CONNECTED;
		mSendThread.start(sendSocket);
		mReceiveThread.start(receiveSocket);
	}
	protected abstract void setNetState(NET_STATE state);
	public virtual void update(float elapsedTime)
	{
		if (mHeartBeatTimer.tickTimer(elapsedTime))
		{
			heartBeat();
		}
		processInput();
	}
	public override void destroy()
	{
		base.destroy();
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
	}
	// 通知已经与服务器断开连接
	public void notifyDisconnectServer()
	{
		mConnectStateLock.waitForUnlock();
		mConnectState = CONNECT_STATE.NOT_CONNECT;
		mConnectStateLock.unlock();
	}
	// 通知已经与服务器建立连接
	public void notifyConnectServer()
	{
		// 建立连接后将消息列表中残留的消息清空
		mOutputBuffer.clear();
		mConnectStateLock.waitForUnlock();
		mConnectState = CONNECT_STATE.CONNECTED;
		mConnectStateLock.unlock();
		mHeartBeatTimer.start();
	}
	public void clearSocket()
	{
		mServerSocket?.Close();
		mServerSocket = null;
	}
	// 重新连接服务器
	public void reconnectServer(bool clearOldSocket)
	{
		if(mConnectState != CONNECT_STATE.NOT_CONNECT)
		{
			return;
		}
		mConnectStateLock.waitForUnlock();
		mConnectState = CONNECT_STATE.CONNECTING;
		mConnectStateLock.unlock();
		if (clearOldSocket)
		{
			clearSocket();
		}
		if(mServerSocket == null)
		{
			mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}
		mServerSocket.BeginConnect(mIP, mPort, mConnectCallback, null);
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
	public void sendClientPacket(Type type)
	{
		sendClientPacket(mSocketFactory.createSocketPacket(type));
	}
	public void sendClientPacket(SocketPacket packet)
	{
		if (mServerSocket == null || !mServerSocket.Connected || mConnectState != CONNECT_STATE.CONNECTED)
		{
			mSocketFactory.destroyPacket(packet);
			return;
		}
		// 如果不使用int替换ulong时的包体长度
		int maxPacketSize = packet.generateSize(true);
		// 需要先序列化消息,同时获得包体实际的长度
		byte[] bodyBuffer = mBytesPool.newBytes(getGreaterPow2(maxPacketSize));
		int realPacketSize = packet.write(bodyBuffer, 0);
		if (realPacketSize < 0)
		{
			mSocketFactory.destroyPacket(packet);
			mBytesPool.destroyBytes(bodyBuffer);
			logError("消息序列化失败!");
			return;
		}
		// 将消息包中的数据准备好,然后放入发送列表中
		// 开始的2个字节仅用于发送数据长度标记,不会真正发出去
		// 接下来的数据是包头,包头的长度为2~4个字节
		ushort packetSize = (ushort)realPacketSize;

		// 包类型的高2位表示了当前包体是用几个字节存储的
		ushort packetType = packet.getPacketType();
		// 包中实际的包头大小
		int headerSize = sizeof(ushort);
		// 包体长度为0,则包头不包含包体长度,包类型高2位全部设置为0
		if (packetSize == 0)
		{
			headerSize += 0;
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

		byte[] packetData = mBytesPool.newBytes(getGreaterPow2(sizeof(ushort) + headerSize + packetSize));
		int index = 0;
		// 本次消息包的数据长度,因为byte[]本身的长度并不代表要发送的实际的长度,所以将数据长度保存下来
		writeUShort(packetData, ref index, (ushort)(headerSize + packetSize));
		// 消息类型
		writeUShort(packetData, ref index, packetType);
		// 消息长度,按实际消息长度写入长度的字节内容
		if (headerSize - sizeof(ushort) == sizeof(byte))
		{
			writeByte(packetData, ref index, (byte)packetSize);
		}
		else if (headerSize - sizeof(ushort) == sizeof(ushort))
		{
			writeUShort(packetData, ref index, packetSize);
		}
		// 写入包体数据
		writeBytes(packetData, ref index, bodyBuffer, -1, -1, realPacketSize);
		mBytesPool.destroyBytes(bodyBuffer);
		// 添加到写缓冲中
		mOutputBuffer.addToBuffer(packetData);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		logInfo("已发送 : " + packetType + ", 字节数:" + packetSize, LOG_LEVEL.LOW);
#endif
	}
	public CONNECT_STATE getConnectState() { return mConnectState; }
	//---------------------------------------------------------------------------------------------------------------------------------------
	protected abstract bool checkPacketType(int type);
	protected abstract void heartBeat();
	// 处理接收到的所有消息
	protected void processInput()
	{
		// 解析所有已经收到的消息包
		var readList = mReceiveBuffer.getReadList();
		int count = readList.Count;
		for (int i = 0; i < count; ++i)
		{
			readList[i].execute();
		}
		readList.Clear();
	}
	// 发送Socket消息
	protected void sendSocket(ref bool run)
	{
		if (mServerSocket != null && mServerSocket.Connected && mConnectState == CONNECT_STATE.CONNECTED)
		{
			// 获取输出数据的读缓冲区
			var readList = mOutputBuffer.getReadList();
			int count = readList.Count;
			try
			{
				for (int i = 0; i < count; ++i)
				{
					byte[] item = readList[i];
					int temp = 0;
					// dataLength表示item的有效数据长度,包含dataLength本身占的2个字节
					ushort dataLength = (ushort)(readUShort(item, ref temp, out _) + sizeof(ushort));
					int sendedCount = sizeof(ushort);
					while(sendedCount < dataLength)
					{
						sendedCount += mServerSocket.Send(item, sendedCount, dataLength - sendedCount, SocketFlags.None);
					}
				}
			}
			catch (SocketException e)
			{
				// 由于需要保证状态正确,所以此处需要立即调用设置状态
				notifyDisconnectServer();
				// 服务器异常
				NET_STATE state = NET_STATE.NET_CLOSE;
				if (e.ErrorCode == 10051)
				{
					state = NET_STATE.NET_CLOSE;
				}
				else if (e.ErrorCode == 10061)
				{
					state = NET_STATE.SERVER_CLOSE;
				}
				setNetState(state);
			}
			// 回收缓冲区的内存
			for (int i = 0; i < count; ++i)
			{
				mBytesPool.destroyBytes(readList[i]);
			}
			readList.Clear();
		}
	}
	// 接收Socket消息
	protected void receiveSocket(ref bool run)
	{
		if (mServerSocket != null && mServerSocket.Connected && mConnectState == CONNECT_STATE.CONNECTED)
		{
			try
			{
				(mRemoteEndPoint as IPEndPoint).Address = IPAddress.Any;
				(mRemoteEndPoint as IPEndPoint).Port = 0;
				int nRecv = mServerSocket.ReceiveFrom(mRecvBuff, ref mRemoteEndPoint);
				if (nRecv <= 0)
				{
					// 由于需要保证状态正确,所以此处需要立即调用设置状态
					notifyDisconnectServer();
					// 服务器异常
					setNetState(NET_STATE.SERVER_CLOSE);
					return;
				}
				mInputBuffer.addData(mRecvBuff, nRecv);
				// 解析接收到的数据
				while (true)
				{
					PARSE_RESULT parseResult = parsePacket();
					// 数据解析成功,继续解析
					if (parseResult != PARSE_RESULT.SUCCESS)
					{
						break;
					}
				}
			}
			catch (SocketException e)
			{
				// 由于需要保证状态正确,所以此处需要立即调用设置状态
				notifyDisconnectServer();
				// 本地网络异常
				NET_STATE state = NET_STATE.NET_CLOSE;
				if (e.ErrorCode == 10051)
				{
					state = NET_STATE.NET_CLOSE;
				}
				else if (e.ErrorCode == 10061)
				{
					state = NET_STATE.SERVER_CLOSE;
				}
				setNetState(state);
			}
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
		else if(bit0 == 0 && bit1 == 1)
		{
			if(mInputBuffer.getDataLength() < sizeof(ushort) + sizeof(byte))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readByte(mInputBuffer.getData(), ref index, out _);
		}
		// 包体长度使用2个字节表示
		else if(bit0 == 1 && bit1 == 0)
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
			logError("包类型错误:" + type, false);
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
		SocketPacket packetReply = mSocketFactory.createSocketPacket(type);
		packetReply.setConnect(this);
		int readDataCount = packetReply.read(mInputBuffer.getData(), ref index);
		if (packetSize != 0 && readDataCount < 0)
		{
			logError("包解析错误:" + type + ", 实际接收字节数:" + packetSize, false);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.ERROR;
		}
		mReceiveBuffer.addToBuffer(packetReply);
		mInputBuffer.removeData(0, index);
		if (readDataCount != packetSize)
		{
			logError("接收字节数与解析后消息包字节数不一致:" + type + ",接收:" + packetSize + ", 解析:" + readDataCount, false);
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		string info = "已接收 : " + type + ", 字节数:" + readDataCount;
		logInfo(info, LOG_LEVEL.LOW);
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
		string info = "最后接收的消息:\n";
		foreach(var item in mReceivePacketHistory)
		{
			info += item + "\n";
		}
		logError(info, false);
	}
	protected void connectCallback(IAsyncResult ar)
	{
		try
		{
			mServerSocket.EndConnect(ar);
		}
		catch(SocketException e)
		{
			// 由于需要保证状态正确,所以此处需要立即调用设置状态
			notifyDisconnectServer();
			NET_STATE state = NET_STATE.NET_CLOSE;
			if (e.ErrorCode == 10051)
			{
				state = NET_STATE.NET_CLOSE;
			}
			else if (e.ErrorCode == 10061)
			{
				state = NET_STATE.SERVER_CLOSE;
			}
			setNetState(state);
			return;
		}
		setNetState(NET_STATE.CONNECTED);
	}
}