using System;
using UnityEngine;

public class CurveBounceInOut : MyCurve
{
	public override float Evaluate(float time)
	{
		return bounceEaseInOut(time);
	}
}