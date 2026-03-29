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
	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		bool success = true;
		success = success && mTargetGUID.read(reader, needReadSign);
		success = success && mSkillID.read(reader, needReadSign);
		success = success && mTimeStamp.read(reader, needReadSign);
		return success;
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		mTargetGUID.write(writer, needWriteSign);
		mSkillID.write(writer, needWriteSign);
		mTimeStamp.write(writer, needWriteSign);
	}
	protected override bool generateHasSignInternal()
	{
		foreach (long item in mTargetGUID)
		{
			if (item < 0)
			{
				return true;
			}
		}
		if (mSkillID < 0)
		{
			return true;
		}
		if (mTimeStamp < 0)
		{
			return true;
		}
		return false;
	}
	// auto generate end
	public static void send()
	{
		sendPacket(get());
	}
}