using System;
using UnityEngine;

public class CurveBackOut : MyCurve
{
	public override float Evaluate(float time)
	{
		time -= 1.0f;
		return time * time * ((mOvershootOrAmplitude + 1.0f) * time + mOvershootOrAmplitude) + 1.0f;
	}
}