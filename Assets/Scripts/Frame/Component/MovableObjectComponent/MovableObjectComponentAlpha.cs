using System;

public class MovableObjectComponentAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
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
		var obj = mComponentOwner as MovableObject;
		obj.setAlpha(lerpSimple(mStartAlpha, mTargetAlpha, value));
	}
}
