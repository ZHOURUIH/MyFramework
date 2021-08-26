using System;
using UnityEngine;

// 平方曲线
public class CurveQuadIn : MyCurve
{
	public override float evaluate(float time)
	{
		return time * time;
	}
}