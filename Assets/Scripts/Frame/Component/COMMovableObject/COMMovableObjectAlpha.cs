using System;

public class COMMovableObjectAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
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
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as MovableObject;
		obj.setAlpha(lerpSimple(mStart, mTarget, value));
	}
}
