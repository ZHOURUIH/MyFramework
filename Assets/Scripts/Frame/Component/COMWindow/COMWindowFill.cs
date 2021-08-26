using System;

// 用于渐变窗口填充度的组件
public class COMWindowFill : ComponentKeyFrameNormal
{
	protected float mStart;		// 起始填充值
	protected float mTarget;	// 目标填充值
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
		var window = mComponentOwner as myUIObject;
		window.setFillPercent(lerpSimple(mStart, mTarget, value));
	}
}