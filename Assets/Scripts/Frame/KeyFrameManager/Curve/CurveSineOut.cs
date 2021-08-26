using System;
using UnityEngine;

// 正弦曲线
public class CurveSineOut : MyCurve
{
	public override float evaluate(float time)
	{
		return sin(time * HALF_PI_RADIAN);
	}
}