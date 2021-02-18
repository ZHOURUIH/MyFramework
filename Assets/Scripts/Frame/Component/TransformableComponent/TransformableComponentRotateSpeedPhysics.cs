using UnityEngine;
using System;

public class TransformableComponentRotateSpeedPhysics : ComponentRotateSpeedPhysics
{
	//-----------------------------------------------------------------------------------------------------------------------
	protected override void applyRotation(ref Vector3 rotation, bool done = false, bool refreshNow = false)
	{
		(mComponentOwner as Transformable).setRotation(rotation);
	}
	protected override Vector3 getCurRotation() { return (mComponentOwner as Transformable).getRotation(); }
}