using UnityEngine;

// 物体的沿指定关键帧列表移动和缩放的组件
public class COMTransformableMoveScalePath : ComponentPath2, IComponentModifyPosition
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value0, Vector3 value1)
	{
		(mComponentOwner as ITransformable).setPosition(value0);
		(mComponentOwner as ITransformable).setScale(value1);
	}
}