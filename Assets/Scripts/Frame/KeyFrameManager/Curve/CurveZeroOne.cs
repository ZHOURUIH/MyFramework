using System;
using UnityEngine;

// 0到1的直线
public class CurveZeroOne : MyCurve
{
	public override float evaluate(float time)
	{
		return time;
	}
}