using UnityEngine;
using System.Collections;

public class CommandGameSceneAudioVolume : Command
{
	public string mKeyFrameName;
	public float mStartVolume;
	public float mTargetVolume;
	public SOUND_DEFINE mSoundVolumeCoe;    // 如果为有效值,则启用音量系数
	public float mOnceLength;   // 持续时间
	public bool mLoop;
	public float mOffset;
	public bool mFullOnce;
	public float mAmplitude;
	public KeyFrameCallback mFadingCallback;
	public KeyFrameCallback mFadeDoneCallback;
	public override void init()
	{
		base.init();
		mKeyFrameName = EMPTY_STRING;
		mStartVolume = 0.0f;
		mTargetVolume = 0.0f;
		mSoundVolumeCoe = SOUND_DEFINE.SD_MIN;
		mOnceLength = 0.0f;
		mLoop = false;
		mOffset = 0.0f;
		mFullOnce = true;
		mAmplitude = 1.0f;
		mFadingCallback = null;
		mFadeDoneCallback = null;
	}
	public override void execute()
	{
		GameScene gameScene = mReceiver as GameScene;
		GameSceneComponentAudio audioComponent = gameScene.getComponent(out audioComponent);
		GameSceneComponentVolume component = gameScene.getComponent(out component);
		if (mSoundVolumeCoe != SOUND_DEFINE.SD_MIN)
		{
			float volumeCoe = mAudioManager.getVolumeScale(mSoundVolumeCoe);
			mStartVolume *= volumeCoe;
			mTargetVolume *= volumeCoe;
		}
		component.setTremblingCallback(mFadingCallback);
		component.setTrembleDoneCallback(mFadeDoneCallback);
		component.setActive(true);
		component.setStartVolume(mStartVolume);
		component.setTargetVolume(mTargetVolume);
		component.play(mKeyFrameName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyFrameName:" + mKeyFrameName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartVolume:" + mStartVolume +
			", mTargetVolume:" + mTargetVolume + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}