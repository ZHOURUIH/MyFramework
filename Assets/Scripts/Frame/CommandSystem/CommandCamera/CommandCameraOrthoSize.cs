using System;

public class CommandCameraOrthoSize : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public KEY_FRAME mKeyframe;
	public bool mLoop;
	public bool mFullOnce;
	public float mOnceLength;
	public float mOffset;
	public float mAmplitude;
	public float mStartOrthoSize;
	public float mTargetOrthoSize;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mAmplitude = 1.0f;
		mStartOrthoSize = 0.0f;
		mTargetOrthoSize = 0.0f;
		mLoop = false;
		mFullOnce = false;
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
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartFOV:" + mStartOrthoSize +
			", mTargetFOV:" + mTargetOrthoSize + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}