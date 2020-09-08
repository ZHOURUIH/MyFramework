using System;
using System.Collections.Generic;
using UnityEngine;

class CommandTransformableRotateFocusPhysics : Command
{
	public Transformable mTarget;
	public Vector3 mOffset;
	public override void init()
	{
		base.init();
		mTarget = null;
		mOffset = Vector3.zero;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentRotateFocusPhysics component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setActive(true);
		component.setFocusTarget(mTarget);
		component.setFocusOffset(mOffset);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override string showDebugInfo()
	{
		string target = mTarget != null ? mTarget.getName() : EMPTY_STRING;
		return base.showDebugInfo() + ": target:" + target;
	}
}