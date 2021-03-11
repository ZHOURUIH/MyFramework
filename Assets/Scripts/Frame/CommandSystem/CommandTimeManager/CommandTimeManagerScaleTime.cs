using System;

public class CommandTimeManagerScaleTime : Command
{
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public KEY_FRAME mKeyframe;
	public float mStartScale;
	public float mTargetScale;
	public float mOnceLength;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallBack = null;
		mDoneCallBack = null;
		mKeyframe = KEY_FRAME.NONE;
		mStartScale = 1.0f;
		mTargetScale = 1.0f;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		mTimeManager.getComponent(out TimeComponentScale component);
		component.setTremblingCallback(mDoingCallBack);
		component.setTrembleDoneCallback(mDoneCallBack);
		component.setActive(true);
		component.setStartScale(mStartScale);
		component.setTargetScale(mTargetScale);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + 
			", mStartScale:" + mStartScale + ", mTargetScale:" + mTargetScale + ", mLoop:" + mLoop + ", mFullOnce:" + mFullOnce;
	}
}
