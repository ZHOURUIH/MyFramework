using UnityEngine;
using System.Collections;

public class CommandTransformableRotateFixed : Command
{
	public Vector3 mFixedEuler;
	public bool mActive;
	public override void init()
	{
		base.init();
		mFixedEuler = Vector3.zero;
		mActive = true;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentRotateFixed component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(Typeof(component));
		component.setActive(mActive);
		component.setFixedEuler(mFixedEuler);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mActive:" + mActive + ", mFixedEuler:" + mFixedEuler; 
	}
}