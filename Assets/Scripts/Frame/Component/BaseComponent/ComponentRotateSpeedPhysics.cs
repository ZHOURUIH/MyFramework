using System;

public class ComponentRotateSpeedPhysics : ComponentRotateSpeed
{
	public override void fixedUpdate(float elapsedTime) 
	{
		if (mPlayState == PLAY_STATE.PLAY && !(isVectorZero(ref mRotateSpeed) && isVectorZero(ref mRotateAcceleration)))
		{
			mCurRotation += mRotateSpeed * elapsedTime;
			adjustAngle360(ref mCurRotation);
			applyRotation(ref mCurRotation);
			mRotateSpeed += mRotateAcceleration * elapsedTime;
		}
		base.fixedUpdate(elapsedTime);
	}
}