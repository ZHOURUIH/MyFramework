using static UnityUtility;

public class SCDemo : NetPacketBit
{
	public BIT_UINT mDemoParam = new();
	public SCDemo()
	{
		addParam(mDemoParam, false);
	}
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mDemoParam.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mDemoParam.write(writer);
	}
	public override void execute()
	{
		log("收到SCDemo");
	}
}