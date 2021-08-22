using System;

public class CmdGameCameraFOV : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public float mOnceLength;
	public float mOffset;
	public float mStartFOV;
	public float mTargetFOV;
	public int mKeyframe;
	public bool mLoop;
	public bool mFullOnce;
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
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
		obj.getComponent(out COMCameraFOV com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStartFOV(mStartFOV);
		com.setTargetFOV(mTargetFOV);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartFOV:", mStartFOV).
				Append(", mTargetFOV:", mTargetFOV).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}