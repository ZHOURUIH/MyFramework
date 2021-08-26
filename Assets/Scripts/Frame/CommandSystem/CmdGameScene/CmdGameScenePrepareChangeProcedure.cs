using System;

// 准备跳转到当前场景的指定流程,准备跳转过程中不会被中断
public class CmdGameScenePrepareChangeProcedure : Command
{
	public Type mProcedure;		// 要跳转到的流程类型
	public string mIntent;		// 跳转时要传递的参数
	public float mPrepareTime;	// 准备跳转的时间
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
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mProcedure:", mProcedure).
				append(", mIntent:", mIntent).
				append(", mPrepareTime:", mPrepareTime);
	}
}