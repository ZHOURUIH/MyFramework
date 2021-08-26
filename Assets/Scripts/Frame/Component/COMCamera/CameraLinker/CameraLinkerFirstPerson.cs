using System;
using UnityEngine;

// 第一人称连接器,实际也是保持固定的相对距离
public class CameraLinkerFirstPerson : CameraLinker
{
	public CameraLinkerFirstPerson()
	{
		mLookAtTarget = false;
		mCheckModelBetween = false;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLookAtTarget = false;
		mCheckModelBetween = false;
	}
	// 第一人称摄像机不会看向目标,因为目标就是自己
	public override void setLookAtTarget(bool lookat) { mLookAtTarget = false; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		// 因为是第一人称,所以固定需要保持摄像机的朝向与角色的朝向一致
		applyRelativePosition(rotateVector3(mRelativePosition, toRadian(mLinkObject.getRotation().y)));
		mCamera.setRotationY(mLinkObject.getRotation().y);
	}
}