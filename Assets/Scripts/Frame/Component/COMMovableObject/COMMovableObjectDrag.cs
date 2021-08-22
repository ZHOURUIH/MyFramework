using UnityEngine;
using System;

public class COMMovableObjectDrag : ComponentDrag
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override bool mouseInObject(Vector3 mousePosition)
	{
		// 使用当前鼠标位置判断是否悬停,忽略被其他物体覆盖的情况
		Collider collider = (mComponentOwner as MovableObject).getCollider();
		if (collider == null)
		{
			return false;
		}
		return collider.Raycast(getMainCameraRay(mousePosition), out _, 10000.0f);
	}
}