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
		mIntent = null;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		// 当流程正在准备跳转流程时,不允许再跳转
		SceneProcedure curProcedure = gameScene.getCurProcedure();
		if(curProcedure != null	&& curProcedure.isPreparingExit())
		{
			logError("procedure is preparing to change, can not change again!");
			return;
		}
		// 不能重复进入同一流程
		if(curProcedure != null && curProcedure.getProcedureType() == mProcedure)
		{
			return;
		}
		gameScene.changeProcedure(mProcedure, mIntent);
	}
	public override string showDebugInfo()
	{
		string procedure = mProcedure != null ? mProcedure.ToString() : null;
		return base.showDebugInfo() + ": mProcedure:" + procedure + ", mIntent:" + mIntent;
	}
}