using UnityEngine;
using System;

public class COMTransformableScaleCurve : ComponentCurve, IComponentModifyScale
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setScale(value);
	}
}