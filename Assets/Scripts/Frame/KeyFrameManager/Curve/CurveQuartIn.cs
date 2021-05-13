using System;
using UnityEngine;

public class CurveQuartIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return time * time * time * time;
	}
}