using System;
using UnityEngine;

public class CurveSineIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return -cos(time * HALF_PI_RADIAN) + 1.0f;
	}
}