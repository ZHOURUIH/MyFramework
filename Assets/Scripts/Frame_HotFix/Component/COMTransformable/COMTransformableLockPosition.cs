using UnityEngine;

// 该组件在部分情况下会出现位置错误,类似于世界坐标位置的刷新没有实时同步,导致获取到的世界坐标错误,并且累积误差
public class COMTransformableLockPosition : GameComponent, IComponentModifyPosition, IComponentBreakable
{
	protected Vector3 mLockPosition;	// 锁定的坐标
	protected bool mLockX;				// 是否锁定X轴
	protected bool mLockY;				// 是否锁定Y轴
	protected bool mLockZ;				// 是否锁定Z轴
	public override void resetProperty()
	{
		base.resetProperty();
		mLockPosition = Vector3.zero;
		mLockX = false;
		mLockY = false;
		mLockZ = false;
	}
	public void setLockPosition(Vector3 pos) { mLockPosition = pos; }
	public void setLock(bool lockX, bool lockY, bool lockZ)
	{
		mLockX = lockX;
		mLockY = lockY;
		mLockZ = lockZ;
		if (!mLockX && !mLockY && !mLockZ)
		{
			setActive(false);
		}
	}
	public override void update(float elapsedTime)
	{
		var obj = mComponentOwner as Transformable;
		Vector3 worldPos = obj.getWorldPosition();
		if (mLockX)
		{
			worldPos.x = mLockPosition.x;
		}
		if (mLockY)
		{
			worldPos.y = mLockPosition.y;
		}
		if (mLockZ)
		{
			worldPos.z = mLockPosition.z;
		}
		obj.setWorldPosition(worldPos);
		base.update(elapsedTime);
	}
	public void notifyBreak() { }
}