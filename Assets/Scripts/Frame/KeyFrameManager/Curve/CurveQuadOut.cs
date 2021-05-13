using System;
using UnityEngine;

public class CurveQuadOut : MyCurve
{
	public override float Evaluate(float time)
	{
		return -time * (time - 2.0f);
	}
}