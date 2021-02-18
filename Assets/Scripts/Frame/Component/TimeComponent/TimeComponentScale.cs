using UnityEngine;
using System;

public class TimeComponentScale : ComponentKeyFrameNormal
{
	protected float mStartScale;
	protected float mTargetScale;
	public void setStartScale(float scale) { mStartScale = scale;}
	public void setTargetScale(float scale) { mTargetScale = scale; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		Time.timeScale = clampMin(lerpSimple(mStartScale, mTargetScale, value));
	}
}
