using UnityEngine;
using static UnityUtility;

// 物体拖拽组件
public class COMMovableObjectDrag : ComponentDrag
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override bool mouseInObject(Vector3 mousePosition)
	{
		// 使用当前鼠标位置判断是否悬停,忽略被其他物体覆盖的情况
		if (mComponentOwner is not MovableObject movable)
		{
			return false;
		}
		Ray ray = getMainCameraRay(mousePosition);
		return movable.raycastSelf(ref ray, out _, 10000.0f);
	}
}