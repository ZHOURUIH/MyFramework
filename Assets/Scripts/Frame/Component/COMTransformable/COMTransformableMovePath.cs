using UnityEngine;
using System;

// 物体的沿指定关键帧列表移动的组件
public class COMTransformableMovePath : ComponentPathNormal, IComponentModifyPosition
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setPosition(value);
	}
}