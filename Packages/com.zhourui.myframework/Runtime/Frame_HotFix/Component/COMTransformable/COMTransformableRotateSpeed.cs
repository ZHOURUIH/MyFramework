using UnityEngine;

// 使物体按速度旋转的组件
public class COMTransformableRotateSpeed : ComponentRotateSpeed
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyRotation(ref Vector3 rotation)
	{
		(mComponentOwner as ITransformable).setRotation(rotation);
	}
	protected override Vector3 getCurRotation() { return (mComponentOwner as ITransformable).getRotation(); }
}