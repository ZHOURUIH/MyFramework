using System;
using UnityEngine;

public class CurveCubicIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return time * time * time;
	}
}