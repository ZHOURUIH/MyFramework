using System;
using UnityEngine;

// 圆曲线
public class CurveCircleOut : MyCurve
{
	public override float evaluate(float time)
	{
		time -= 1.0f;
		return sqrt(1.0f - time * time);
	}
}