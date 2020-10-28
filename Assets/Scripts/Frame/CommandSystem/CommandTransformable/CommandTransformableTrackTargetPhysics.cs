using System;
using System.Collections.Generic;
using UnityEngine;

class CommandTransformableTrackTargetPhysics : Command
{
	public Transformable mTarget;
	public TrackCallback mDoneCallback;
	public TrackCallback mTrackingCallback;
	public Vector3 mOffset;
	public float mSpeed;
	public override void init()
	{
		base.init();
		mTarget = null;
		mDoneCallback = null;
		mTrackingCallback = null;
		mOffset = Vector3.zero;
		mSpeed = 0.0f;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		ComponentTrackTargetPhysics component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(Typeof(component));
		component.setSpeed(mSpeed);
		component.setTargetOffset(mOffset);
		component.setActive(true);
		component.setMoveDoneTrack(mTarget, mDoneCallback);
		component.setTrackingCallback(mTrackingCallback);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
}