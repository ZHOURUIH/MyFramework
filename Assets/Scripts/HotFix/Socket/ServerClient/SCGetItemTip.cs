
// auto generate start
// 通用的奖励物品提示
public class SCGetItemTip : NetPacketBit
{
	public NetStructItemInfo_List mIDCountList = new();
	public SCGetItemTip()
	{
		addParam(mIDCountList, false);
	}
	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		bool success = true;
		success = success && mIDCountList.read(reader, needReadSign);
		return success;
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		mIDCountList.write(writer, needWriteSign);
	}
	protected override bool generateHasSignInternal()
	{
		foreach (NetStructItemInfo item in mIDCountList)
		{
			if (item.hasSign())
			{
				return true;
			}
		}
		return false;
	}
	// auto generate end
	public override void execute()
	{}
}