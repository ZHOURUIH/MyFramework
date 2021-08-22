using UnityEngine;
using System;

public class COMTransformableRotate : ComponentKeyFrameNormal, IComponentModifyRotation
{
	protected Vector3 mStart;
	protected Vector3 mTarget;
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