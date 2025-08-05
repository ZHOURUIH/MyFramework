using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 渐变一个窗口的颜色,需要窗口有HSLOffset的shader
public class CmdWindowHSL : Command
{
	public KeyFrameCallback mDoingCallback;		// 变化中回调
	public KeyFrameCallback mDoneCallback;		// 变化完成时回调
	public Vector3 mStartHSL;					// 起始值
	public Vector3 mTargetHSL;					// 目标值
	public float mOnceLength;					// 单次所需时间
	public float mOffset;						// 起始时间偏移
	public int mKeyframe;						// 所使用的关键帧ID
	public bool mLoop;							// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mStartHSL = Vector3.zero;
		mTargetHSL = Vector3.zero;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUGUIObject;
		if (isEditor() && 
			!isFloatZero(mOnceLength) && 
			!obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + obj.getName());
		}
		obj.getOrAddComponent(out COMWindowHSL com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setStart(mStartHSL);
		com.setTarget(mTargetHSL);
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
				append(", mStartHSL:", mStartHSL).
				append(", mTargetHSL:", mTargetHSL).
				append(", mLoop:", mLoop);
	}
}