using System;

public class CommandMovableObjectAlpha : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public KEY_FRAME mKeyframe;
	public float mStartAlpha;
	public float mTargetAlpha;
	public float mOnceLength;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_FRAME.NONE;
		mStartAlpha = 1.0f;
		mTargetAlpha = 1.0f;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as ComponentOwner;
		obj.getComponent(out MovableObjectComponentAlpha component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setStartAlpha(mStartAlpha);
		component.setTargetAlpha(mTargetAlpha);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartAlpha:" + mStartAlpha +
			", mTargetAlpha:" + mTargetAlpha + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}
