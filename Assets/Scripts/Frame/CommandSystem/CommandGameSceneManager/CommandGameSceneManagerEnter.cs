using UnityEngine;
using System.Collections;
using System;

public class CommandGameSceneManagerEnter : Command
{
	public Type mSceneType;
	public Type mStartProcedure;
	public string mIntent;
	public override void init()
	{
		base.init();
		mSceneType = null;
		mStartProcedure = null;
		mIntent = EMPTY_STRING;
	}
	public override void execute()
	{
		mGameSceneManager.enterScene(mSceneType, mStartProcedure, mIntent);
	}
	public override string showDebugInfo()
	{
		string scene = mSceneType != null ? mSceneType.ToString() : "null";
		string procedure = mStartProcedure != null ? mStartProcedure.ToString() : "null";
		return base.showDebugInfo() + ": mSceneType:" + scene + ", mStartProcedure:" + procedure;
	}
}