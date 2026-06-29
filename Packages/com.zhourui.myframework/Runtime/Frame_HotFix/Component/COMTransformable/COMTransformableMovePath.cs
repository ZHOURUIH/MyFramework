using UnityEngine;

// 物体的沿指定关键帧列表移动的组件
public class COMTransformableMovePath : ComponentPath, IComponentModifyPosition
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as ITransformable).setPosition(value);
	}
}