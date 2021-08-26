using System;

// 渐变一个浮点数
public class CmdMyTweenerFloat : Command
{
	public KeyFrameCallback mDoingCallBack;		// 变化中回调
	public KeyFrameCallback mDoneCallBack;		// 变化完成时回调
	public float mStart;						// 起始值
	public float mTarget;						// 目标值
	public float mOnceLength;					// 单次所需时间
	public float mOffset;						// 起始时间偏移
	public int mKeyframeID;						// 所使用的关键帧曲线ID
	public bool mLoop;							// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallBack = null;
		mDoneCallBack = null;
		mKeyframeID = KEY_CURVE.NONE;
		mStart = 1.0f;
		mTarget = 1.0f;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mLoop = false;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		obj.getComponent(out COMMyTweenerFloat com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setStart(mStart);
		com.setTarget(mTarget);
		com.play(mKeyframeID, mLoop, mOnceLength, mOffset);
	}
}