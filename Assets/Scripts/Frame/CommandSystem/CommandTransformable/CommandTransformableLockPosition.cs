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
		obj.breakComponent<IComponentModifyPosition>(component.GetType());
		component.setActive(true);
		component.setLockPosition(mLockPosition);
		component.setLock(mLockX, mLockY, mLockZ);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLockPosition:" + mLockPosition + ", mLockX:" + mLockX + ", mLockY:" + mLockY + ", mLockZ:" + mLockZ; 
	}
}