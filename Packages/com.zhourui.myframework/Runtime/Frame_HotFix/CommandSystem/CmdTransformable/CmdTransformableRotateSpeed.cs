using UnityEngine;

// 使物体旋转
public class CmdTransformableRotateSpeed
{
	// 旋转加速度
	// 旋转起始速度
	// 旋转起始角度
	// 是否在物理更新中执行
	public static void execute(ITransformable obj, Vector3 rotateAcceleration, Vector3 rotateSpeed, Vector3 startAngle, bool updateInFixedTick)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableRotateSpeed com);
		com.setUpdateInFixedTick(updateInFixedTick);
		com.setActive(true);
		com.startRotateSpeed(startAngle, rotateSpeed, rotateAcceleration);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}