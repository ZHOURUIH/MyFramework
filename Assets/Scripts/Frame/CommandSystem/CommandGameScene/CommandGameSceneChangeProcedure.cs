using UnityEngine;
using System.Collections;
using System;

public class CommandGameSceneChangeProcedure : Command
{
	public Type mProcedure;
	public string mIntent;
	public override void init()
	{
		base.init();
		mProcedure = null;
		mIntent = EMPTY_STRING;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		// 当流程正在准备跳转流程时,不允许再跳转
		SceneProcedure curProcedure = gameScene.getCurProcedure();
		if(curProcedure != null	&& curProcedure.isPreparingExit())
		{
			logError("procedure is preparing to change, can not change again!");
		}
		else
		{
			if(curProcedure == null || curProcedure.getProcedureType() != mProcedure)
			{
				gameScene.changeProcedure(mProcedure, mIntent);
				mFrameLogSystem?.logProcedure("进入流程: " + mProcedure.ToString());
			}
		}
	}
	public override string showDebugInfo()
	{
		string procedure = mProcedure != null ? mProcedure.ToString() : null;
		return base.showDebugInfo() + ": mProcedure:" + procedure + ", mIntent:" + mIntent;
	}
}