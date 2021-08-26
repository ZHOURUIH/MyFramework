using System;
using UnityEngine;

// 弹跳曲线
public class CurveBounceIn : MyCurve
{
	public override float evaluate(float time)
	{
		return bounceEaseIn(time);
	}
}