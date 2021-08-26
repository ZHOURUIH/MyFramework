using System;
using UnityEngine;

// 通过Unity中编辑的曲线
public class UnityCurve : MyCurve
{
	protected AnimationCurve mCurve;		// unity曲线对象
	public override void resetProperty()
	{
		base.resetProperty();
		mCurve = null;
	}
	public void setCurve(AnimationCurve curve) { mCurve = curve; }
	public override float evaluate(float time)
	{
		if(mCurve == null)
		{
			return 0.0f;
		}
		return mCurve.Evaluate(time);
	}
	public override float getLength()
	{
		if (mCurve == null)
		{
			return 0.0f;
		}
		return mCurve.length;
	}
}