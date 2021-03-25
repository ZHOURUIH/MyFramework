using System;

public class CommandMovableObjectPlayAudio : Command
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
		mVolume = 1.0f;
		mUseVolumeCoe = true;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as ComponentOwner;
		obj.getComponent(out MovableObjectComponentAudio component);
		component.setActive(true);
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