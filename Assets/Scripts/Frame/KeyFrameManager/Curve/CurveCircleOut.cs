using System;
using UnityEngine;

public class CurveCircleOut : MyCurve
{
	public override float Evaluate(float time)
	{
		time -= 1.0f;
		return sqrt(1.0f - time * time);
	}
}