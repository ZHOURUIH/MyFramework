using System;

// 返回到上一个流程
public class CmdGameSceneBackToLastProcedure : Command
{
	public string mIntent;		// 跳转流程时要传递的参数
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
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mIntent:", mIntent);
	}
}