using System;
using UnityEngine;

// 振荡曲线
public class CurveElasticInOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (isFloatZero(time))
		{
			return 0.0f;
		}
		if (isFloatEqual(time, 1.0f))
		{
			return 1.0f;
		}
		float period = 0.45f;
		float s = period / TWO_PI_RADIAN * asin(1.0f / mOvershootOrAmplitude);
		if (time < 1.0f)
		{
			time -= 1.0f;
			return -0.5f * (mOvershootOrAmplitude * pow(2.0f, 10.0f * time) * sin((time - s) * TWO_PI_RADIAN / period));
		}
		else
		{
			time -= 1.0f;
			return mOvershootOrAmplitude * pow(2.0f, -10.0f * time) * sin((time - s) * TWO_PI_RADIAN / period) * 0.5f + 1.0f;
		}
	}
}