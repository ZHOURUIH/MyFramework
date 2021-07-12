using System;

public class CmdTimeManagerScaleTime : Command
{
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public float mStartScale;
	public float mTargetScale;
	public float mOnceLength;
	public float mOffset;
	public int mKeyframe;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallBack = null;
		mDoneCallBack = null;
		mKeyframe = KEY_CURVE.NONE;
		mStartScale = 1.0f;
		mTargetScale = 1.0f;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		mTimeManager.getComponent(out COMTimeScale com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setStart(mStartScale);
		com.setTarget(mTargetScale);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe);
		builder.Append(", mOnceLength:", mOnceLength);
		builder.Append(", mOffset:", mOffset);
		builder.Append(", mStartScale:", mStartScale);
		builder.Append(", mTargetScale:", mTargetScale);
		builder.Append(", mLoop:", mLoop);
		builder.Append(", mFullOnce:", mFullOnce);
	}
}
