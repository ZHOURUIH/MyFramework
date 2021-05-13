using UnityEngine;

// 相对位置固定,但是旋转会平滑过渡
public class CameraLinkerSmoothRotate : CameraLinker
{
	protected Vector3 mCurRelative;
	protected float mSmoothRotateSpeed;
	public CameraLinkerSmoothRotate()
	{
		mSmoothRotateSpeed = 5.0f;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCurRelative = Vector3.zero;
		mSmoothRotateSpeed = 5.0f;
	}
	//-------------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		// 如果使用目标物体的航向角,则对相对位置进行旋转
		float targetRadianYaw = toRadian(mLinkObject.getRotation().y);
		// 使用摄像机自身的航向角计算相对位置的航向角
		float curYaw = getVectorYaw(-mCurRelative);
		adjustRadian360(ref targetRadianYaw);
		adjustRadian360(ref curYaw);
		// 调整角度范围
		if (abs(curYaw - targetRadianYaw) > PI_RADIAN)
		{
			adjustRadian180(ref targetRadianYaw);
			adjustRadian180(ref curYaw);
		}
		curYaw = lerp(curYaw, targetRadianYaw, elapsedTime * mSmoothRotateSpeed, 0.01f);
		adjustRadian360(ref targetRadianYaw);
		adjustRadian360(ref curYaw);
		float curPitch = getVectorPitch(-mRelativePosition);
		Vector3 newRelative = -getDirectionFromRadianYawPitch(curYaw, curPitch) * getLength(ref mRelativePosition);
		applyRelativePosition(newRelative);
	}
	public override void applyRelativePosition(Vector3 relative)
	{
		base.applyRelativePosition(relative);
		mCurRelative = relative;
	}
}