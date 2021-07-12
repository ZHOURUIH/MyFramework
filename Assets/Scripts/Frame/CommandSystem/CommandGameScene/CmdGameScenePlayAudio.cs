using System;

public class CmdGameScenePlayAudio : Command
{
	public string mSoundFileName;
	public float mVolume;
	public int mSound;
	public bool mUseVolumeCoe;       // 是否启用数据表格中的音量系数
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mSound = 0;
		mSoundFileName = null;
		mLoop = false;
		mVolume = 1.0f;
		mUseVolumeCoe = true;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out COMGameSceneAudio com);
		string soundName = mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		if (mUseVolumeCoe)
		{
			mVolume *= mAudioManager.getVolumeScale(mSound);
		}
		com.play(soundName, mLoop, mVolume);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		string soundName = mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		builder.Append(": mSound:", mSound).
				Append(", soundName:", soundName).
				Append(", mLoop:", mLoop).
				Append(", mVolume:", mVolume);
	}
}