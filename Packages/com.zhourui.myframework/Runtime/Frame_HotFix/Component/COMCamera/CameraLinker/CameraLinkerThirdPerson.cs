using UnityEngine;

// 第三人称的摄像机连接器,与连接的物体保持固定的相对坐标
public class CameraLinkerThirdPerson : CameraLinker
{
	public override void resetProperty()
	{
		base.resetProperty();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
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
		applyRelativePosition(relative);
	}
}