using System;

public class WindowComponentSlider : ComponentKeyFrameNormal
{
	protected float mStartValue;   // 移动开始时的位置
	protected float mTargetValue;
	public void setTargetValue(float value) { mTargetValue = value; }
	public void setStartValue(float value) { mStartValue = value; }
	//-----------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if(!(mComponentOwner is ISlider))
		{
			logError("window is not a Slider Window!");
			return;
		}
		(mComponentOwner as ISlider).setValue(lerpSimple(mStartValue, mTargetValue, value));
	}
}