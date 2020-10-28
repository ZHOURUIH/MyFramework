using UnityEngine;
using System.Collections;

public class CommandTransformableLockPosition : Command
{
	public Vector3 mLockPosition;
	public bool mLockX;
	public bool mLockY;
	public bool mLockZ;
	public override void init()
	{
		base.init();
		mLockPosition = Vector3.zero;
		mLockX = false;
		mLockY = false;
		mLockZ = false;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentLockPosition component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(Typeof(component));
		component.setActive(true);
		component.setLockPosition(mLockPosition);
		component.setLock(mLockX, mLockY, mLockZ);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLockPosition:" + mLockPosition + ", mLockX:" + mLockX + ", mLockY:" + mLockY + ", mLockZ:" + mLockZ; 
	}
}