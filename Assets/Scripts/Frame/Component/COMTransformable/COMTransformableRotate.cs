using UnityEngine;
using System;

// 物体的旋转组件
public class COMTransformableRotate : ComponentKeyFrameNormal, IComponentModifyRotation
{
	protected Vector3 mStart;		// 起始的旋转角度
	protected Vector3 mTarget;		// 目标的旋转角度
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Vector3.zero;
		mTarget = Vector3.zero;
	}
	public void setStart(Vector3 rot) { mStart = rot; }
	public void setTarget(Vector3 rot){	mTarget = rot; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setRotation(lerpSimple(mStart, mTarget, value));
	}
}