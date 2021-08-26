using System;

// 销毁一个角色
public class CmdCharacterManagerDestroy : Command
{
	public long mGUID;		// 角色唯一ID
	public override void resetProperty()
	{
		base.resetProperty();
		mGUID = 0;
	}
	public override void execute()
	{
		if (mGUID != 0)
		{
			mCharacterManager.destroyCharacter(mGUID);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mGUID:", mGUID);
	}
}