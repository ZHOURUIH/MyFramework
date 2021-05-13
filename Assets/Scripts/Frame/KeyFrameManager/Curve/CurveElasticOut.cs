using System;
using UnityEngine;

public class CurveElasticOut : MyCurve
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
		float s1 = period / TWO_PI_RADIAN * asin(1.0f / mOvershootOrAmplitude);
		return mOvershootOrAmplitude * pow(2.0f, -10.0f * time) * sin((time - s1) * TWO_PI_RADIAN / period) + 1.0f;
	}
}