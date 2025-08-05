﻿using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 插值改变一个物体的旋转,如果目标旋转不变,离目标旋转越近,旋转速度越慢
public class CmdTransformableLerpRotation : Command
{
	public LerpCallback mDoingCallBack;		// 插值中回调
	public LerpCallback mDoneCallBack;		// 插值完成时回调
	public Vector3 mTargetRotation;			// 目标旋转值
	public float mLerpSpeed;				// 插值速度
	public override void resetProperty()
	{
		base.resetProperty();
		mDoingCallBack = null;
		mDoneCallBack = null;
		mTargetRotation = Vector3.zero;
		mLerpSpeed = 0.0f;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			!isFloatZero(mLerpSpeed) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableLerpRotation com);
		com.setLerpingCallback(mDoingCallBack);
		com.setLerpDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setTargetRotation(mTargetRotation);
		com.setLerpSpeed(mLerpSpeed);
		com.play();
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mLerpSpeed:", mLerpSpeed).
				append(", mTargetRotation:", mTargetRotation);
	}
}