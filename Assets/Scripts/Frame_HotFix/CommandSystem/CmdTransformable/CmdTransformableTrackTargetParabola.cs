﻿using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 以抛物线追踪一个目标
public class CmdTransformableTrackTargetParabola : Command
{
	public Transformable mTarget;			// 追踪目标
	public TrackCallback mDoingCallback;	// 追踪时回调
	public TrackCallback mDoneCallback;		// 追踪结束时回调
	public Vector3 mStartPosition;			// 抛物线起点
	public Vector3 mOffset;                 // 追踪目标位置偏移
	public float mNearRange;				// 距离目标还剩多少距离时认为是追踪完成
	public float mSpeed;                    // 追踪速度
	public float mMaxHeight;				// 抛物线最高高度,相对于起点
	public bool mUpdateInFixedTick;			// 是否在FixedUpdate中更新位置
	public override void resetProperty()
	{
		base.resetProperty();
		mTarget = null;
		mDoneCallback = null;
		mDoingCallback = null;
		mStartPosition = Vector3.zero;
		mOffset = Vector3.zero;
		mNearRange = 0.0f;
		mSpeed = 0.0f;
		mMaxHeight = 0.0f;
		mUpdateInFixedTick = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUIObject uiObj && 
			mTarget != null && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out ComponentTrackTargetParabola com);
		com.setUpdateInFixedTick(mUpdateInFixedTick);
		com.setActive(true);
		com.setTrackNearRange(mNearRange);
		com.setSpeed(mSpeed);
		com.setTargetOffset(mOffset);
		com.setMaxHeight(mMaxHeight);
		com.setStartPosition(mStartPosition);
		com.setTrackingCallback(mDoingCallback);
		com.setMoveDoneTrack(mTarget, mDoneCallback);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": target:", mTarget?.getName()).
				append(", mSpeed:", mSpeed);
	}
}