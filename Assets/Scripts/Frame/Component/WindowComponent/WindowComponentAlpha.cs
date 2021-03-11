using System;

public class WindowComponentAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
{
	protected float mStartAlpha;
	protected float mTargetAlpha;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartAlpha = 0.0f;
		mTargetAlpha = 0.0f;
	}
	public void setStartAlpha(float alpha) {mStartAlpha = alpha;}
	public void setTargetAlpha(float alpha) {mTargetAlpha = alpha;}
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as myUIObject;
		// 此处不使用递归透明度变化是为了尽量不影响其他窗口
		obj.setAlpha(lerpSimple(mStartAlpha, mTargetAlpha, value), false);
	}
}
