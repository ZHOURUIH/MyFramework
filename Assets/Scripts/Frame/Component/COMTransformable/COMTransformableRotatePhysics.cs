using UnityEngine;
using System;
using static MathUtility;

// 在物理更新中旋转物体的组件
public class COMTransformableRotatePhysics : ComponentKeyFramePhysics, IComponentModifyRotation
{
	protected Vector3 mStart;	// 起始旋转值
	protected Vector3 mTarget;	// 目标旋转值
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Vector3.zero;
		mTarget = Vector3.zero;
	}
	public void setStart(Vector3 rot){ mStart = rot;}
	public void setTarget(Vector3 rot){ mTarget = rot;}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setRotation(lerpSimple(mStart, mTarget, value));
	}
}