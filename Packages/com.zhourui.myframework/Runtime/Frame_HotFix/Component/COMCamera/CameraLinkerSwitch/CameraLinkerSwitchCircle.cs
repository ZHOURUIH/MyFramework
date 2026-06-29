using UnityEngine;
using static MathUtility;

// 用于执行切换连接器时的不同行为,水平方向上的弧形运动, 竖直方向上的直线运动
public class CameraLinkerSwitchCircle : CameraLinkerSwitch
{
	protected Vector3 mRotateCenter;    // 高度忽略的旋转圆心
	protected float mRotatedAngle;		// 已经旋转过的角度
	protected float mTotalAngle;		// 总共需要旋转的角度
	public CameraLinkerSwitchCircle()
	{
		mTotalAngle = PI_RADIAN;
		mSpeed = PI_RADIAN;
	}
	public override void init(Vector3 origin, Vector3 target, float speed)
	{
		base.init(origin, target, speed);
		mRotatedAngle = 0.0f;
		mRotateCenter = mOriginRelative + (mTargetRelative - mOriginRelative) * 0.5f;
		mRotateCenter.y = 0.0f;
		mTotalAngle = PI_RADIAN;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRotateCenter = Vector3.zero;
		mRotatedAngle = 0.0f;
		mTotalAngle = PI_RADIAN;
		mSpeed = PI_RADIAN;
	}
	public override void update(float elapsedTime)
	{
		if (mLinker == null)
		{
			return;
		}
		// 旋转转换分为两个部分,一部分是水平方向上的弧形运动, 另一部分是竖直方向上的直线运动
		mRotatedAngle += mSpeed * elapsedTime;
		if (mRotatedAngle >= mTotalAngle)
		{
			mRotatedAngle = mTotalAngle;
			mLinker.setRelativePosition(mTargetRelative);
			mLinker.notifyFinishSwitching();
		}
		else
		{
			Vector3 rotateVec = rotateVector3(resetY(mOriginRelative - mRotateCenter), mRotatedAngle) + mRotateCenter;
			rotateVec.y = (mTargetRelative.y - mOriginRelative.y) * divide(mRotatedAngle, mTotalAngle) + mOriginRelative.y;
			mLinker.setRelativePosition(rotateVec);
		}
	}
}