using System;
using UnityEngine;

public abstract class ComponentLerpPosition : ComponentLerp, IComponentModifyPosition
{
	protected Vector3 mTargetPosition;
	protected float mMinRange;
	public ComponentLerpPosition()
	{
		mMinRange = 0.001f;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		Vector3 curPos = lerp(getPosition(), mTargetPosition, mLerpSpeed * elapsedTime, mMinRange);
		applyPosition(curPos);
		afterApplyLerp(isVectorEqual(curPos, mTargetPosition));
	}
	public override void play()
	{
		if (isVectorEqual(getPosition(), mTargetPosition))
		{
			stop();
			return;
		}
		base.play();
	}
	public void setTargetPosition(Vector3 target) { mTargetPosition = target; }
	public Vector3 getTargetPosition() { return mTargetPosition; }
	public void setMinRange(float minRange) { mMinRange = minRange; }
	public float getMinRange() { return mMinRange; }
	//----------------------------------------------------------------------------------------------------------------------------
	protected abstract void applyPosition(Vector3 position);
	protected abstract Vector3 getPosition();
}