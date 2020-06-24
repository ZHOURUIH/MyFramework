using UnityEngine;
using System;
using System.Collections;

public class ComponentRotateFixedBase : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	public Vector3 mFixedEuler;
	public void setFixedEuler(Vector3 euler) { mFixedEuler = euler; }
	public Vector3 getFiexdEuler() { return mFixedEuler; }
	public void notifyBreak(){}
}
