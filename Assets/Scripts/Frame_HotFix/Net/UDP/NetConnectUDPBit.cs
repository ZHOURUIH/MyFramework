using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static CSharpUtility;
using static MathUtility;
using static BinaryUtility;
using static FrameDefine;

// Frame层默认的UDP连接封装类,按bit传输,应用层可根据实际需求仿照此类封装自己的UDP连接类
public class NetConnectUDPBit : NetConnectUDP
{
	protected EncryptPacket mEncryptPacket;				// 加密函数
	protected DecryptPacket mDecryptPacket;				// 解密函数
	protected SerializerBitWrite mBitWriter = new();	// 用于序列化
	protected long mToken;								// 用于服务器识别客户端的唯一凭证,一般是当前角色的ID
	public override void resetProperty()
	{
		base.resetProperty();
		mEncryptPacket = null;
		mDecryptPacket = null;
		mBitWriter.clear();
		mToken = 0;
	}
	public void setToken(long token) { mToken = token; }
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
		if (mSocket == null)
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

		mBitWriter.clear();
		netPacket.write(mBitWriter, out ulong fieldFlag);
		int realPacketSize = mBitWriter.getByteCount();
		byte[] packetBodyData = mBitWriter.getBuffer();
		// 序列化完以后立即销毁消息包
		mNetPacketFactory.destroyPacket(netPacket);

		if (realPacketSize < 0)
		{
			logError("消息序列化失败!");
			return;
		}

		// 加密包体
		if (packetBodyData != null)
		{
			mEncryptPacket?.Invoke(packetBodyData, 0, realPacketSize, 0);
		}

		using var a = new ClassScope<SerializerBitWrite>(out var writer);
		writer.write(realPacketSize);
		writer.write(generateCRC16(realPacketSize));
		writer.write(netPacket.getPacketType());
		writer.write(mToken);
		// 写入一位用于获取是否需要使用标记位
		writer.write(fieldFlag != FULL_FIELD_FLAG);
		if (fieldFlag != FULL_FIELD_FLAG)
		{
			writer.write(fieldFlag);
		}
		writer.writeBuffer(packetBodyData, realPacketSize);
		writer.write(generateCRC16(writer.getBuffer(), writer.getByteCount()));
		int curByteCount = writer.getByteCount();
		ARRAY_BYTE_THREAD(out byte[] packetData, getGreaterPow2(curByteCount));
		memcpy(packetData, writer.getBuffer(), 0, 0, curByteCount);
		// 添加到写缓冲中
		mOutputBuffer.add(new(packetData, curByteCount, true, 0));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override PARSE_RESULT preParsePacket(byte[] buffer, int size, ref int bitIndex, out byte[] outPacket, out ushort packetType, out int packetSize, out ulong fieldFlag)
	{
		outPacket = null;
		packetType = 0;
		packetSize = 0;
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
			return PARSE_RESULT.ERROR;
		}
		if (!reader.read(out packetType))
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
		// 需要在读取crc之前就计算crc
		ushort generatedCRC = generateCRC16(reader.getBuffer(), reader.getBufferSize());
		if (!reader.read(out ushort readCrc))
		{
			UN_ARRAY_BYTE_THREAD(ref outPacket);
			return PARSE_RESULT.NOT_ENOUGH;
		}
		if (generatedCRC != readCrc)
		{
			logError("crc校验失败:" + packetType + ",解析出的crc:" + readCrc + ",计算出的crc:" + generatedCRC);
			UN_ARRAY_BYTE_THREAD(ref outPacket);
			return PARSE_RESULT.ERROR;
		}
		bitIndex = reader.getBitIndex();
		return PARSE_RESULT.SUCCESS;
	}
	// 解析包体数据
	protected override NetPacket parsePacket(ushort packetType, byte[] buffer, int size, ulong fieldFlag)
	{
		// 创建对应的消息包,并设置数据,然后放入列表中等待解析
		var packetReply = mNetPacketFactory.createSocketPacket(packetType) as NetPacketBit;
		if (packetReply == null)
		{
			return null;
		}
		packetReply.setConnect(this);

		// 解密包体
		if (buffer.isEmpty() || size == 0)
		{
			return packetReply;
		}
		mDecryptPacket?.Invoke(buffer, 0, size, 0);
		using var a = new ClassScope<SerializerBitRead>(out var reader);
		reader.init(buffer, size);
		if (!packetReply.read(reader, fieldFlag))
		{
			logError("解析失败:" + packetReply.getPacketType());
		}
		int readDataCount = reader.getReadByteCount();
		if (readDataCount != size)
		{
			logError("接收字节数与解析后消息包字节数不一致:" + packetType + ",接收:" + size + ", 解析:" + readDataCount + ", type:" + packetType);
			mNetPacketFactory.destroyPacket(packetReply);
			return null;
		}
		return packetReply;
	}
}