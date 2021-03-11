using System;
using UnityEngine;

public class CommandTransformableRotateFocus : Command
{
	public Transformable mTarget;
	public Vector3 mOffset;
	public override void resetProperty()
	{
		base.resetProperty();
		mTarget = null;
		mOffset = Vector3.zero;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentRotateFocus component);
		component.setActive(true);
		component.setFocusTarget(mTarget);
		component.setFocusOffset(mOffset);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
}