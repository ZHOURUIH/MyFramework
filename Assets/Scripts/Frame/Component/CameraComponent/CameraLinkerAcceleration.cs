using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraLinkerAcceleration : CameraLinker
{
	protected Spring mSpringX;
	protected Spring mSpringY;
	protected Spring mSpringZ;
	protected bool mUseTargetYaw;           // 是否使用目标物体的旋转来旋转摄像机的位置
	public CameraLinkerAcceleration()
	{
		mSpringX = new Spring();
		mSpringY = new Spring();
		mSpringZ = new Spring();
		mUseTargetYaw = true;
	}
	public override void setRelativePosition(Vector3 pos, Type switchType = null, bool useDefaultSwitchSpeed = true, float switchSpeed = 1.0f)
	{
		base.setRelativePosition(pos, switchType, useDefaultSwitchSpeed, switchSpeed);
		// 获得加速度
		Vector3 acceleration = mLinkObject.getPhysicsAcceleration();
		Vector3 curRelative = (mComponentOwner as GameCamera).getPosition() - mLinkObject.getPosition();
		float relativeAngle = getAngleFromVector(curRelative);
		acceleration = rotateVector3(acceleration, relativeAngle) * -1.0f;
		mSpringX.setCurLength(abs(curRelative.x));
		mSpringX.setForce(acceleration.x);
		mSpringY.setCurLength(abs(curRelative.y));
		mSpringY.setForce(acceleration.y);
		mSpringZ.setCurLength(abs(curRelative.z));
		mSpringZ.setForce(acceleration.z);

		mSpringX.setNormaLength(abs(mRelativePosition.x));
		mSpringY.setNormaLength(abs(mRelativePosition.y));
		mSpringZ.setNormaLength(abs(mRelativePosition.z));
		mSpringX.setCurLength(abs(mRelativePosition.x));
		mSpringY.setCurLength(abs(mRelativePosition.y));
		mSpringZ.setCurLength(abs(mRelativePosition.z));
		mSpringX.setForce(0.0f);
		mSpringY.setForce(0.0f);
		mSpringZ.setForce(0.0f);
		mSpringX.setSpeed(0.0f);
		mSpringY.setSpeed(0.0f);
		mSpringZ.setSpeed(0.0f);
		// 改变摄像机位置
		Vector3 targetPos = mLinkObject.getPosition();
		(mComponentOwner as GameCamera).setPosition(targetPos + mRelativePosition);
	}
	public void setUseTargetYaw(bool use) { mUseTargetYaw = use; }
	public bool isUseTargetYaw() { return mUseTargetYaw; }
	//----------------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		mSpringX.update(elapsedTime);
		mSpringY.update(elapsedTime);
		mSpringZ.update(elapsedTime);
		// 如果使用目标物体的航向角,则对相对位置进行旋转
		Vector3 relative = mRelativePosition;
		if (mUseTargetYaw)
		{
			relative = rotateVector3(relative, toRadian(mLinkObject.getRotation().y));
		}
		//判断是否为零
		float curX = 0.0f;
		float curY = 0.0f;
		float curZ = 0.0f;
		Vector3 acceleration = mLinkObject.getPhysicsAcceleration();
		processRelative(mSpringX, relative.x, acceleration.x, ref curX);
		processRelative(mSpringY, relative.y, acceleration.y, ref curY);
		processRelative(mSpringZ, relative.z, acceleration.z, ref curZ);
		// 改变摄像机位置
		applyRelativePosition(new Vector3(curX, curY, curZ));
	}
	protected static void processRelative(Spring spring, float relative, float acceleration, ref float curRelative)
	{
		if (isFloatZero(relative))
		{
			if (!isFloatZero(acceleration))
			{
				curRelative = spring.getLength() * acceleration / abs(acceleration);
			}
			return;
		}
		curRelative = spring.getLength() * relative / abs(relative);
	}
}