using static FrameUtility;
using static GU;

// auto generate start
// 通用的奖励物品提示
public class SCGetItemTip : NetPacketBit
{
	public NetStructItemInfo_List mIDCountList = new();
	public SCGetItemTip()
	{
		addParam(mIDCountList, false);
	}
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mIDCountList.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mIDCountList.write(writer);
	}
	// auto generate end
	public override void execute()
	{}
}