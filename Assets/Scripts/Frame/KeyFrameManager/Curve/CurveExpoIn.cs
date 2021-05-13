using System;
using UnityEngine;

public class CurveExpoIn : MyCurve
{
	public override float Evaluate(float time)
	{
		if (isFloatZero(time))
		{
			return 0.0f;
		}
		return pow(2.0f, 10.0f * (time - 1.0f));
	}
}