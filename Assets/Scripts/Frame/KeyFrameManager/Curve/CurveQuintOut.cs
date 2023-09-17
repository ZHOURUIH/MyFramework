using System;
using UnityEngine;

// 五次方曲线
public class CurveQuintOut : MyCurve
{
	public override float evaluate(float time)
	{
		time -= 1.0f;
		return time * time * time * time * time + 1.0f;
	}
}