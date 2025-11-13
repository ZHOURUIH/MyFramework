using UnityEngine;

// 用于插值物体旋转的组件
public class COMTransformableLerpRotation : ComponentLerpRotation
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyRotation(Vector3 rotation)
	{
		(mComponentOwner as ITransformable).setRotation(rotation);
	}
	protected override Vector3 getRotation()
	{
		return (mComponentOwner as ITransformable).getRotation();
	}
}