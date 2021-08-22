using UnityEngine;
using System;

public class COMTransformableMoveCurve : ComponentCurve, IComponentModifyPosition
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setPosition(value);
	}
}