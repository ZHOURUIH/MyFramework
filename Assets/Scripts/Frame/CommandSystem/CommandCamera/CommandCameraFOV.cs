using UnityEngine;
using System.Collections;

public class CommandCameraFOV : Command
{
	public string mName;
	public float mOnceLength;
	public float mOffset;
	public bool mLoop;
	public float mAmplitude;
	public bool mFullOnce;
	public float mStartFOV;
	public float mTargetFOV;
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public override void init()
	{
		base.init();
		mName = EMPTY_STRING;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mLoop = false;
		mAmplitude = 1.0f;
		mFullOnce = false;
        mStartFOV = 0.0f;
        mTargetFOV = 0.0f;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
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
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartFOV:" + mStartFOV +
			", mTargetFOV:" + mTargetFOV + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}