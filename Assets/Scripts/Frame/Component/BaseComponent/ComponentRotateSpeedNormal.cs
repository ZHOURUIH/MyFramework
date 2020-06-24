using System;
using UnityEngine;
using System.Collections;

public class ComponentRotateSpeedNormal : ComponentRotateSpeedBase
{
	public override void update(float elapsedTime) 
	{
		if (mPlayState == PLAY_STATE.PS_PLAY && !(isVectorZero(ref mRotateSpeed) && isVectorZero(ref mRotateAcceleration)))
		{
			mCurRotation += mRotateSpeed * elapsedTime;
			adjustAngle360(ref mCurRotation);
			applyRotation(ref mCurRotation, false);
			mRotateSpeed += mRotateAcceleration * elapsedTime;
		}
		base.update(elapsedTime);
	}
}