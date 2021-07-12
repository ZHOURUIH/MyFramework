using System;

public class ComponentRotateSpeedNormal : ComponentRotateSpeed
{
	public override void update(float elapsedTime) 
	{
		if (mPlayState == PLAY_STATE.PLAY && !(isVectorZero(mRotateSpeed) && isVectorZero(mRotateAcceleration)))
		{
			mCurRotation += mRotateSpeed * elapsedTime;
			adjustAngle360(ref mCurRotation);
			applyRotation(ref mCurRotation);
			mRotateSpeed += mRotateAcceleration * elapsedTime;
		}
		base.update(elapsedTime);
	}
}