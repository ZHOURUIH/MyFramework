using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;

public interface IClient
{
	uint getClientGUID();
}

// 当前程序作为服务器时,NetClient代表一个连接到服务器的客户端
public abstract class NetClient : GameBase
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
		// 将消息包中的数据准备好,然后放入发送列表中
		// 开始的2个字节仅用于发送数据长度标记,不会真正发出去
		// 接下来6个字节分别是1个uint和1个ushort,代表消息类型和消息内容长度
		ushort packetSize = (ushort)packet.generateSize();
		byte[] packetData = mBytesPool.newBytes(getGreaterPow2(sizeof(ushort) + FrameDefine.PACKET_HEADER_SIZE + packetSize));
		int index = 0;
		// 本次消息包的数据长度,因为byte[]本身的长度并不代表要发送的实际的长度,所以将数据长度保存下来
		writeUShort(packetData, ref index, (ushort)(FrameDefine.PACKET_HEADER_SIZE + packetSize));
		// 消息类型
		writeInt(packetData, ref index, packet.getPacketType());
		// 消息长度
		writeUShort(packetData, ref index, packetSize);
		// 消息内容
		if (packetSize > 0 && !packet.write(packetData, sizeof(ushort) + FrameDefine.PACKET_HEADER_SIZE))
		{
			clientError("消息序列化失败 : " + packet.getPacketType());
			return;
		}
		// 添加到写缓冲中
		mOutputBuffer.addToBuffer(packetData);
		if (packet.showInfo())
		{
			clientInfo("已发送 : " + packet.debugInfo() + ", 字节数:" + packetSize);
		}
	}
	public void processSend()
	{
		// 获取输出数据的读缓冲区
		var readList = mOutputBuffer.getReadList();
		try
		{
			foreach (var item in readList)
			{
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
			logInfo("发送数据到客户端时发现异常");
		}
		// 回收所有byte[]
		foreach (var item in readList)
		{
			mBytesPool.destroyBytes(item);
		}
		readList.Clear();
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
		var readList = mReceiveBuffer.getReadList();
		foreach (var item in readList)
		{
			if (item.showInfo())
			{
				clientInfo("type:" + item.getPacketType() + ", " + item.debugInfo());
			}
			item.execute();
			destroyPacket(item);
		}
		readList.Clear();
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
		if (mRecvBytes.getDataLength() < FrameDefine.PACKET_HEADER_SIZE)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		int index = 0;
		// 读取包类型
		int type = readInt(mRecvBytes.getData(), ref index, out _);
		// 客户端接收到的必须是SC类型的
		if (!checkPacketType(type))
		{
			clientError("包类型错误 : " + type);
			// 发生错误时,清空缓冲区
			mRecvBytes.clear();
			return PARSE_RESULT.ERROR;
		}
		// 读取消息长度
		ushort packetSize = readUShort(mRecvBytes.getData(), ref index, out _);
		// 是否已经接收完全
		if (mRecvBytes.getDataLength() < index + packetSize)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		SocketPacket packetReply = createClientPacket(type);
		packetReply.setConnect(mServer);
		packetReply.mClient = this;
		packetReply.mClientID = mClientGUID;
		if (packetSize != 0 && !packetReply.read(mRecvBytes.getData(), ref index))
		{
			clientError("解析错误:" + type + ", 实际接收字节数:" + packetSize);
			// 发生错误时,清空缓冲区
			mRecvBytes.clear();
			return PARSE_RESULT.ERROR;
		}
		mReceiveBuffer.addToBuffer(packetReply);
		// 移除已解析的数据
		mRecvBytes.removeData(0, index);
		return PARSE_RESULT.SUCCESS;
	}
	protected void clientInfo(string info)
	{
		Character player = mCharacterManager.getCharacter(mCharacterGUID);
		string name = player != null ? player.getName() : EMPTY_STRING;
		string fullInfo = "IP:" + mIP + ", 角色GUID:" + uintToString(mCharacterGUID) + ", 名字:" + name + " ||\t" + info;
		logInfo(fullInfo);
	}
	protected void clientError(string info)
	{
		Character player = mCharacterManager.getCharacter(mCharacterGUID);
		string name = player != null ? player.getName() : EMPTY_STRING;
		logError("IP:" + mIP + ", 角色GUID:" + uintToString(mCharacterGUID) + ", 名字:" + name + " ||\t" + info);
	}
}