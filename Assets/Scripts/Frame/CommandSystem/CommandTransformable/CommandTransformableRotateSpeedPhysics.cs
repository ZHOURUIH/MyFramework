using UnityEngine;
using System.Collections;

public class CommandMovableObjectRotateSpeedPhysics : Command
{
	public Vector3 mStartAngle;
	public Vector3 mRotateSpeed;
	public Vector3 mRotateAcceleration;
	public override void init()
	{
		base.init();
		mStartAngle = Vector3.zero;
		mRotateSpeed = Vector3.zero;
		mRotateAcceleration = Vector3.zero;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		TransformableComponentRotateSpeedPhysics component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setActive(true);
		component.startRotateSpeed(mStartAngle, mRotateSpeed, mRotateAcceleration);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mStartAngle:" + mStartAngle + ", mRotateSpeed:" + mRotateSpeed + ", mRotateAcceleration:" + mRotateAcceleration;
	}
}