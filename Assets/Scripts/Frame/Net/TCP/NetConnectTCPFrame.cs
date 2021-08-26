using System;
using System.Net;

// Frame层默认的TCP连接封装类,应用层可根据实际需求仿照此类封装自己的TCP连接类
public class NetConnectTCPFrame : NetConnectTCP
{
	public override void init(IPAddress ip, int port, float heartBeatTimeOut)
	{
		base.init(ip, port, heartBeatTimeOut);
		mMinPacketSize = FrameDefine.PACKET_TYPE_SIZE;
	}
	public override void resetProperty()
	{
		base.resetProperty();
	}
	public override void sendPacket(NetPacketTCP packet)
	{
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			mSocketFactory.destroyPacket(packet);
			return;
		}
		// 需要先序列化消息,同时获得包体实际的长度,提前获取包类型
		// 包类型的高2位表示了当前包体是用几个字节存储的
		var tcpPacket = packet as NetPacketTCPFrame;
		ushort packetType = tcpPacket.getPacketType();
		ARRAY(out byte[] bodyBuffer, getGreaterPow2(tcpPacket.generateSize(true)));
		int realPacketSize = tcpPacket.write(bodyBuffer);
		// 序列化完以后立即销毁消息包
		mSocketFactory.destroyPacket(tcpPacket);

		if (realPacketSize < 0)
		{
			UN_ARRAY(bodyBuffer);
			logError("消息序列化失败!");
			return;
		}

		// 将消息包中的数据准备好,然后放入发送列表中
		// 开始的2个字节仅用于发送数据长度标记,不会真正发出去
		// 接下来的数据是包头,包头的长度为2~6个字节
		// 包中实际的包头大小
		int headerSize = FrameDefine.PACKET_TYPE_SIZE;
		// 包体长度为0,则包头不包含包体长度,包类型高2位全部设置为0
		if (realPacketSize == 0)
		{
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 1, 0);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 2, 0);
		}
		// 包体长度可以使用1个字节表示,则包体长度使用1个字节表示
		else if (realPacketSize <= 0xFF)
		{
			headerSize += sizeof(byte);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 1, 0);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 2, 1);
		}
		// 包体长度使用2个字节表示
		else if (realPacketSize <= 0xFFFF)
		{
			headerSize += sizeof(ushort);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 1, 1);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 2, 0);
		}
		// 包体长度使用4个字节表示
		else
		{
			headerSize += sizeof(int);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 1, 1);
			setBit(ref packetType, FrameDefine.PACKET_TYPE_SIZE * 8 - 2, 1);
		}
		ARRAY_THREAD(out byte[] packetData, getGreaterPow2(sizeof(int) + headerSize + realPacketSize));
		int index = 0;
		// 本次消息包的数据长度,因为byte[]本身的长度并不代表要发送的实际的长度,所以将数据长度保存下来
		writeInt(packetData, ref index, headerSize + realPacketSize);
		// 消息类型
		writeUShort(packetData, ref index, packetType);
		// 消息长度,按实际消息长度写入长度的字节内容
		if (headerSize == FrameDefine.PACKET_TYPE_SIZE + sizeof(byte))
		{
			writeByte(packetData, ref index, (byte)realPacketSize);
		}
		else if (headerSize == FrameDefine.PACKET_TYPE_SIZE + sizeof(ushort))
		{
			writeUShort(packetData, ref index, (ushort)realPacketSize);
		}
		else if (headerSize == FrameDefine.PACKET_TYPE_SIZE + sizeof(int))
		{
			writeInt(packetData, ref index, realPacketSize);
		}
		// 写入包体数据
		writeBytes(packetData, ref index, bodyBuffer, -1, -1, realPacketSize);
		UN_ARRAY(bodyBuffer);
		// 添加到写缓冲中
		mOutputBuffer.add(packetData);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override PARSE_RESULT packetRead(byte[] buffer, int size, ref int index, out NetPacketTCP packet)
	{
		packet = null;
		ushort type = readUShort(buffer, ref index, out _);
		int bit0 = getBit(type, FrameDefine.PACKET_TYPE_SIZE * 8 - 1);
		int bit1 = getBit(type, FrameDefine.PACKET_TYPE_SIZE * 8 - 2);

		// 获得包体长度
		int packetSize = 0;
		// 包体长度为0
		if (bit0 == 0 && bit1 == 0)
		{
			packetSize = 0;
		}
		// 包体长度使用1个字节表示
		else if (bit0 == 0 && bit1 == 1)
		{
			if (size < FrameDefine.PACKET_TYPE_SIZE + sizeof(byte))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readByte(buffer, ref index, out _);
		}
		// 包体长度使用2个字节表示
		else if (bit0 == 1 && bit1 == 0)
		{
			if (size < FrameDefine.PACKET_TYPE_SIZE + sizeof(ushort))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readUShort(buffer, ref index, out _);
		}
		// 包体长度使用4个字节表示
		else if (bit0 == 1 && bit1 == 1)
		{
			if (size < FrameDefine.PACKET_TYPE_SIZE + sizeof(int))
			{
				return PARSE_RESULT.NOT_ENOUGH;
			}
			packetSize = readInt(buffer, ref index, out _);
		}

		// 还原包类型
		setBit(ref type, FrameDefine.PACKET_TYPE_SIZE * 8 - 1, 0);
		setBit(ref type, FrameDefine.PACKET_TYPE_SIZE * 8 - 2, 0);
		// 客户端接收到的必须是SC类型的
		if (type <= FrameDefine.SC_MIN || type >= FrameDefine.SC_MAX)
		{
			logError("包类型错误:" + type);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.ERROR;
		}

		// 验证包长度是否正确,未接收完全,等待下次接收
		if (size < index + packetSize)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}

		// 创建对应的消息包,并设置数据,然后放入列表中等待解析
		var packetReply = mSocketFactoryThread.createSocketPacket(type) as NetPacketTCPFrame;
		packet = packetReply;
		packet.setConnect(this);
		int readDataCount = packetReply.read(buffer, ref index);
		if (packetSize != 0 && readDataCount < 0)
		{
			logError("包解析错误:" + type + ", 实际接收字节数:" + packetSize);
			mSocketFactoryThread.destroyPacket(packet);
			return PARSE_RESULT.ERROR;
		}
		if (readDataCount != packetSize)
		{
			logError("接收字节数与解析后消息包字节数不一致:" + type + ",接收:" + packetSize + ", 解析:" + readDataCount);
			mSocketFactoryThread.destroyPacket(packet);
			return PARSE_RESULT.ERROR;
		}
		return PARSE_RESULT.SUCCESS;
	}
}