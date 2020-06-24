using System;
using System.Collections.Generic;
using UnityEngine;

public class ComponentTrackTargetBase : GameComponent, IComponentModifyPosition, IComponentBreakable
{
	protected Transformable mTarget;
	protected TrackCallback mDoneCallback;
	protected TrackCallback mTrackingCallback;
	protected Vector3 mTargetOffset;
	protected float mSpeed;
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		if(!(mComponentOwner is Transformable))
		{
			logError("ComponentTrackTarget can only add to Transformable");
		}
	}
	public virtual void setMoveDoneTrack(Transformable target, TrackCallback doneCallback, bool callLast = true)
	{
		TrackCallback tempCallback = mDoneCallback;
		mDoneCallback = null;
		// 如果回调函数当前不为空,则是中断了正在进行的变化
		if(callLast)
		{
			tempCallback?.Invoke(this, true);
		}
		mDoneCallback = doneCallback;
		mTarget = target;
		if(mTarget == null)
		{
			setActive(false);
		}
	}
	public void setTrackingCallback(TrackCallback callback) { mTrackingCallback = callback; }
	public void setSpeed(float speed) { mSpeed = speed; }
	public void setTargetOffset(Vector3 offset) { mTargetOffset = offset; }
	public Transformable getTrackTarget() { return mTarget; }
	public void notifyBreak()
	{
		setMoveDoneTrack(null, null);
		setTrackingCallback(null);
	}
	//-----------------------------------------------------------------------------------------------------------------
	protected virtual Vector3 getPosition()
	{
		return (mComponentOwner as Transformable).getPosition();
	}
	protected virtual void setPosition(ref Vector3 pos)
	{
		(mComponentOwner as Transformable).setPosition(pos);
	}
	protected virtual Vector3 getTargetPosition() { return mTarget.getWorldPosition() + mTarget.localToWorldDirection(mTargetOffset); }
}