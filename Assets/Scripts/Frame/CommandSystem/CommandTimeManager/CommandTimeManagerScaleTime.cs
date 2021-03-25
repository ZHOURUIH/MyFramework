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
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe.ToString());
		builder.Append(", mOnceLength:", mOnceLength);
		builder.Append(", mOffset:", mOffset);
		builder.Append(", mStartScale:", mStartScale);
		builder.Append(", mTargetScale:", mTargetScale);
		builder.Append(", mLoop:", mLoop);
		builder.Append(", mFullOnce:", mFullOnce);
	}
}
