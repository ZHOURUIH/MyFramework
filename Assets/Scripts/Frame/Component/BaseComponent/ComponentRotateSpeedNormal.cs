using System;

public class ComponentRotateSpeedNormal : ComponentRotateSpeedBase
{
	public override void update(float elapsedTime) 
	{
		if (mPlayState == PLAY_STATE.PLAY && !(isVectorZero(ref mRotateSpeed) && isVectorZero(ref mRotateAcceleration)))
		{
			mCurRotation += mRotateSpeed * elapsedTime;
			adjustAngle360(ref mCurRotation);
			applyRotation(ref mCurRotation);
			mRotateSpeed += mRotateAcceleration * elapsedTime;
		}
		base.update(elapsedTime);
	}
}