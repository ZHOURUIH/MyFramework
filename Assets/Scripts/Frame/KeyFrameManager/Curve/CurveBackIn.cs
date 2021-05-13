using System;
using UnityEngine;

public class CurveBackIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return time * time * ((mOvershootOrAmplitude + 1) * time - mOvershootOrAmplitude);
	}
}