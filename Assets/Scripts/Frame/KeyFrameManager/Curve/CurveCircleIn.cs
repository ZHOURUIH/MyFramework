using System;
using UnityEngine;
using static MathUtility;

// 圆曲线
public class CurveCircleIn : MyCurve
{
	public override float evaluate(float time)
	{
		return -(sqrt(1.0f - time * time) - 1.0f);
	}
}