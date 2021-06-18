using System;
using UnityEngine;

public class CameraLinkerFixed : CameraLinker
{
	public override void resetProperty()
	{
		base.resetProperty();
	}
	//---------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
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
		applyRelativePosition(relative);
	}
}