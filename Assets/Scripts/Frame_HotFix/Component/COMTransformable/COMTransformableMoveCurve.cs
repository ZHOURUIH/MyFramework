using UnityEngine;

// 物体的沿指定路线移动的组件
public class COMTransformableMoveCurve : ComponentCurve, IComponentModifyPosition
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as ITransformable).setPosition(value);
	}
}