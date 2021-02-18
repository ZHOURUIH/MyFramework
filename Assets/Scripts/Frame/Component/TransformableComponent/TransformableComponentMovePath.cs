using UnityEngine;
using System;

public class TransformableComponentMovePath : ComponentPathNormal, IComponentModifyPosition
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setPosition(value);
	}
}