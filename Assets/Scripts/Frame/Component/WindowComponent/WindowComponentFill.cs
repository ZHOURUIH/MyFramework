using System;

public class WindowComponentFill : ComponentKeyFrameNormal
{
	protected float mStartValue;   // 移动开始时的位置
	protected float mTargetValue;
	public void setTargetValue(float value) { mTargetValue = value; }
	public void setStartValue(float value) { mStartValue = value; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStartValue = 0.0f;
		mTargetValue = 0.0f;
	}
	//------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var window = mComponentOwner as myUIObject;
		window.setFillPercent(lerpSimple(mStartValue, mTargetValue, value));
	}
}