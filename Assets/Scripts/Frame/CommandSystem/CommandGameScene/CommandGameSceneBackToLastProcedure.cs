using UnityEngine;
using System.Collections;

public class CommandGameSceneBackToLastProcedure : Command
{
	public string mIntent;
	public override void init()
	{
		base.init();
		mIntent = null;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		gameScene.backToLastProcedure(mIntent);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mIntent:" + mIntent;
	}
}