using UnityEngine;

// 使物体始终朝向指定目标
public class CmdTransformableRotateFocus
{
	// 朝向的目标
	// 目标位置偏移
	public static void execute(ITransformable obj, ITransformable target, Vector3 offset)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMTransformableRotateFocus com);
		com.setActive(true);
		com.setFocusTarget(target);
		com.setFocusOffset(offset);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
}