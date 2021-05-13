using System;
using UnityEngine;

public class CurveZeroOneZero : MyCurve
{
	public override float Evaluate(float time)
	{
		if(time <= 0.5f)
		{
			return time * 2.0f;
		}
		return 2.0f - time * 2.0f;
	}
}