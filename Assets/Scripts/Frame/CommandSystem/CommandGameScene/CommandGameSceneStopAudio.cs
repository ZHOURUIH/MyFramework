using UnityEngine;
using System.Collections;

public class CommandGameSceneStopAudio : Command
{
	public override void init()
	{
		base.init();
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		GameSceneComponentAudio component = gameScene.getComponent(out component);
		component?.stop();
	}
}