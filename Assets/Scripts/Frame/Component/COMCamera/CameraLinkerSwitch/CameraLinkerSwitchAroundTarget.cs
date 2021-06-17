using UnityEngine;

public class CameraLinkerSwitchAroundTarget : CameraLinkerSwitch
{
	protected float mDistanceCurrent;
	protected float mDistanceDelta;
	protected float mRotatedAngle;
	protected float mTotalAngle;
	protected bool mClockwise;
	public CameraLinkerSwitchAroundTarget()
	{
		mClockwise = true;
		mSpeed = HALF_PI_RADIAN;
	}
	public override void init(Vector3 origin, Vector3 target, float speed)
	{
		base.init(origin, target, speed);
		if (mClockwise)
		{
			mTotalAngle = getAngleFromVector(mTargetRelative) - getAngleFromVector(mOriginRelative);
			adjustRadian360(ref mTotalAngle);
			mSpeed = abs(mSpeed);
		}
		else
		{
			mTotalAngle = getAngleFromVector(mOriginRelative) - getAngleFromVector(mTargetRelative);
			adjustRadian360(ref mTotalAngle);
			mSpeed = -abs(mSpeed);
		}
		mDistanceDelta = getLength(ref mTargetRelative) - getLength(ref mOriginRelative);
		mDistanceCurrent = 0.0f;
		mRotatedAngle = 0.0f;
	}
	public override void update(float elapsedTime)
	{
		if (mLinker == null)
		{
			return;
		}
		mRotatedAngle += mSpeed * elapsedTime;
		// 计算速度
		mDistanceCurrent += mDistanceDelta / (mTotalAngle / mSpeed) * elapsedTime;

		// 顺时针
		if (mClockwise)
		{
			if (mRotatedAngle >= mTotalAngle)
			{
				mRotatedAngle = mTotalAngle;
				mLinker.setRelativePosition(mTargetRelative);
				mLinker.notifyFinishSwitching();
				mDistanceCurrent = 0.0f;
				mRotatedAngle = 0.0f;
			}
			else
			{
				// z方向上旋转后的轴
				Vector3 rotateAxis = rotateVector3(mOriginRelative, mRotatedAngle);
				// 距离变化
				Vector3 projectVec = normalize(resetY(rotateAxis)) * (getLength(ref mOriginRelative) + mDistanceCurrent);
				// 高度变化
				rotateAxis.y = (mTargetRelative.y - mOriginRelative.y) * (mRotatedAngle / mTotalAngle) + mOriginRelative.y;
				// 最终值
				rotateAxis.x = projectVec.x;
				rotateAxis.z = projectVec.z;
				mLinker.setRelativePosition(rotateAxis);
			}
		}
		// 逆时针
		else
		{
			if (mRotatedAngle <= mTotalAngle)
			{
				mRotatedAngle = mTotalAngle;
				mLinker.setRelativePosition(mTargetRelative);
				mLinker.notifyFinishSwitching();
				mDistanceCurrent = 0.0f;
				mRotatedAngle = 0.0f;
			}
			else
			{
				Vector3 rotateAxis = rotateVector3(mOriginRelative, mRotatedAngle);
				Vector3 projectVec = normalize(resetY(rotateAxis)) * (getLength(ref mOriginRelative) + mDistanceCurrent);
				rotateAxis.y = (mTargetRelative.y - mOriginRelative.y) * (mRotatedAngle / mTotalAngle) + mOriginRelative.y;
				rotateAxis.x = projectVec.x;
				rotateAxis.z = projectVec.z;
				mLinker.setRelativePosition(rotateAxis);
			}
		}
	}
}