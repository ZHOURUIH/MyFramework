using System;
using UnityEngine;

public class CurveBackInOut : MyCurve
{
	public override float Evaluate(float time)
	{
		time *= 2.0f;
		float overshootOrAmplitude = mOvershootOrAmplitude * 1.525f;
		if (time < 1.0f)
		{
			return 0.5f * (time * time * ((overshootOrAmplitude + 1.0f) * time - overshootOrAmplitude));
		}
		time -= 2.0f;
		return 0.5f * (time * time * ((overshootOrAmplitude + 1.0f) * time + overshootOrAmplitude) + 2.0f);
	}
}