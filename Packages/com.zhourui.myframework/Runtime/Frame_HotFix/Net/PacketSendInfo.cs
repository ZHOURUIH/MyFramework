
// 待发送的消息包信息
// 包含原始字节数据、数据大小、是否需要销毁标记和消息类型,由各连接子类发送线程取出并写入Socket
public struct PacketSendInfo
{
	public byte[] mData;
	public int mDataSize;
	public bool mDataNeedDestroy;
	public int mPacketType;
	public PacketSendInfo(byte[] data, int size, bool dataNeedDestroy, int packetType)
	{
		mData = data;
		mDataSize = size;
		mDataNeedDestroy = dataNeedDestroy;
		mPacketType = packetType;
	}
}