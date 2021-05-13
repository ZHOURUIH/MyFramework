using System;

public class CmdGameSceneChangeProcedure : Command
{
	public Type mProcedure;
	public string mIntent;
	public override void resetProperty()
	{
		base.resetProperty();
		mProcedure = null;
		mIntent = null;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		// 当流程正在准备跳转流程时,不允许再跳转
		SceneProcedure curProcedure = gameScene.getCurProcedure();
		if(curProcedure != null	&& curProcedure.isPreparingExit())
		{
			logError("procedure is preparing to change, can not change again!");
			return;
		}
		// 不能重复进入同一流程
		if(curProcedure != null && curProcedure.getType() == mProcedure)
		{
			return;
		}
		gameScene.changeProcedure(mProcedure, mIntent);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mProcedure:", mProcedure).
				Append(", mIntent:", mIntent);
	}
}