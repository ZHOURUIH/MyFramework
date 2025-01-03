using static UnityUtility;
using static MathUtility;
using static FrameEditorUtility;

// 渐变一个窗口的填充值
public class CmdWindowFill : Command
{
	public KeyFrameCallback mDoingCallback;		// 变化中回调
	public KeyFrameCallback mDoneCallback;		// 变化完成时回调
	public float mStartValue;					// 起始值
	public float mTargetValue;					// 目标值
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
		mStartValue = 0.0f;
		mTargetValue = 0.0f;
		mOnceLength = 0.0f;
		mOffset = 0.0f;
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
		obj.getOrAddComponent(out COMWindowFill com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartValue);
		com.setTarget(mTargetValue);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
}