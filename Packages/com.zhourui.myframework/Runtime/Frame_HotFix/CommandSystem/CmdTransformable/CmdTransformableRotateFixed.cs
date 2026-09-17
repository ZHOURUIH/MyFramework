using UnityEngine;

// 锁定物体的世界旋转
public class CmdTransformableRotateFixed
{
	// 锁定的世界旋转值
	// 是否锁定
	public static void execute(ITransformable obj, Vector3 fixedEuler, bool active)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableRotateFixed com);
		com.setActive(active);
		com.setFixedEuler(fixedEuler);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}