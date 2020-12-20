using UnityEngine;
using System.Collections;

public class CommandCameraFOV : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public KEY_FRAME mKeyframe;
	public float mOnceLength;
	public float mOffset;
	public float mAmplitude;
	public float mStartFOV;
	public float mTargetFOV;
	public bool mLoop;
	public bool mFullOnce;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mAmplitude = 1.0f;
		mStartFOV = 0.0f;
		mTargetFOV = 0.0f;
		mLoop = false;
		mFullOnce = false;
	}
	public override void execute()
	{
		GameCamera obj = mReceiver as GameCamera;
		CameraComponentFOV component = obj.getComponent(out component);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartFOV(mStartFOV);
		component.setTargetFOV(mTargetFOV);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartFOV:" + mStartFOV +
			", mTargetFOV:" + mTargetFOV + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}