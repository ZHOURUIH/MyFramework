using UnityEngine;

// 物体按指定旋转角度列表进行旋转的组件
public class COMTransformableRotateCurve : ComponentCurve, IComponentModifyRotation
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setRotation(value);
	}
}