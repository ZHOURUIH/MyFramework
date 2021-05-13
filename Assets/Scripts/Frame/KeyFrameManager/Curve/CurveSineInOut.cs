using System;
using UnityEngine;

public class CurveSineInOut : MyCurve
{
	public override float Evaluate(float time)
	{
		return -0.5f * (cos(PI_RADIAN * time) - 1.0f);
	}
}