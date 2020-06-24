using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentMovePath : ComponentPathNormal, IComponentModifyPosition
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setPosition(value);
	}
}