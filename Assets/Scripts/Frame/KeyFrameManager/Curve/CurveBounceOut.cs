using System;
using UnityEngine;

public class CurveBounceOut : MyCurve
{
	public override float Evaluate(float time)
	{
		return bounceEaseOut(time);
	}
}