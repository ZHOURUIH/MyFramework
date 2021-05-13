using System;
using UnityEngine;

public class CurveQuintOut : MyCurve
{
	public override float Evaluate(float time)
	{
		time -= 1.0f;
		return time * time * time * time * time + 1.0f;
	}
}