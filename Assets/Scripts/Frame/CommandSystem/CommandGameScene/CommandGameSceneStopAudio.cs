using System;

public class CommandGameSceneStopAudio : Command
{
	public override void resetProperty()
	{
		base.resetProperty();
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out GameSceneComponentAudio component);
		component?.stop();
	}
}