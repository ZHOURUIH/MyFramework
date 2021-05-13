using UnityEngine;
using System;

public class COMTransformableLerpRotation : ComponentLerpRotation
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyRotation(Vector3 rotation)
	{
		(mComponentOwner as Transformable).setRotation(rotation);
	}
	protected override Vector3 getRotation()
	{
		return (mComponentOwner as Transformable).getRotation();
	}
}