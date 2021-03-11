using System;

public class CommandGameSceneChangeAudioVolume : Command
{
	public float mVolume;
	public override void resetProperty()
	{
		base.resetProperty();
		mVolume = 0.0f;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out GameSceneComponentAudio audio);
		audio?.setVolume(mVolume);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mVolume:" + mVolume;
	}
}