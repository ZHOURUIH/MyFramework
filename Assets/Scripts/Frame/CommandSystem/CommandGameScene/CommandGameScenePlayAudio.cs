using UnityEngine;
using System.Collections;

public class CommandGameScenePlayAudio : Command
{
	public SOUND_DEFINE mSound;
	public string mSoundFileName;
	public float mVolume;
	public bool mUseVolumeCoe;       // 是否启用数据表格中的音量系数
	public bool mLoop;
	public override void init()
	{
		base.init();
		mSound = SOUND_DEFINE.MIN;
		mSoundFileName = null;
		mLoop = false;
		mVolume = 1.0f;
		mUseVolumeCoe = true;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		GameSceneComponentAudio component = gameScene.getComponent(out component);
		string soundName = mSound != SOUND_DEFINE.MIN ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		if (mUseVolumeCoe)
		{
			mVolume *= mAudioManager.getVolumeScale(mSound);
		}
		component.play(soundName, mLoop, mVolume);
	}
	public override string showDebugInfo()
	{
		string soundName = mSound != SOUND_DEFINE.MIN ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		return base.showDebugInfo() + ": mSound:" + mSound + ", soundName:" + soundName + ", mLoop:" + mLoop + ", mVolume:" + mVolume;
	}
}