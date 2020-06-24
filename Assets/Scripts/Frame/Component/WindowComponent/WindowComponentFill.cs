using System;
using System.Collections.Generic;

public class WindowComponentFill : ComponentKeyFrameNormal
{
	protected float mStartValue;   // 移动开始时的位置
	protected float mTargetValue;
	public void setTargetValue(float value) { mTargetValue = value; }
	public void setStartValue(float value) { mStartValue = value; }
	//------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		txUIObject window = mComponentOwner as txUIObject;
		window.setFillPercent(lerpSimple(mStartValue, mTargetValue, value));
	}
}