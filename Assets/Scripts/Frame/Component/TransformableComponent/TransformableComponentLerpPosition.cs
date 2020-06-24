using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentLerpPosition : ComponentLerpPosition
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyPosition(Vector3 position)
	{
		(mComponentOwner as Transformable).setPosition(position);
	}
	protected override Vector3 getPosition()
	{
		return (mComponentOwner as Transformable).getPosition();
	}
}