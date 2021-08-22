using System;

public class COMWindowAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
{
	protected float mStart;
	protected float mTarget;
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	public void setStart(float alpha) { mStart = alpha; }
	public void setTarget(float alpha) { mTarget = alpha; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as myUIObject;
		// 此处不使用递归透明度变化是为了尽量不影响其他窗口
		obj.setAlpha(lerpSimple(mStart, mTarget, value), false);
	}
}