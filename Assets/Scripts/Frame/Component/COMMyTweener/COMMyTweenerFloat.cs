using System;

public class COMMyTweenerFloat : ComponentKeyFrameNormal
{
	protected float mCurFloatValue;
	protected float mStart;
	protected float mTarget;
	public void setStart(float alpha) { mStart = alpha; }
	public void setTarget(float alpha) { mTarget = alpha; }
	public float getFloatValue() { return mCurFloatValue; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
		mCurFloatValue = 0.0f;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		mCurFloatValue = lerpSimple(mStart, mTarget, value);
	}
}