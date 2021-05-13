using System;
using UnityEngine;

public class CurveElasticIn : MyCurve
{
	public override float Evaluate(float time)
	{
		if (isFloatZero(time))
		{
			return 0.0f;
		}
		if (isFloatEqual(time, 1.0f))
		{
			return 1.0f;
		}
		float period = 0.3f;
		float s0 = period / TWO_PI_RADIAN * asin(1 / mOvershootOrAmplitude);
		time -= 1.0f;
		return -(mOvershootOrAmplitude * pow(2.0f, 10.0f * time) * sin((time - s0) * TWO_PI_RADIAN / period));
	}
}