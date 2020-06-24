using UnityEngine;
using System;
using System.Collections;

public class TransformableComponentRotateFocus : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	protected Transformable mFocusTarget;
	protected Vector3 mFocusOffset;
	public void setFocusTarget(Transformable obj)
	{
		mFocusTarget = obj;
		if(mFocusTarget == null)
		{
			setActive(false);
		}
	}
	public void setFocusOffset(Vector3 offset) { mFocusOffset = offset; }
	public override void update(float elapsedTime)
	{
		Transformable obj = mComponentOwner as Transformable;
		Vector3 dir = mFocusTarget.localToWorldDirection(mFocusOffset) + mFocusTarget.getWorldPosition() - obj.getWorldPosition();
		obj.setWorldRotation(getLookAtRotation(dir));
		base.update(elapsedTime);
	}
	public void notifyBreak() { }
	//---------------------------------------------------------------------------------------------------------------
}
