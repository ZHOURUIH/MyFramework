using System;

public class CmdGameSceneChangeAudioVolume : Command
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
		gameScene.getComponent(out COMGameSceneAudio audio);
		audio?.setVolume(mVolume);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mVolume:", mVolume);
	}
}