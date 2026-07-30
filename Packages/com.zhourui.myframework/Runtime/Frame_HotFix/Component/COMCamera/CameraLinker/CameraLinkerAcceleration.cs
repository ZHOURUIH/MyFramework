using UnityEngine;
using static MathUtility;

// 第三人称的摄像机连接器,与连接的物体的相对坐标会随着加速度的增加而增加
public class CameraLinkerAcceleration : CameraLinkerThirdPerson
{
	protected Spring mSpringX = new();		// X轴的弹簧
	protected Spring mSpringY = new();		// Y轴的弹簧
	protected Spring mSpringZ = new();		// Z轴的弹簧
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
		acceleration = acceleration.rotateVector3(curRelative.getAngleFromVector3()) * -1.0f;
		mSpringX.setCurLength(curRelative.x.abs());
		mSpringX.setForce(acceleration.x);
		mSpringY.setCurLength(curRelative.y.abs());
		mSpringY.setForce(acceleration.y);
		mSpringZ.setCurLength(curRelative.z.abs());
		mSpringZ.setForce(acceleration.z);

		mSpringX.setNormaLength(mRelativePosition.x.abs());
		mSpringY.setNormaLength(mRelativePosition.y.abs());
		mSpringZ.setNormaLength(mRelativePosition.z.abs());
		mSpringX.setCurLength(mRelativePosition.x.abs());
		mSpringY.setCurLength(mRelativePosition.y.abs());
		mSpringZ.setCurLength(mRelativePosition.z.abs());
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
			relative = mRelativePosition.rotateVector3(mLinkObject.getRotation().y.toRadian());
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
		applyRelativePosition(new(curX, curY, curZ));
	}
	protected void processRelative(Spring spring, float relative, float acceleration, out float curRelative)
	{
		if (!relative.isFloatZero())
		{
			curRelative = spring.getLength() * sign(relative);
		}
		else
		{
			curRelative = spring.getLength() * sign(acceleration);
		}
	}
}