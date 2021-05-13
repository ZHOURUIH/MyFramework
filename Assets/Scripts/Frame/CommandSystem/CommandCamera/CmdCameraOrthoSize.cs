using System;

public class CmdCameraOrthoSize : Command
{
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public float mTargetOrthoSize;
	public float mStartOrthoSize;
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
        obj.getComponent(out COMCameraOrthoSize com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartOrthoSize);
		com.setTarget(mTargetOrthoSize);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartFOV:", mStartOrthoSize).
				Append(", mTargetFOV:", mTargetOrthoSize).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}