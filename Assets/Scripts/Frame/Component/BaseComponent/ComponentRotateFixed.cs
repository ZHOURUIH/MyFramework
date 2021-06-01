using UnityEngine;

public class ComponentRotateFixed : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	public Vector3 mFixedEuler;
	public override void resetProperty()
	{
		base.resetProperty();
		mFixedEuler = Vector3.zero;
	}
	public void setFixedEuler(Vector3 euler) { mFixedEuler = euler; }
	public Vector3 getFiexdEuler() { return mFixedEuler; }
	public void notifyBreak(){}
}
