using UnityEngine;

public class CommandTransformableRotateSpeed : Command
{
	public Vector3 mStartAngle;
	public Vector3 mRotateSpeed;
	public Vector3 mRotateAcceleration;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartAngle = Vector3.zero;
		mRotateSpeed = Vector3.zero;
		mRotateAcceleration = Vector3.zero;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentRotateSpeed component);
		component.setActive(true);
		component.startRotateSpeed(mStartAngle, mRotateSpeed, mRotateAcceleration);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mStartAngle:", mStartAngle).
				Append(", mRotateSpeed:", mRotateSpeed).
				Append(", mRotateAcceleration:", mRotateAcceleration);
	}
}