using System;
using UnityEngine;

public class CurveCubicOut : MyCurve
{
	public override float Evaluate(float time)
	{
		time -= 1.0f;
		return time * time * time + 1.0f;
	}
}