using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentLerpRotation : ComponentLerp, IComponentModifyRotation
{
	protected Vector3 mTargetRotation;
	protected float mMinRange;
	public ComponentLerpRotation()
	{
		mMinRange = 0.001f;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		Vector3 curRot = lerp(getRotation(), mTargetRotation, mLerpSpeed * elapsedTime, mMinRange);
		applyRotation(curRot);
		afterApplyLerp(isVectorEqual(curRot, mTargetRotation));
	}
	public override void play()
	{
		if (isVectorEqual(getRotation(), mTargetRotation))
		{
			stop();
			return;
		}
		base.play();
	}
	public void setTargetRotation(Vector3 target) { mTargetRotation = target; }
	public Vector3 getTargetRotation() { return mTargetRotation; }
	public void setMinRange(float minRange) { mMinRange = minRange; }
	public float getMinRange() { return mMinRange; }
	//----------------------------------------------------------------------------------------------------------------------------
	protected abstract void applyRotation(Vector3 rotation);
	protected abstract Vector3 getRotation();
}