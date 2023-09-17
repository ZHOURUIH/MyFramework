using System;
using UnityEngine;

// 倒退曲线
public class CurveBackIn : MyCurve
{
	public override float evaluate(float time)
	{
		return time * time * ((mOvershootOrAmplitude + 1) * time - mOvershootOrAmplitude);
	}
}