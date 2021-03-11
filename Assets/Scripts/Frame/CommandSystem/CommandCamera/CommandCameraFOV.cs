using System;

public class CommandCameraFOV : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public KEY_FRAME mKeyframe;
	public float mOnceLength;
	public float mOffset;
	public float mStartFOV;
	public float mTargetFOV;
	public bool mLoop;
	public bool mFullOnce;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartFOV = 0.0f;
		mTargetFOV = 0.0f;
		mLoop = false;
		mFullOnce = false;
	}
	public override void execute()
	{
		var obj = mReceiver as GameCamera;
		obj.getComponent(out CameraComponentFOV component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setStartFOV(mStartFOV);
		component.setTargetFOV(mTargetFOV);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartFOV:" + mStartFOV +
			", mTargetFOV:" + mTargetFOV + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}