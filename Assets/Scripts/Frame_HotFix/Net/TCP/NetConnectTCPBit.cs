using static UnityUtility;
using static FrameBase;
using static BinaryUtility;
using static MathUtility;
using static FrameUtility;
using static CSharpUtility;
using static FrameDefine;

// Frame层默认的TCP连接封装类,按bit传输,应用层可根据实际需求仿照此类封装自己的TCP连接类
public class NetConnectTCPBit : NetConnectTCP
{
	protected EncryptPacket mEncryptPacket;				// 加密函数
	protected DecryptPacket mDecryptPacket;				// 解密函数
	protected SerializerBitWrite mBitWriter = new();	// 用于序列化
	protected int mLastReceiveSequenceNumber;			// 上一次接收到的序列号
	protected int mSendSequenceNumber;					// 当前序列号
	public override void resetProperty()
	{
		base.resetProperty();
		mEncryptPacket = null;
		mDecryptPacket = null;
		mBitWriter.clear();
		mLastReceiveSequenceNumber = 0;
		mSendSequenceNumber = 0;
	}
	public override void clearSocket()
	{
		base.clearSocket();
		mLastReceiveSequenceNumber = 0;
		mSendSequenceNumber = 0;
	}
	public void setEncrypt(EncryptPacket encrypt, DecryptPacket decrypt)
	{
		mEncryptPacket = encrypt;
		mDecryptPacket = decrypt;
	}
	public override void sendNetPacket(NetPacket packet)
	{
		if (!isMainThread())
		{
			mNetPacketFactory.destroyPacket(packet);
			logError("只能在主线程发送消息");
			return;
		}
		if (mSocket == null || !mSocket.Connected || mNetState != NET_STATE.CONNECTED)
		{
			mNetPacketFactory.destroyPacket(packet);
			return;
		}
		
		var netPacket = packet as NetPacketBit;
		if (netPacket.isDestroy())
		{
			logError("消息对象已经被销毁,数据无效");
			return;
		}

		// 需要先序列化消息,同时获得包体实际的长度,提前获取包类型
		// 包类型的高2位表示了当前包体是用几个字节存储的
		ushort packetType = netPacket.getPacketType();
		mBitWriter.clear();
		netPacket.write(mBitWriter, out ulong fieldFlag);
		int realPacketSize = mBitWriter.getByteCount();
		byte[] packetBodyData = mBitWriter.getBuffer();
		if (realPacketSize < 0)
		{
			logError("消息序列化失败!");
			return;
		}

		// 加密包体
		++mSendSequenceNumber;
		if (packetBodyData != null)
		{
			mEncryptPacket?.Invoke(packetBodyData, 0, realPacketSize, (byte)(packetType + realPacketSize + (mSendSequenceNumber ^ 123 ^ packetType)));
		}

		// 将消息包中的数据准备好,然后放入发送列表中
		using var a = new ClassScope<SerializerBitWrite>(out var writer);
		writer.write(realPacketSize);
		writer.write(generateCRC16(realPacketSize));
		writer.write(packetType);
		writer.write(mSendSequenceNumber);
		// 写入一位用于获取是否需要使用标记位
		writer.write(fieldFlag != FULL_FIELD_FLAG);
		if (fieldFlag != FULL_FIELD_FLAG)
		{
			writer.write(fieldFlag);
		}
		// 写入包体数据
		writer.writeBuffer(packetBodyData, realPacketSize);
		// 确认字节所有位都已经填充
		writer.fillZeroToByteEnd();
		// 校验码
		writer.write(generateCRC16(writer.getBuffer(), writer.getByteCount()));
		int curByteCount = writer.getByteCount();
		// 添加到写缓冲中
		ARRAY_BYTE_THREAD(out byte[] packetData, getGreaterPow2(curByteCount));
		memcpy(packetData, writer.getBuffer(), 0, 0, curByteCount);
		mOutputBuffer.add(new(packetData, curByteCount, true, 0));
		mNetPacketFactory.destroyPacket(netPacket);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 解析包体数据
	protected override NetPacket parsePacket(ushort packetType, byte[] buffer, int size, int sequence, ulong fieldFlag)
	{
		// 创建对应的消息包,并设置数据,然后放入列表中等待解析
		var packetReply = mNetPacketFactory.createSocketPacket(packetType) as NetPacketBit;
		if (packetReply == null)
		{
			return null;
		}
		packetReply.setConnect(this);

		// 解密包体,然后解析包体
		if (buffer.isEmpty() || size == 0)
		{
			return packetReply;
		}
		mDecryptPacket?.Invoke(buffer, 0, size, (byte)(packetType + size + (sequence ^ 123 ^ packetType)));
		using var a = new ClassScope<SerializerBitRead>(out var reader);
		reader.init(buffer, size);
		if (!packetReply.read(reader, fieldFlag))
		{
			logError("解析失败:" + packetReply.getPacketType());
		}
		int readDataCount = reader.getReadByteCount();
		if (readDataCount != size)
		{
			logError("接收字节数与解析后消息包字节数不一致:" + packetType + ",接收:" + size + ", 解析:" + readDataCount + ", type:" + packetType + ", sequence:" + sequence);
			mNetPacketFactory.destroyPacket(packetReply);
			return null;
		}
		return packetReply;
	}
	protected override PARSE_RESULT preParsePacket(byte[] buffer, int size, out int bitIndex, out byte[] outPacket, out ushort packetType, 
													out int packetSize, out int sequence, out ulong fieldFlag)
	{
		bitIndex = 0;
		outPacket = null;
		packetType = 0;
		packetSize = 0;
		sequence = 0;
		fieldFlag = FULL_FIELD_FLAG;
		// 可能还没有接收完全,等待下次接收
		if (size == 0)
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}

		using var a = new ClassThreadScope<SerializerBitRead>(out var reader);
		reader.init(buffer, size, bitIndex);
		if (!reader.read(out packetSize))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (!reader.read(out ushort packetSizeCRC))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (generateCRC16(packetSize) != packetSizeCRC)
		{
			logError("packetSize crc校验失败,size:" + packetSize);
			return PARSE_RESULT.ERROR;
		}
		if (!reader.read(out packetType))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (!reader.read(out sequence))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (!reader.read(out bool useFlag))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (useFlag && !reader.read(out fieldFlag))
		{
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (packetSize > 0)
		{
			ARRAY_BYTE_THREAD(out outPacket, getGreaterPow2(packetSize));
			if (!reader.readBuffer(outPacket, packetSize))
			{
				UN_ARRAY_BYTE_THREAD(ref outPacket);
				return PARSE_RESULT.NOT_ENOUGH;
			}
		}
		reader.skipToByteEnd();

		// 需要在读crc之前计算包体的crc
		ushort curCrc = generateCRC16(reader.getBuffer(), reader.getReadByteCount());
		if (!reader.read(out ushort readCrc))
		{
			UN_ARRAY_BYTE_THREAD(ref outPacket);
			return PARSE_RESULT.NOT_ENOUGH;
		}

		// 客户端接收到的必须是SC类型的
		if (!(packetType > SC_GAME_MIN && packetType < SC_GAME_MAX) && 
			!(packetType > SC_GAME_CORE_MIN && packetType < SC_GAME_CORE_MAX))
		{
			UN_ARRAY_BYTE_THREAD(ref outPacket);
			logError("包类型错误:" + packetType);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.ERROR;
		}

		// 确认此消息的数据接收完全以后再验证序列号
		if (sequence != mLastReceiveSequenceNumber + 1 && mLastReceiveSequenceNumber != 0x7FFFFFFF)
		{
			// 不通知服务器接收到非法消息,因为可能会有误报
			// return PARSE_RESULT.ERROR;
		}
		mLastReceiveSequenceNumber = sequence;

		if (curCrc != readCrc)
		{
			logError("crc校验失败:" + packetType + ",解析出的crc:" + readCrc + ",计算出的crc:" + curCrc);
			UN_ARRAY_BYTE_THREAD(ref outPacket);
			return PARSE_RESULT.ERROR;
		}
		bitIndex = reader.getBitIndex();
		return PARSE_RESULT.SUCCESS;
	}
}