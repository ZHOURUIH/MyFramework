using System;
using System.Collections;

public class CommandGameScenePrepareChangeProcedure : Command
{
	public Type		mProcedure;
	public string	mIntent;
	public float	mPrepareTime;
	public override void init()
	{
		base.init();
		mProcedure = null;
		mIntent = EMPTY_STRING;
		mPrepareTime = -1.0f;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		// 准备时间必须大于0
		SceneProcedure curProcedure = gameScene.getCurProcedure();
		if(mPrepareTime <= 0.0f)
		{
			logError("preapare time must be larger than 0!");
		}
		// 正在准备跳转时,不允许再次准备跳转
		else if(curProcedure.isPreparingExit())
		{
			logError("procedure is preparing to exit, can not prepare again!");
		}
		else
		{
			gameScene.prepareChangeProcedure(mProcedure, mPrepareTime, mIntent);
			mFrameLogSystem?.logProcedure("准备进入流程 : " + mProcedure);
		}
	}
	public override string showDebugInfo()
	{
		string procedure = mProcedure != null ? mProcedure.ToString() : "null";
		return base.showDebugInfo() + ": mProcedure:" + procedure + ", mIntent:" + mIntent + ", mPrepareTime:" + mPrepareTime;
	}
}