using UnityEngine;
using System;

// 在物理更新中锁定物体旋转的组件
public class COMTransformableRotateFixedPhysics : ComponentRotateFixed
{
	public override void fixedUpdate(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.fixedUpdate(elapsedTime);
	}
}