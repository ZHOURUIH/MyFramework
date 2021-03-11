using System;

public class CommandCameraOrthoSize : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public KEY_FRAME mKeyframe;
	public bool mLoop;
	public bool mFullOnce;
	public float mOnceLength;
	public float mOffset;
	public float mStartOrthoSize;
	public float mTargetOrthoSize;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartOrthoSize = 0.0f;
		mTargetOrthoSize = 0.0f;
		mLoop = false;
		mFullOnce = false;
	}
	public override void execute()
	{
		var obj = mReceiver as GameCamera;
        obj.getComponent(out CameraComponentOrthoSize component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setStartOrthoSize(mStartOrthoSize);
		component.setTargetOrthoSize(mTargetOrthoSize);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartFOV:" + mStartOrthoSize +
			", mTargetFOV:" + mTargetOrthoSize + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}