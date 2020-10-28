using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraLinkerSwitchCircle : CameraLinkerSwitch
{
	protected Vector3 mRotateCenter;    // 高度忽略的旋转圆心
	protected float mRotatedAngle;		// 已经旋转过的角度
	protected float mTotalAngle;
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
	public override void update(float elapsedTime)
	{
		if (mParentLinker == null)
		{
			return;
		}
		// 旋转转换分为两个部分,一部分是水平方向上的弧形运动, 另一部分是竖直方向上的直线运动
		mRotatedAngle += mSpeed * elapsedTime;
		if (mRotatedAngle >= mTotalAngle)
		{
			mRotatedAngle = mTotalAngle;
			mParentLinker.setRelativePosition(mTargetRelative);
			mParentLinker.notifyFinishSwitching(this);
		}
		else
		{
			Vector3 rotateVec = mOriginRelative - mRotateCenter;
			rotateVec.y = 0.0f;
			rotateVec = rotateVector3(rotateVec, mRotatedAngle);
			rotateVec += mRotateCenter;
			rotateVec.y = (mTargetRelative.y - mOriginRelative.y) * (mRotatedAngle / mTotalAngle) + mOriginRelative.y;
			mParentLinker.setRelativePosition(rotateVec);
		}
	}
}