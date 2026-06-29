
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