using System;

// 设置摄像机的FOV
public class CmdGameCameraFOV : Command
{
	public KeyFrameCallback mDoingCallback;	// 变化中的回调
	public KeyFrameCallback mDoneCallback;	// 变化结束时的回调
	public float mOnceLength;				// 变化的持续时间,如果是循环的,则表示单次的时间
	public float mOffsetTime;				// 时间起始偏移量
	public float mStartFOV;					// 起始的FOV
	public float mTargetFOV;				// 终止的FOV
	public int mKeyframe;					// 所使用的关键帧曲线ID
	public bool mLoop;						// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffsetTime = 0.0f;
		mStartFOV = 0.0f;
		mTargetFOV = 0.0f;
		mLoop = false;
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
		com.play(mKeyframe, mLoop, mOnceLength, mOffsetTime);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffsetTime).
				append(", mStartFOV:", mStartFOV).
				append(", mTargetFOV:", mTargetFOV).
				append(", mLoop:", mLoop);
	}
}