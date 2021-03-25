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
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:" , mKeyframe.ToString()).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartAlpha:", mStartAlpha).
				Append(", mTargetAlpha:", mTargetAlpha).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}
