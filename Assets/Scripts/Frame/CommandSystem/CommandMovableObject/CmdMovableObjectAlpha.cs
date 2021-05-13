using System;

public class CmdMovableObjectAlpha : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public float mStartAlpha;
	public float mTargetAlpha;
	public float mOnceLength;
	public float mOffset;
	public int mKeyframe;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
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
		obj.getComponent(out COMMovableObjectAlpha com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartAlpha);
		com.setTarget(mTargetAlpha);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:" , mKeyframe).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartAlpha:", mStartAlpha).
				Append(", mTargetAlpha:", mTargetAlpha).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}
