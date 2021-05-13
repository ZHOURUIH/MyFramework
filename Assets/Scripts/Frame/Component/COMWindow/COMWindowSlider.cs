using System;

public class COMWindowSlider : ComponentKeyFrameNormal
{
	protected float mStart;   // 移动开始时的位置
	protected float mTarget;
	public void setTarget(float value) { mTarget = value; }
	public void setStart(float value) { mStart = value; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	//-----------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if(!(mComponentOwner is ISlider))
		{
			logError("window is not a Slider Window!");
			return;
		}
		(mComponentOwner as ISlider).setValue(lerpSimple(mStart, mTarget, value));
	}
}