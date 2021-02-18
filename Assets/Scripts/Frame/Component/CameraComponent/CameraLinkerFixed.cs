using System;
using UnityEngine;

public class CameraLinkerFixed : CameraLinker
{
	protected bool mUseTargetYaw;           // 是否使用目标物体的旋转来旋转摄像机的位置
	public CameraLinkerFixed()
	{
		mUseTargetYaw = true;
	}
	public void setUseTargetYaw(bool use) { mUseTargetYaw = use; }
	public bool isUseTargetYaw() { return mUseTargetYaw; }
	//---------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		// 如果使用目标物体的航向角,则对相对位置进行旋转
		Vector3 relative = mRelativePosition;
		if (mUseTargetYaw)
		{
			relative = rotateVector3(relative, toRadian(mLinkObject.getRotation().y));
		}
		applyRelativePosition(relative);
	}
}