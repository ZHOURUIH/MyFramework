using System;
using static MathUtility;

// 在Update中执行旋转
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