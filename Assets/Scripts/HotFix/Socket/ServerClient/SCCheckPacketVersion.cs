using static UnityUtility;

// auto generate start
// 向客户端返回网络消息版本号检查结果,此消息应该排第一个
public class SCCheckPacketVersion : NetPacketBit
{
	public BIT_BOOL mResult = new();
	public SCCheckPacketVersion()
	{
		addParam(mResult, false);
	}
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mResult.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mResult.write(writer);
	}
	// auto generate end
	public override void execute()
	{
		log("消息版本号对比结果:" + mResult.mValue);
	}
}