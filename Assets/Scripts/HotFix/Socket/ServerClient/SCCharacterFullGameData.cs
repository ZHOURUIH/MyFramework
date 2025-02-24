using static BinaryUtility;
using static UnityUtility;
using static StringUtility;
using static FrameUtility;

// auto generate start
// 角色在Game层的完整数据
public class SCCharacterFullGameData : NetPacketBit
{
	public BIT_INT mHP = new();
	public BIT_INT mMaxHP = new();
	public BIT_STRING mName = new();
	public SCCharacterFullGameData()
	{
		addParam(mHP, true);
		addParam(mMaxHP, true);
		addParam(mName, true);
	}
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		if (hasBit(fieldFlag, 0))
		{
			success = success && mHP.read(reader);
		}
		if (hasBit(fieldFlag, 1))
		{
			success = success && mMaxHP.read(reader);
		}
		if (hasBit(fieldFlag, 2))
		{
			success = success && mName.read(reader);
		}
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		if (mHP.mValid)
		{
			setBitOne(ref fieldFlag, 0);
			mHP.write(writer);
		}
		if (mMaxHP.mValid)
		{
			setBitOne(ref fieldFlag, 1);
			mMaxHP.write(writer);
		}
		if (mName.mValid)
		{
			setBitOne(ref fieldFlag, 2);
			mName.write(writer);
		}
	}
	// auto generate end
	public override void execute()
	{
		if (!mMaxHP.isValid())
		{
			mMaxHP.set(-1);
		}
		log("登录成功,角色名:" + mName + ",HP:" + IToS(mHP) + ", MaxHP:" + IToS(mMaxHP));
		changeProcedure<MainSceneGaming>();
	}
}