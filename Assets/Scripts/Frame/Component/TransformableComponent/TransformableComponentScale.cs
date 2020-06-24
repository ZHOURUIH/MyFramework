using UnityEngine;
using System.Collections;
using System;

public class TransformableComponentScale : ComponentKeyFrameNormal, IComponentModifyScale
{
	protected Vector3 mStartScale;
	protected Vector3 mTargetScale;
	public void setStartScale(Vector3 start){mStartScale = start;}
	public void setTargetScale(Vector3 target){mTargetScale = target;}
	//--------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setScale(lerpSimple(mStartScale, mTargetScale, value));
	}
}