using System;

public class CmdGameSceneManagerEnter : Command
{
	public Type mStartProcedure;
	public Type mSceneType;
	public string mIntent;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartProcedure = null;
		mSceneType = null;
		mIntent = null;
	}
	public override void execute()
	{
		mGameSceneManager.enterScene(mSceneType, mStartProcedure, mIntent);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mSceneType:", mSceneType).
				Append(", mStartProcedure:", mStartProcedure).
				Append(", mIntent:", mIntent);
	}
}