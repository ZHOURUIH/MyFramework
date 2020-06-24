using UnityEngine;
using System;
using System.Collections;

public class MovableObjectComponentAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
{
	protected float mStartAlpha;
	protected float mTargetAlpha;
	public void setStartAlpha(float alpha) {mStartAlpha = alpha;}
	public void setTargetAlpha(float alpha) {mTargetAlpha = alpha;}
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float offset)
	{
		MovableObject obj = mComponentOwner as MovableObject;
		float newAlpha = lerpSimple(mStartAlpha, mTargetAlpha, offset);
		obj.setAlpha(newAlpha);
	}
}
