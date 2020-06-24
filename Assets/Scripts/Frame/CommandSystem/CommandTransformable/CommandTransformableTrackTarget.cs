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
		ComponentOwner obj = mReceiver as ComponentOwner;
		ComponentTrackTargetNormal component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(component.GetType());
		component.setActive(true);
		component.setSpeed(mSpeed);
		component.setTargetOffset(mOffset);
		component.setTrackingCallback(mDoingCallback);
		component.setMoveDoneTrack(mTarget, mDoneCallback);
	}
	public override string showDebugInfo()
	{
		string target = mTarget != null ? mTarget.getName() : EMPTY_STRING;
		return base.showDebugInfo() + ": target:" + target + ", mSpeed:" + mSpeed;
	}
}