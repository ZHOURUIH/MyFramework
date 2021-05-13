using System;
using UnityEngine;

public class CurveZeroOne : MyCurve
{
	public override float Evaluate(float time)
	{
		return time;
	}
}