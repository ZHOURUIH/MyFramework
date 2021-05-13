using System;

public class CmdGameSceneBackToLastProcedure : Command
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
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mIntent:", mIntent);
	}
}