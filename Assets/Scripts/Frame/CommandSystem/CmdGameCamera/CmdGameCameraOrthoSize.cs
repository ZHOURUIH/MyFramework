using System;

// 用于渐变正交摄像机的正交大小
public class CmdGameCameraOrthoSize : Command
{
	public KeyFrameCallback mDoingCallback;	// 变化中的回调
	public KeyFrameCallback mDoneCallback;	// 变化结束时的回调
	public float mTargetOrthoSize;			// 起始的大小
	public float mStartOrthoSize;			// 终止的大小
	public float mOnceLength;				// 变化的持续时间,如果是循环的,则表示单次的时间
	public float mOffset;					// 时间起始偏移量
	public int mKeyframe;					// 所使用的关键帧曲线ID
	public bool mLoop;						// 是否循环
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
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mStartFOV:", mStartOrthoSize).
				append(", mTargetFOV:", mTargetOrthoSize).
				append(", mLoop:", mLoop);
	}
}