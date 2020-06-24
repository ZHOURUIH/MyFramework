using System;
using UnityEngine;
using System.Collections;

public class ComponentRotateSpeedPhysics : ComponentRotateSpeedBase
{
	public override void fixedUpdate(float elapsedTime) 
	{
		if (mPlayState == PLAY_STATE.PS_PLAY && !(isVectorZero(ref mRotateSpeed) && isVectorZero(ref mRotateAcceleration)))
		{
			mCurRotation += mRotateSpeed * elapsedTime;
			adjustAngle360(ref mCurRotation);
			applyRotation(ref mCurRotation, false);
			mRotateSpeed += mRotateAcceleration * elapsedTime;
		}
		base.fixedUpdate(elapsedTime);
	}
}