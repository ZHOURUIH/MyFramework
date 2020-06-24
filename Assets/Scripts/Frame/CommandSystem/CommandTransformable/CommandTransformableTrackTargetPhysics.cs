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
		ComponentOwner obj = mReceiver as ComponentOwner;
		ComponentTrackTargetPhysics component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(component.GetType());
		component.setSpeed(mSpeed);
		component.setTargetOffset(mOffset);
		component.setActive(true);
		component.setMoveDoneTrack(mTarget, mDoneCallback);
		component.setTrackingCallback(mTrackingCallback);
	}
}