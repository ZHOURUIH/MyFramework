using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentScalePath : ComponentPathNormal, IComponentModifyScale
{
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setScale(value);
	}
}