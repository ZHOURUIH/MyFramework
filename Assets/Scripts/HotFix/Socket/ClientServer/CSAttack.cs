using static FrameUtility;
using static GU;

// auto generate start
// 通知服务器攻击
public class CSAttack : NetPacketBit
{
	public BIT_LONGS mTargetGUID = new();
	public BIT_INT mSkillID = new();
	public BIT_LONG mTimeStamp = new();
	public CSAttack()
	{
		addParam(mTargetGUID, false);
		addParam(mSkillID, false);
		addParam(mTimeStamp, false);
	}
	public static CSAttack get() { return PACKET<CSAttack>(); }
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mTargetGUID.read(reader);
		success = success && mSkillID.read(reader);
		success = success && mTimeStamp.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mTargetGUID.write(writer);
		mSkillID.write(writer);
		mTimeStamp.write(writer);
	}
	// auto generate end
	public static void send()
	{
		sendPacket(get());
	}
}