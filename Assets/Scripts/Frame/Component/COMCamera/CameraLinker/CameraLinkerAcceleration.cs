using System;
using UnityEngine;
using static MathUtility;

// 第三人称的摄像机连接器,与连接的物体的相对坐标会随着加速度的增加而增加
public class CameraLinkerAcceleration : CameraLinkerThirdPerson
{
	protected Spring mSpringX;		// X轴的弹簧
	protected Spring mSpringY;		// Y轴的弹簧
	protected Spring mSpringZ;		// Z轴的弹簧
	public CameraLinkerAcceleration()
	{
		mSpringX = new Spring();
		mSpringY = new Spring();
		mSpringZ = new Spring();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mSpringX.resetProperty();
		mSpringY.resetProperty();
		mSpringZ.resetProperty();
	}
	public override void setRelativePosition(Vector3 pos)
	{
		base.setRelativePosition(pos);
		// 获得加速度
		Vector3 acceleration = mLinkObject.getPhysicsAcceleration();
		Vector3 curRelative = mCamera.getPosition() - mLinkObject.getPosition();
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
		mCamera.setPosition(mLinkObject.getPosition() + mRelativePosition);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		mSpringX.update(elapsedTime);
		mSpringY.update(elapsedTime);
		mSpringZ.update(elapsedTime);
		// 如果使用目标物体的航向角,则对相对位置进行旋转
		Vector3 relative;
		if (mUseTargetYaw)
		{
			relative = rotateVector3(mRelativePosition, toRadian(mLinkObject.getRotation().y));
		}
		else
		{
			relative = mRelativePosition;
		}
		// 判断是否为零
		Vector3 acceleration = mLinkObject.getPhysicsAcceleration();
		processRelative(mSpringX, relative.x, acceleration.x, out float curX);
		processRelative(mSpringY, relative.y, acceleration.y, out float curY);
		processRelative(mSpringZ, relative.z, acceleration.z, out float curZ);
		// 改变摄像机位置
		applyRelativePosition(new Vector3(curX, curY, curZ));
	}
	protected void processRelative(Spring spring, float relative, float acceleration, out float curRelative)
	{
		curRelative = 0.0f;
		if (isFloatZero(relative))
		{
			if (!isFloatZero(acceleration))
			{
				curRelative = spring.getLength() * acceleration / abs(acceleration);
			}
		}
		else
		{
			curRelative = spring.getLength() * relative / abs(relative);
		}
	}
}