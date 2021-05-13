using UnityEngine;
using System;

public class COMTransformableScale : ComponentKeyFrameNormal, IComponentModifyScale
{
	protected Vector3 mStart;
	protected Vector3 mTarget;
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Vector3.zero;
		mTarget = Vector3.zero;
	}
	public void setStart(Vector3 start) { mStart = start; }
	public void setTarget(Vector3 target) { mTarget = target; }
	//--------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setScale(lerpSimple(mStart, mTarget, value));
	}
}