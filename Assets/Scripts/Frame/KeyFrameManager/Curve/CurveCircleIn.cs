using System;
using UnityEngine;

public class CurveCircleIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return -(sqrt(1.0f - time * time) - 1.0f);
	}
}