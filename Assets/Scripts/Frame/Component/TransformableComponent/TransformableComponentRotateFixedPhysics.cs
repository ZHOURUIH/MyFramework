using UnityEngine;
using System;
using System.Collections;

public class TransformableComponentRotateFixedPhysics : ComponentRotateFixedBase
{
	public override void fixedUpdate(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.fixedUpdate(elapsedTime);
	}
	//---------------------------------------------------------------------------------------------------------------
}
