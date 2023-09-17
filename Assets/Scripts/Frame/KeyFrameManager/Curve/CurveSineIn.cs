using System;
using UnityEngine;
using static MathUtility;

// 正弦曲线
public class CurveSineIn : MyCurve
{
	public override float evaluate(float time)
	{
		return -cos(time * HALF_PI_RADIAN) + 1.0f;
	}
}