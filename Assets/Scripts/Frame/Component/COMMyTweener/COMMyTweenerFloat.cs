using System;
using static MathUtility;

// 用于渐变一个浮点数的组件
public class COMMyTweenerFloat : ComponentKeyFrameNormal
{
	protected float mCurFloatValue;	// 当前值
	protected float mStart;			// 起始值
	protected float mTarget;		// 目标值
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