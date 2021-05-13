using System;
using UnityEngine;

public class CurveCircleInOut : MyCurve
{
	public override float Evaluate(float time)
	{
		time *= 2.0f;
		if (time < 1.0f)
		{
			return -0.5f * (sqrt(1.0f - time * time) - 1.0f);
		}
		time -= 2.0f;
		return 0.5f * (sqrt(1.0f - time * time) + 1.0f);
	}
}