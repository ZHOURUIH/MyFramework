using System;

// 创建一个角色
public class CmdCharacterManagerCreateCharacter : Command
{
	public Type mCharacterType;		// 角色类型
	public string mName;			// 角色名字
	public long mID;				// 角色唯一ID
	public override void resetProperty()
	{
		base.resetProperty();
		mCharacterType = null;
		mName = null;
		mID = 0;
	}
	public override void execute()
	{
		if(mCharacterType == null)
		{
			return;
		}
		if (mID == 0)
		{
			mID = generateGUID();
		}
		mCharacterManager.createCharacter(mName, mCharacterType, mID);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mName:", mName).
				append(", mCharacterType:", mCharacterType.ToString()).
				append(", mID:", mID);
	}
}