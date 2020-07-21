using UnityEngine;
using System;
using System.Collections;

public class TimeComponentScale : ComponentKeyFrameNormal
{
	protected float mStartScale;
	protected float mTargetScale;
	public void setStartScale(float scale) { mStartScale = scale;}
	public void setTargetScale(float scale) { mTargetScale = scale; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		float scale = lerpSimple(mStartScale, mTargetScale, value);
		clampMin(ref scale, 0.0f);
		Time.timeScale = scale;
	}
}
