using UnityEngine;

public class CommandTransformableRotateFixedPhysics : Command
{
	public Vector3 mFixedEuler;
	public bool mActive;
	public override void resetProperty()
	{
		base.resetProperty();
		mFixedEuler = Vector3.zero;
		mActive = true;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentRotateFixedPhysics component);
		component.setActive(mActive);
		component.setFixedEuler(mFixedEuler);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mActive:", mActive).
				Append(", mFixedEuler:", mFixedEuler);
	}
}