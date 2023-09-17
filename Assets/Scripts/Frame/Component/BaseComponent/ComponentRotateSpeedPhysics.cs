using System;
using static MathUtility;

// 在物理更新中执行旋转
public class ComponentRotateSpeedPhysics : ComponentRotateSpeed
{
	public override void fixedUpdate(float elapsedTime) 
	{
		if (mPlayState == PLAY_STATE.PLAY && !(isVectorZero(mRotateSpeed) && isVectorZero(mRotateAcceleration)))
		{
			mCurRotation += mRotateSpeed * elapsedTime;
			adjustAngle360(ref mCurRotation);
			applyRotation(ref mCurRotation);
			mRotateSpeed += mRotateAcceleration * elapsedTime;
		}
		base.fixedUpdate(elapsedTime);
	}
}