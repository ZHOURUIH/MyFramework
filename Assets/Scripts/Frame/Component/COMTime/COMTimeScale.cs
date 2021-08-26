using UnityEngine;
using System;

// 用于渐变时间缩放的组件
public class COMTimeScale : ComponentKeyFrameNormal
{
	protected float mStart;		// 起始缩放值
	protected float mTarget;	// 目标缩放值
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