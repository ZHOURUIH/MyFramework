using System;
using UnityEngine;

public class CurveQuadIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return time * time;
	}
}