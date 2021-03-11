using UnityEngine;
using System;

public class TimeComponentScale : ComponentKeyFrameNormal
{
	protected float mStartScale;
	protected float mTargetScale;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartScale = 0.0f;
		mTargetScale = 0.0f;
	}
	public void setStartScale(float scale) { mStartScale = scale;}
	public void setTargetScale(float scale) { mTargetScale = scale; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		Time.timeScale = clampMin(lerpSimple(mStartScale, mTargetScale, value));
	}
}
