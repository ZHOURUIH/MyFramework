using System;

public class CmdCharacterManagerDestroy : Command
{
	public long mGUID;
	public override void resetProperty()
	{
		base.resetProperty();
		mGUID = 0;
	}
	public override void execute()
	{
		if(mGUID != 0)
		{
			mCharacterManager.destroyCharacter(mGUID);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": mGUID:", mGUID);
	}
}