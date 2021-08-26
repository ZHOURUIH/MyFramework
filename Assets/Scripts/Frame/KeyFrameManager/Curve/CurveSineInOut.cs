using System;
using UnityEngine;

// 正弦曲线
public class CurveSineInOut : MyCurve
{
	public override float evaluate(float time)
	{
		return -0.5f * (cos(PI_RADIAN * time) - 1.0f);
	}
}