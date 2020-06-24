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
		ComponentOwner obj = mReceiver as ComponentOwner;
		TransformableComponentRotateFixed component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setActive(mActive);
		component.setFixedEuler(mFixedEuler);	
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mActive:" + mActive + ", mFixedEuler:" + mFixedEuler; 
	}
}