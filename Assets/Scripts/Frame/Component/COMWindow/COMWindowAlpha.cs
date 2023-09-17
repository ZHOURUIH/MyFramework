using System;
using static MathUtility;

// UI的透明度变化组件
public class COMWindowAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
{
	protected float mStart;		// 起始透明度
	protected float mTarget;	// 目标透明度
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