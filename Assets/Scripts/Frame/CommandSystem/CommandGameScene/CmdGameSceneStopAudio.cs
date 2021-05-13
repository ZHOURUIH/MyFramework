using System;

public class CmdGameSceneStopAudio : Command
{
	public override void resetProperty()
	{
		base.resetProperty();
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out COMGameSceneAudio com);
		com?.stop();
	}
}