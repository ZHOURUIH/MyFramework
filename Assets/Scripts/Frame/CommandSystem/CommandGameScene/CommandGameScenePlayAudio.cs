using System;

public class CommandGameScenePlayAudio : Command
{
	public SOUND_DEFINE mSound;
	public string mSoundFileName;
	public float mVolume;
	public bool mUseVolumeCoe;       // 是否启用数据表格中的音量系数
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mSound = SOUND_DEFINE.MIN;
		mSoundFileName = null;
		mLoop = false;
		mVolume = 1.0f;
		mUseVolumeCoe = true;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out GameSceneComponentAudio component);
		string soundName = mSound != SOUND_DEFINE.MIN ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		if (mUseVolumeCoe)
		{
			mVolume *= mAudioManager.getVolumeScale(mSound);
		}
		component.play(soundName, mLoop, mVolume);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		string soundName = mSound != SOUND_DEFINE.MIN ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		builder.Append(": mSound:", mSound.ToString()).
				Append(", soundName:", soundName).
				Append(", mLoop:", mLoop).
				Append(", mVolume:", mVolume);
	}
}