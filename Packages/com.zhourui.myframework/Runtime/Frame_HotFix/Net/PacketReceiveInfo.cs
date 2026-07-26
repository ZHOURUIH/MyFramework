
// 消息包的信息,用于中转消息数据
// 包含原始字节数据、字段有效性标记、序列号、消息类型和符号位标记,由网络线程填充后交给主线程处理
public struct PacketReceiveInfo
{
	public byte[] mPacketData;	// 消息包内容
	public ulong mFieldFlag;	// 位有效标记
	public int mPacketSize;		// 消息大小
	public uint mSequence;		// 序列号
	public ushort mType;		// 消息ID
	public bool mHasSign;		// 是否有负数,有负数时就会需要写入符号位
	public PacketReceiveInfo(byte[] data, ulong fieldFlag, int packetSize, uint sequence, ushort type, bool hasSign)
	{
		mPacketData = data;
		mFieldFlag = fieldFlag;
		mPacketSize = packetSize;
		mSequence = sequence;
		mType = type;
		mHasSign = hasSign;
	}
}