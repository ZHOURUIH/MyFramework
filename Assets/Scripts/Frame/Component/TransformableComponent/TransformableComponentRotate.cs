using UnityEngine;
using System;

public class TransformableComponentRotate : ComponentKeyFrameNormal, IComponentModifyRotation
{
	protected Vector3 mStartRotation;
	protected Vector3 mTargetRotation;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartRotation = Vector3.zero;
		mTargetRotation = Vector3.zero;
	}
	public void setStartRotation(Vector3 rot){mStartRotation = rot;}
	public void setTargetRotation(Vector3 rot){	mTargetRotation = rot;}
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setRotation(lerpSimple(mStartRotation, mTargetRotation, value));
	}
}