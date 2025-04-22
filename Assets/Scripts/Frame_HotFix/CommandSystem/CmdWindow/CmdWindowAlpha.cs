using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 渐变窗口透明度
public class CmdWindowAlpha : Command
{
	public KeyFrameCallback mDoingCallback;		// 变化中回调
	public KeyFrameCallback mDoneCallback;		// 变化完成时回调
	public float mTargetAlpha;					// 目标透明度
	public float mStartAlpha;					// 起始透明度
	public float mOnceLength;					// 单次所需时间
	public float mOffset;						// 起始时间偏移
	public int mKeyframe;						// 所使用的关键帧ID
	public bool mLoop;							// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartAlpha = 1.0f;
		mTargetAlpha = 1.0f;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
		if (isEditor() && 
			!isFloatZero(mOnceLength) && 
			!obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + obj.getName());
		}
		obj.getOrAddComponent(out COMWindowAlpha com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartAlpha);
		com.setTarget(mTargetAlpha);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mStartAlpha:", mStartAlpha).
				append(", mTargetAlpha:", mTargetAlpha).
				append(", mLoop:", mLoop);
	}
}