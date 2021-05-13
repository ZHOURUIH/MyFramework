using System;
using UnityEngine;

public class CurveQuintIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return time * time * time * time * time;
	}
}