using System;

// 渐变UI滑动条值的组件
public class COMWindowSlider : ComponentKeyFrameNormal
{
	protected float mStart;		// 起始值
	protected float mTarget;	// 目标值
	public void setTarget(float value) { mTarget = value; }
	public void setStart(float value) { mStart = value; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	//------------------------------------------------------------------------------------------------------------------------------
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