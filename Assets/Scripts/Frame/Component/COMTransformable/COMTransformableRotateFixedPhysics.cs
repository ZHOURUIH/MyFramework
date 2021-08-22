using UnityEngine;
using System;

public class COMTransformableRotateFixedPhysics : ComponentRotateFixed
{
	public override void fixedUpdate(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.fixedUpdate(elapsedTime);
	}
}