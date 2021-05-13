using System;
using UnityEngine;

public class CurveExpoOut : MyCurve
{
	public override float Evaluate(float time)
	{
		if (isFloatEqual(time, 1.0f))
		{
			return 1.0f;
		}
		return -pow(2.0f, -10.0f * time) + 1.0f;
	}
}