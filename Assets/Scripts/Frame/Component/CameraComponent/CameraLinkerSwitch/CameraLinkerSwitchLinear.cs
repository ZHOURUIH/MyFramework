using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraLinkerSwitchLinear : CameraLinkerSwitch
{
	public float mMovedDistance;
	public float mDistance;		// 此次转换的总长度,用于避免不必要的向量长度重复计算
	public Vector3 mDirection;	// 此次转换的方向,用于避免不必要的向量重复计算
	public CameraLinkerSwitchLinear()
	{
		mMovedDistance = 0.0f;
		mDirection = Vector3.zero;
		mSpeed = 7.0f;
	}
	public override void init(Vector3 origin, Vector3 target, float speed)
	{
		base.init(origin, target, speed);
		mMovedDistance = 0.0f;
		mDistance = getLength(mOriginRelative - mTargetRelative);
		mDirection = normalize(mTargetRelative - mOriginRelative);
	}
	public override void update(float elapsedTime)
	{
		if (mParentLinker == null)
		{
			return;
		}
		mMovedDistance += mSpeed * elapsedTime;
		if (mMovedDistance < mDistance)
		{
			Vector3 newPos = mOriginRelative + mDirection * mMovedDistance;
			mParentLinker.setRelativePosition(newPos);
		}
		else
		{
			// 如果已经超过了转换距离,则设置校正位置到指定点,并且通知连接器转换完成
			mMovedDistance = mDistance;
			mParentLinker.setRelativePosition(mTargetRelative);
			mParentLinker.notifyFinishSwitching(this);
		}
	}
	public override void destroy()
	{
		base.destroy();
		mMovedDistance = 0.0f;
	}
}