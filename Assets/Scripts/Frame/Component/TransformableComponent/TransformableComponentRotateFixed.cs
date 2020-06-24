using UnityEngine;
using System;
using System.Collections;

public class TransformableComponentRotateFixed : ComponentRotateFixedBase
{
	public override void update(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.update(elapsedTime);
	}
	//---------------------------------------------------------------------------------------------------------------
}
