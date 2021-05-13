using UnityEngine;
using System;

public class COMTransformableRotateSpeed : ComponentRotateSpeedNormal  
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyRotation(ref Vector3 rotation)
	{
		(mComponentOwner as Transformable).setRotation(rotation);
	}
	protected override Vector3 getCurRotation() { return (mComponentOwner as Transformable).getRotation(); }
}