using System;

public class CommandGameSceneBackToLastProcedure : Command
{
	public string mIntent;
	public override void resetProperty()
	{
		base.resetProperty();
		mIntent = null;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.backToLastProcedure(mIntent);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mIntent:" + mIntent;
	}
}