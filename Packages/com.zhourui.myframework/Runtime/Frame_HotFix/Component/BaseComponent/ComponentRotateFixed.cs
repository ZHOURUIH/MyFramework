using UnityEngine;

// 锁定旋转的组件
public class ComponentRotateFixed : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	public Vector3 mFixedEuler;		// 锁定的旋转值
	public override void resetProperty()
	{
		base.resetProperty();
		mFixedEuler = Vector3.zero;
	}
	public void setFixedEuler(Vector3 euler) { mFixedEuler = euler; }
	public Vector3 getFiexdEuler() { return mFixedEuler; }
	public void notifyBreak(){}
}