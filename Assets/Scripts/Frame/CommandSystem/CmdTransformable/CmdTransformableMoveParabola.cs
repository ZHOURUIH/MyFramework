using UnityEngine;

// 以抛物线移动物体
public class CmdTransformableMoveParabola : Command
{
	public KeyFrameCallback mDoingCallback;		// 移动中回调
	public KeyFrameCallback mDoneCallback;		// 移动完成时回调
	public Vector3 mStartPos;					// 起始位置
	public Vector3 mTargetPos;					// 目标位置
	public float mOnceLength;					// 单次所需时间
	public float mTopHeight;					// 抛物线最高的高度,相对于起始位置
	public float mOffset;						// 起始时间偏移
	public int mKeyframe;						// 所使用的关键帧ID
	public bool mLoop;							// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallback = null;
		mDoneCallback = null;
		mStartPos = Vector3.zero;
		mTargetPos = Vector3.zero;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mTopHeight = 0.0f;
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
		obj.getComponent(out COMTransformableMoveParabola com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setTargetPos(mTargetPos);
		com.setStartPos(mStartPos);
		com.setTopHeight(mTopHeight);
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
				append(", mStartPos:", mStartPos).
				append(", mTargetPos:", mTargetPos).
				append(", mTopHeight:", mTopHeight).
				append(", mLoop:", mLoop);
	}
}