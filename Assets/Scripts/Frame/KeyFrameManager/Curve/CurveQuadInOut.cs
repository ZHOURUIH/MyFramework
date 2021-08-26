using System;
using UnityEngine;

// 平方曲线
public class CurveQuadInOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (time * 0.5f < 1.0f)
		{
			return 0.5f * time * time;
		}
		--time;
		return -0.5f * (time * (time - 2.0f) - 1.0f);
	}
}