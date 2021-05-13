using System;
using UnityEngine;

public class CurveCubicInOut : MyCurve
{
	public override float Evaluate(float time)
	{
		if (time * 0.5f < 1.0f)
		{
			return 0.5f * time * time * time;
		}
		time -= 2;
		return 0.5f * (time * time * time + 2.0f);
	}
}