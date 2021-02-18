using UnityEngine;
using System;

public class TransformableComponentScaleCurve : ComponentCurve, IComponentModifyScale
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setScale(value);
	}
}