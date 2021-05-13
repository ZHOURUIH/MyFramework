using UnityEngine;
using System;

public class COMTransformableRotatePath : ComponentPathNormal, IComponentModifyRotation
{
	// 旋转是不能相乘的,只能相加
	public override void setOffsetBlendAdd(bool blendMode) { mOffsetBlendAdd = true; }
	//-------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setRotation(value);
	}
}