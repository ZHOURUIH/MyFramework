using System;
using System.Collections.Generic;

public class CommandWindowPlayAudio : Command
{
	public SOUND_DEFINE mSound;
	public string mSoundFileName;
	public bool mLoop;
	public float mVolume;
	public bool mUseVolumeCoe;       // 是否启用数据表格中的音量系数
	public override void init()
	{
		base.init();
		mSound = SOUND_DEFINE.SD_MAX;
		mSoundFileName = EMPTY_STRING;
		mLoop = false;
		mVolume = 1.0f;
		mUseVolumeCoe = true;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		WindowComponentAudio component = obj.getComponent(out component);
		component.setActive(true);
		string soundName = mSound != SOUND_DEFINE.SD_MAX ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		if (mUseVolumeCoe)
		{
			mVolume *= mAudioManager.getVolumeScale(mSound);
		}
		component.play(soundName, mLoop, mVolume);
	}
	public override string showDebugInfo()
	{
		string soundName = mSound != SOUND_DEFINE.SD_MAX ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		return base.showDebugInfo() + ": mSound:" + mSound + ", soundName:" + soundName + ", mLoop:" + mLoop + ", mVolume:" + mVolume
			+ ", mSoundFileName:" + mSoundFileName + ", mUseVolumeCoe:" + mUseVolumeCoe;
	}
}