using System;

public class CmdGameSceneAudioVolume : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public float mStartVolume;
	public float mTargetVolume;
	public float mOnceLength;   // 持续时间
	public float mOffset;
	public int mSoundVolumeCoe;
	public int mKeyframe;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mSoundVolumeCoe = 0;
		mKeyframe = KEY_CURVE.NONE;
		mStartVolume = 0.0f;
		mTargetVolume = 0.0f;
		mOnceLength = 0.0f;
		mOffset = 0.0f;
		mLoop = false;
		mFullOnce = true;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out COMGameSceneVolume com);
		if (mSoundVolumeCoe != 0)
		{
			float volumeCoe = mAudioManager.getVolumeScale(mSoundVolumeCoe);
			mStartVolume *= volumeCoe;
			mTargetVolume *= volumeCoe;
		}
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartVolume);
		com.setTarget(mTargetVolume);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartVolume:", mStartVolume).
				Append(", mTargetVolume:", mTargetVolume).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}