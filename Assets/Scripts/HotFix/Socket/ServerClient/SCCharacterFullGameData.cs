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
	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		bool success = true;
		if (hasBit(fieldFlag, 0))
		{
			success = success && mHP.read(reader, needReadSign);
		}
		if (hasBit(fieldFlag, 1))
		{
			success = success && mMaxHP.read(reader, needReadSign);
		}
		if (hasBit(fieldFlag, 2))
		{
			success = success && mName.read(reader, needReadSign);
		}
		return success;
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		if (mHP.mValid)
		{
			setBitOne(ref fieldFlag, 0);
			mHP.write(writer, needWriteSign);
		}
		if (mMaxHP.mValid)
		{
			setBitOne(ref fieldFlag, 1);
			mMaxHP.write(writer, needWriteSign);
		}
		if (mName.mValid)
		{
			setBitOne(ref fieldFlag, 2);
			mName.write(writer, needWriteSign);
		}
	}
	protected override bool generateHasSignInternal()
	{
		if (mHP.mValid && mHP < 0)
		{
			return true;
		}
		if (mMaxHP.mValid && mMaxHP < 0)
		{
			return true;
		}
		return false;
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