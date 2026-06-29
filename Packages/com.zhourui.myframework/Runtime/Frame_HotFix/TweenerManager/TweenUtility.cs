using UnityEngine;

public static class TweenUtility
{
	public static Vector3 Evaluate(Vector3 start, Vector3 target, float value)
	{
		return Vector3.LerpUnclamped(start, target, value);
	}
	public static float EvaluateCurve(MyCurve curve, float percent)
	{
		return curve.evaluate(percent);
	}
}