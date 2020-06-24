using UnityEngine;
using System.Collections;

public class CommandCameraOrthoSize : Command
{
	public string mName;
	public float mOnceLength;
	public float mOffset;
	public bool mLoop;
	public float mAmplitude;
	public bool mFullOnce;
	public float mStartOrthoSize;
	public float mTargetOrthoSize;
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
        mStartOrthoSize = 0.0f;
        mTargetOrthoSize = 0.0f;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
	}
	public override void execute()
	{
		GameCamera obj = mReceiver as GameCamera;
        CameraComponentOrthoSize component = obj.getComponent(out component);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartOrthoSize(mStartOrthoSize);
		component.setTargetOrthoSize(mTargetOrthoSize);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartFOV:" + mStartOrthoSize +
			", mTargetFOV:" + mTargetOrthoSize + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}