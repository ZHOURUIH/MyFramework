using System;
using UnityEngine;

// 指数曲线
public class CurveExpoIn : MyCurve
{
	public override float evaluate(float time)
	{
		if (isFloatZero(time))
		{
			return 0.0f;
		}
		return pow(2.0f, 10.0f * (time - 1.0f));
	}
}