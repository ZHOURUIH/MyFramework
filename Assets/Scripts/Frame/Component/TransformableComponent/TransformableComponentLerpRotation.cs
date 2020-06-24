using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentLerpRotation : ComponentLerpRotation
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