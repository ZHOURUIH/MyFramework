using System;

public struct PacketSimpleInfo
{
	public byte[] mPacketData;
	public ulong mFieldFlag;
	public int mPacketSize;
	public int mSequence;
	public ushort mType;
	public PacketSimpleInfo(byte[] data, ulong fieldFlag, int packetSize, int sequence, ushort type)
	{
		mPacketData = data;
		mFieldFlag = fieldFlag;
		mPacketSize = packetSize;
		mSequence = sequence;
		mType = type;
	}
}