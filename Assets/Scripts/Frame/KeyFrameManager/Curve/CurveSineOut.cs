using System;
using UnityEngine;

public class CurveSineOut : MyCurve
{
	public override float Evaluate(float time)
	{
		return sin(time * HALF_PI_RADIAN);
	}
}