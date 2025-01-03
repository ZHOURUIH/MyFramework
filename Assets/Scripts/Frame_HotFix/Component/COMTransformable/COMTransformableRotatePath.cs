using UnityEngine;

// 物体按指定的旋转关键帧列表进行旋转的组件
public class COMTransformableRotatePath : ComponentPath, IComponentModifyRotation
{
	// 旋转是不能相乘的,只能相加
	public override void setOffsetBlendAdd(bool blendMode) { mOffsetBlendAdd = true; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setRotation(value);
	}
}