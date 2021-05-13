using UnityEngine;
using System;

public class COMTransformableScalePath : ComponentPathNormal, IComponentModifyScale
{
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setScale(value);
	}
}