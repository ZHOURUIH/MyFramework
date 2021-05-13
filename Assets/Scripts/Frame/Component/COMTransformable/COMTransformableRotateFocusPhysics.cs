using UnityEngine;
using System;

public class COMTransformableRotateFocusPhysics : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	protected Transformable mFocusTarget;
	protected Vector3 mFocusOffset;
	public override void resetProperty()
	{
		base.resetProperty();
		mFocusTarget = null;
		mFocusOffset = Vector3.zero;
	}
	public void setFocusTarget(Transformable obj)
	{
		mFocusTarget = obj;
		if (mFocusTarget == null)
		{
			setActive(false);
		}
	}
	public void setFocusOffset(Vector3 offset) { mFocusOffset = offset; }
	public override void fixedUpdate(float elapsedTime)
	{
		var obj = mComponentOwner as Transformable;
		Vector3 dir = mFocusTarget.localToWorldDirection(mFocusOffset) + mFocusTarget.getWorldPosition() - obj.getWorldPosition();
		obj.setWorldRotation(getLookAtRotation(dir));
		base.fixedUpdate(elapsedTime);
	}
	public void notifyBreak() { }
	//---------------------------------------------------------------------------------------------------------------
}
