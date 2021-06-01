using UnityEngine;
using System;

public class COMTransformableRotateFixed : ComponentRotateFixed
{
	public override void update(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.update(elapsedTime);
	}
	//---------------------------------------------------------------------------------------------------------------
}
