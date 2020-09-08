using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandTransformableTrackTarget : Command
{
	public Transformable mTarget;
	public TrackCallback mDoneCallback;
	public TrackCallback mDoingCallback;
	public Vector3 mOffset;
	public float mSpeed;
	public override void init()
	{
		base.init();
		mTarget = null;
		mDoneCallback = null;
		mDoingCallback = null;
		mOffset = Vector3.zero;
		mSpeed = 0.0f;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		ComponentTrackTargetNormal component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(component.GetType());
		component.setActive(true);
		component.setSpeed(mSpeed);
		component.setTargetOffset(mOffset);
		component.setTrackingCallback(mDoingCallback);
		component.setMoveDoneTrack(mTarget, mDoneCallback);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override string showDebugInfo()
	{
		string target = mTarget != null ? mTarget.getName() : EMPTY_STRING;
		return base.showDebugInfo() + ": target:" + target + ", mSpeed:" + mSpeed;
	}
}