using System;

// 物体的透明度变化组件
public class COMMovableObjectAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
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
		var obj = mComponentOwner as MovableObject;
		obj.setAlpha(lerpSimple(mStart, mTarget, value));
	}
}