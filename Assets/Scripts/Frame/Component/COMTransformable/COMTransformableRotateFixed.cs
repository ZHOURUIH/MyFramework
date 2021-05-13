using UnityEngine;
using System;

public class COMTransformableRotateFixed : ComponentRotateFixedBase
{
	public override void update(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.update(elapsedTime);
	}
	//---------------------------------------------------------------------------------------------------------------
}
