using UnityEngine;
using System;

// 在物理更新中执行的使物体始终朝向目标的组件
public class COMTransformableRotateFocusPhysics : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	protected Transformable mFocusTarget;	// 朝向的目标
	protected Vector3 mFocusOffset;			// 目标的位置偏移
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
}