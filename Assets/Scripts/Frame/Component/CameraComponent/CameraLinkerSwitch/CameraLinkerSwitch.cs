using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraLinkerSwitch : GameBase
{
	protected CAMERA_LINKER_SWITCH mType;
	protected CameraLinker mParentLinker;
	protected Vector3 mOriginRelative;
	protected Vector3 mTargetRelative;
	// 转换器的速度,不同的转换器速度含义不同
	// 直线转换器是直线速度
	// 环形转换器是角速度
	// 绕目标旋转转换器是角速度
	protected float mSpeed;
	public CameraLinkerSwitch()
	{
		mOriginRelative = Vector3.zero;
		mTargetRelative = Vector3.zero;
	}
	public void initType(CAMERA_LINKER_SWITCH type, CameraLinker parentLinker)
	{
		mType = type;
		mParentLinker = parentLinker;
	}
	public virtual void init(Vector3 origin, Vector3 target, float speed)
	{
		mOriginRelative = origin;
		mTargetRelative = target;
		mSpeed = speed;
	}
	public abstract void update(float elapsedTime);
	public virtual void destroy()
	{
		mParentLinker = null;
	}
	public CameraLinker getParent() { return mParentLinker; }
	public void setOriginRelative(Vector3 origin) { mOriginRelative = origin; }
	public void setTargetRelative(Vector3 target) { mTargetRelative = target; }
	public Vector3 getOriginRelative() { return mOriginRelative; }
	public Vector3 getTargetRelative() { return mTargetRelative; }
	public void setSwitchSpeed(float speed) { mSpeed = speed; }
	public float getSwitchSpeed() { return mSpeed; }
}