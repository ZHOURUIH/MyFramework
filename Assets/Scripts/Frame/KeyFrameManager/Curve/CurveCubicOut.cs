using System;
using UnityEngine;

// 立方体曲线
public class CurveCubicOut : MyCurve
{
	public override float evaluate(float time)
	{
		time -= 1.0f;
		return time * time * time + 1.0f;
	}
}