using System.Net.Sockets;

// 当前程序作为服务器时,NetClient代表一个连接到服务器的客户端
public abstract class NetClient : FrameBase
{
	protected DoubleBuffer<byte[]> mOutputBuffer;   // 使用双缓冲提高发送消息的效率
	protected DoubleBuffer<SocketPacket> mReceiveBuffer;
	protected SocketConnectServer mServer;
	protected StreamBuffer mRecvBytes;
	protected MyTimer mHeartBeatTimer;
	protected Socket mSocket;
	protected string mIP;
	protected string mDeadReason;
	protected float mConnectTime;                   // 客户端连接到服务器的时间,秒
	protected uint mClientGUID;
	protected uint mCharacterGUID;
	protected bool mDeadClient;						// 该客户端是否已经断开连接或者心跳超时
	public NetClient(SocketConnectServer server)
	{
		mServer = server;
		mHeartBeatTimer = new MyTimer();
		mReceiveBuffer = new DoubleBuffer<SocketPacket>();
		mRecvBytes = new StreamBuffer(1024 * 1024); // 默认开1MB的缓存
		mOutputBuffer = new DoubleBuffer<byte[]>();
	}
	public void init(Socket socket, uint guid, string ip)
	{
		mHeartBeatTimer.init(-1.0f, 30.0f, false);
		mSocket = socket;
		mClientGUID = guid;
		mIP = ip;
	}
	public virtual void destroy(){}
	public void sendServerPacket(SocketPacket packet)
	{
		// 包类型的高2位表示了当前包体是用几个字节存储的
		ushort packetType = packet.getPacketType();
		// 需要先序列化消息,同时获得包体实际的长度
		ARRAY(out byte[] bodyBuffer, getGreaterPow2(packet.generateSize(true)));
		int realPacketSize = packet.write(bodyBuffer);
		string debugInfo = null;
		if (packet.showInfo())
		{
			debugInfo = packet.debugInfo();
		}
		// 序列化完以后就可以直接销毁消息
		mSocketFactory.destroyPacket(packet);

		if (realPacketSize < 0)
		{
			UN_ARRAY(bodyBuffer);
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
		else if(packetSize <= 0xFF)
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
		ARRAY_THREAD(out byte[] packetData, getGreaterPow2(sizeof(ushort) + headerSize + packetSize));
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
		else if(headerSize == sizeof(ushort) + sizeof(ushort))
		{
			writeUShort(packetData, ref index, packetSize);
		}
		// 写入包体数据
		writeBytes(packetData, ref index, bodyBuffer, -1, -1, realPacketSize);
		UN_ARRAY(bodyBuffer);
		// 添加到写缓冲中
		mOutputBuffer.add(packetData);
		if (debugInfo != null)
		{
			clientInfo("已发送 : " + debugInfo + ", 字节数:" + IToS(packetSize));
		}
	}
	public void processSend()
	{
		// 获取输出数据的读缓冲区
		var readList = mOutputBuffer.get();
		int count = readList.Count;
		try
		{
			for(int i = 0; i < count; ++i)
			{
				var item = readList[i];
				int temp = 0;
				// dataLength表示item的有效数据长度,包含dataLength本身占的2个字节
				ushort dataLength = (ushort)(readUShort(item, ref temp, out _) + sizeof(ushort));
				int sendedCount = sizeof(ushort);
				while (sendedCount < dataLength)
				{
					sendedCount += mSocket.Send(item, sendedCount, dataLength - sendedCount, SocketFlags.None);
				}
			}
		}
		catch
		{
			log("发送数据到客户端时发现异常");
		}
		// 回收所有byte[]
		for (int i = 0; i < count; ++i)
		{
			UN_ARRAY_THREAD(readList[i]);
		}
		readList.Clear();
		mOutputBuffer.endGet();
	}
	public void recvData(byte[] data, int count)
	{
		if (data == null || count <= 0)
		{
			mDeadClient = true;
			return;
		}
		mRecvBytes.addData(data, count);
		while (true)
		{
			// 数据解析成功,继续解析,否则不再解析
			if (parsePacket() != PARSE_RESULT.SUCCESS)
			{
				break;
			}
		}
	}
	public void update(float elapsedTime)
	{
		if (mDeadClient)
		{
			return;
		}
		// 执行所有已经收到的消息包
		var readList = mReceiveBuffer.get();
		int count = readList.Count;
		for (int i = 0; i < count; ++i)
		{
			SocketPacket packet = readList[i];
			if (packet.showInfo())
			{
				clientInfo("type:" + IToS(packet.getPacketType()) + ", " + packet.debugInfo());
			}
			packet.execute();
			destroyPacket(packet);
		}
		readList.Clear();
		mReceiveBuffer.endGet();
		mConnectTime += elapsedTime;
		// 判断客户端心跳是否超时
		if (mHeartBeatTimer.tickTimer(elapsedTime))
		{
			mDeadClient = true;
		}
	}
	public void notifyPlayerLogin(uint guid) { mCharacterGUID = guid; }
	public uint getCharacterGUID() { return mCharacterGUID; }
	public bool isDeadClient() { return mDeadClient; }
	public void notifyReceiveHeartBeat() { mHeartBeatTimer.start(); }
	public uint getClientGUID() { return mClientGUID; }
	public Socket getSocket() { return mSocket; }
	public void setDeadClient(string reason) { mDeadClient = true; mDeadReason = reason; }
	public string getDeadReason() { return mDeadReason; }
	//-----------------------------------------------------------------------------------------------------------------------------------
	protected abstract bool checkPacketType(int type);
	protected abstract SocketPacket createClientPacket(int type);
	protected abstract void destroyPacket(SocketPacket packet);
	protected PARSE_RESULT parsePacket()
	{
		if (mRecvBytes.getDataLength() < sizeof(ushort))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		int index = 0;
		// 读取包类型
		ushort type = readUShort(mRecvBytes.getData(), ref index, out _);
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
			if (mRecvBytes.getDataLength() < sizeof(ushort) + sizeof(byte))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readByte(mRecvBytes.getData(), ref index, out _);
		}
		// 包体长度使用2个字节表示
		else if (bit0 == 1 && bit1 == 0)
		{
			if (mRecvBytes.getDataLength() < sizeof(ushort) + sizeof(ushort))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readUShort(mRecvBytes.getData(), ref index, out _);
		}

		// 还原包类型
		setBit(ref type, sizeof(ushort) * 8 - 1, 0);
		setBit(ref type, sizeof(ushort) * 8 - 2, 0);

		// 客户端接收到的必须是SC类型的
		if (!checkPacketType(type))
		{
			clientError("包类型错误 : " + type);
			// 发生错误时,清空缓冲区
			mRecvBytes.clear();
			return PARSE_RESULT.ERROR;
		}
		// 是否已经接收完全
		if (mRecvBytes.getDataLength() < index + packetSize)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		SocketPacket packetReply = createClientPacket(type);
		packetReply.setConnect(mServer);
		packetReply.setClient(this);
		packetReply.setClientID(mClientGUID);
		if (packetSize != 0 && packetReply.read(mRecvBytes.getData(), ref index) < 0)
		{
			clientError("解析错误:" + type + ", 实际接收字节数:" + packetSize);
			// 发生错误时,清空缓冲区
			mRecvBytes.clear();
			return PARSE_RESULT.ERROR;
		}
		mReceiveBuffer.add(packetReply);
		// 移除已解析的数据
		mRecvBytes.removeData(0, index);
		return PARSE_RESULT.SUCCESS;
	}
	protected void clientInfo(string info)
	{
		Character player = mCharacterManager.getCharacter(mCharacterGUID);
		log(strcat("IP:", mIP, ", 角色GUID:", IToS(mCharacterGUID), ", 名字:", player?.getName(), " ||\t", info));
	}
	protected void clientError(string info)
	{
		Character player = mCharacterManager.getCharacter(mCharacterGUID);
		logError(strcat("IP:", mIP, ", 角色GUID:", IToS(mCharacterGUID), ", 名字:", player?.getName(), " ||\t", info));
	}
}