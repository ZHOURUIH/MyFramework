using UnityEngine;

// 锁定物体的世界坐标不变,可能会有误差
public class CmdTransformableLockPosition
{
	// 锁定的目标位置
	// 是否锁定X轴
	// 是否锁定Y轴
	// 是否锁定Z轴
	public static void execute(ITransformable obj, Vector3 lockPosition, bool lockX, bool lockY, bool lockZ)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableLockPosition com);
		com.setActive(true);
		com.setLockPosition(lockPosition);
		com.setLock(lockX, lockY, lockZ);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}