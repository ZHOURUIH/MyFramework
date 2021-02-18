using System;

public class CommandGameSceneChangeAudioVolume : Command
{
	public float mVolume;
	public override void init()
	{
		base.init();
		mVolume = 0.0f;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		GameSceneComponentAudio audio = gameScene.getComponent(out audio);
		audio?.setVolume(mVolume);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mVolume:" + mVolume;
	}
}