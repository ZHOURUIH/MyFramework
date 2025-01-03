
// 消息包的信息,用于中转消息数据
public struct PacketReceiveInfo
{
	public byte[] mPacketData;	// 消息包内容
	public ulong mFieldFlag;	// 位有效标记
	public int mPacketSize;		// 消息大小
	public int mSequence;		// 序列号
	public ushort mType;		// 消息ID
	public PacketReceiveInfo(byte[] data, ulong fieldFlag, int packetSize, int sequence, ushort type)
	{
		mPacketData = data;
		mFieldFlag = fieldFlag;
		mPacketSize = packetSize;
		mSequence = sequence;
		mType = type;
	}
}