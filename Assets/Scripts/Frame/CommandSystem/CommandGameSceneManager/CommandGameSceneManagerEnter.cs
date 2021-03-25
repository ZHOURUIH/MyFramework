using System;

public class CommandGameSceneManagerEnter : Command
{
	public Type mSceneType;
	public Type mStartProcedure;
	public string mIntent;
	public override void resetProperty()
	{
		base.resetProperty();
		mSceneType = null;
		mStartProcedure = null;
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