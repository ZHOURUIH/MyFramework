using UnityEngine;
using static MathUtility;

// 用于执行切换连接器时的不同行为,绕目标旋转来切换连接器,并且相对位置的长度上是插值
public class CameraLinkerSwitchAroundTarget : CameraLinkerSwitch
{
	protected float mDistanceCurrent;		// 当前的相对距离
	protected float mDistanceDelta;			// 初始位置到目标位置的距离差值
	protected float mRotatedAngle;			// 已经旋转过的角度
	protected float mTotalAngle;			// 总共需要旋转的角度
	protected bool mClockwise;				// 是否顺时针旋转
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
		mDistanceDelta = getLength(mTargetRelative) - getLength(mOriginRelative);
		mDistanceCurrent = 0.0f;
		mRotatedAngle = 0.0f;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDistanceCurrent = 0.0f;
		mDistanceDelta = 0.0f;
		mRotatedAngle = 0.0f;
		mTotalAngle = 0.0f;
		mClockwise = true;
		mSpeed = HALF_PI_RADIAN;
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
				Vector3 projectVec = normalize(resetY(rotateAxis)) * (getLength(mOriginRelative) + mDistanceCurrent);
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
				Vector3 projectVec = normalize(resetY(rotateAxis)) * (getLength(mOriginRelative) + mDistanceCurrent);
				rotateAxis.y = (mTargetRelative.y - mOriginRelative.y) * (mRotatedAngle / mTotalAngle) + mOriginRelative.y;
				rotateAxis.x = projectVec.x;
				rotateAxis.z = projectVec.z;
				mLinker.setRelativePosition(rotateAxis);
			}
		}
	}
}