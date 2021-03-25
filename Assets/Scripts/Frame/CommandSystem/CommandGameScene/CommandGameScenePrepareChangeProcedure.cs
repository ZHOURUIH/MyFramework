using System;

public class CommandGameScenePrepareChangeProcedure : Command
{
	public Type mProcedure;
	public string mIntent;
	public float mPrepareTime;
	public override void resetProperty()
	{
		base.resetProperty();
		mProcedure = null;
		mIntent = null;
		mPrepareTime = -1.0f;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		// 准备时间必须大于0
		if (mPrepareTime <= 0.0f)
		{
			logError("preapare time must be larger than 0!");
			return;
		}
		// 正在准备跳转时,不允许再次准备跳转
		SceneProcedure curProcedure = gameScene.getCurProcedure();
		if (curProcedure.isPreparingExit())
		{
			logError("procedure is preparing to exit, can not prepare again!");
			return;
		}
		gameScene.prepareChangeProcedure(mProcedure, mPrepareTime, mIntent);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mProcedure:", mProcedure).
				Append(", mIntent:", mIntent).
				Append(", mPrepareTime:", mPrepareTime);
	}
}