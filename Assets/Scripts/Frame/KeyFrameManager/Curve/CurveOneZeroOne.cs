using System;
using UnityEngine;

public class CurveOneZeroOne : MyCurve
{
	public override float Evaluate(float time)
	{
		if(time <= 0.5f)
		{
			return 2.0f - time * 2.0f;
		}
		return 2.0f * time - 1.0f;
	}
}