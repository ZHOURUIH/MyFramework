using System;
using UnityEngine;

public class CurveOneZero : MyCurve
{
	public override float Evaluate(float time)
	{
		return 1.0f - time;
	}
}