using System;

// 渐变一个物体的透明度
public class CmdMovableObjectAlpha : Command
{
	public KeyFrameCallback mDoingCallback;		// 渐变中回调
	public KeyFrameCallback mDoneCallback;		// 渐变结束时回调
	public float mStartAlpha;					// 起始透明度
	public float mTargetAlpha;					// 目标透明度
	public float mOnceLength;					// 单次需要的时间
	public float mOffset;						// 起始时间偏移
	public int mKeyframe;						// 所使用的关键帧曲线ID
	public bool mLoop;							// 是否循环
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
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:" , mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mStartAlpha:", mStartAlpha).
				append(", mTargetAlpha:", mTargetAlpha).
				append(", mLoop:", mLoop);
	}
}