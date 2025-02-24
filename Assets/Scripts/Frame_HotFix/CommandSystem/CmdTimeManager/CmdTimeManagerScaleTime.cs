using static FrameBaseHotFix;

// 渐变时间缩放
public class CmdTimeManagerScaleTime : Command
{
	public KeyFrameCallback mDoingCallBack;		// 变化中回调
	public KeyFrameCallback mDoneCallBack;		// 变化完成时回调
	public float mStartScale;					// 起始缩放值
	public float mTargetScale;					// 目标缩放值
	public float mOnceLength;					// 单次所需时间
	public float mOffset;						// 起始时间偏移
	public int mKeyframe;						// 使用的关键帧曲线ID
	public bool mLoop;							// 是否循环
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
		mLoop = false;
	}
	public override void execute()
	{
		mTimeManager.getOrAddComponent(out COMTimeScale com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setStart(mStartScale);
		com.setTarget(mTargetScale);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mStartScale:", mStartScale).
				append(", mTargetScale:", mTargetScale).
				append(", mLoop:", mLoop);
	}
}