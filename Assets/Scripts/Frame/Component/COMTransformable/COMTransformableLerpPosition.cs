using UnityEngine;
using System;

// 用于插值物体位置的组件
public class COMTransformableLerpPosition : ComponentLerpPosition
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyPosition(Vector3 position)
	{
		(mComponentOwner as Transformable).setPosition(position);
	}
	protected override Vector3 getPosition()
	{
		return (mComponentOwner as Transformable).getPosition();
	}
}