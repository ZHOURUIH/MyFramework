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
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe.ToString()).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mStartFOV:", mStartFOV).
				Append(", mTargetFOV:", mTargetFOV).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}