using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 旋转物体
public class CmdTransformableRotate : Command
{
	public KeyFrameCallback mDoingCallback;		// 旋转中回调
	public KeyFrameCallback mDoneCallback;		// 旋转完成时回调
	public Vector3 mStartRotation;				// 起始旋转值
	public Vector3 mTargetRotation;				// 目标旋转值
	public float mOnceLength;					// 单次所需时间
	public float mOffset;						// 起始时间偏移
	public int mKeyframe;						// 所使用的关键帧ID
	public bool mLoop;							// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mStartRotation = Vector3.zero;
		mTargetRotation = Vector3.zero;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (!isFloatZero(mOnceLength) && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableRotate com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setTarget(mTargetRotation);
		com.setStart(mStartRotation);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mLoop:", mLoop).
				append(", mStartRotation:", mStartRotation).
				append(", mTargetRotation:", mTargetRotation);
	}
}