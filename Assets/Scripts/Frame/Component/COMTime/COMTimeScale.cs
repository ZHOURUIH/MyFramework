using UnityEngine;
using System;

public class COMTimeScale : ComponentKeyFrameNormal
{
	protected float mStart;
	protected float mTarget;
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	public void setStart(float scale) { mStart = scale;}
	public void setTarget(float scale) { mTarget = scale; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		Time.timeScale = clampMin(lerpSimple(mStart, mTarget, value));
	}
}