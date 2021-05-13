using System;
using UnityEngine;

public class CurveBounceIn : MyCurve
{
	public override float Evaluate(float time)
	{
		return bounceEaseIn(time);
	}
}