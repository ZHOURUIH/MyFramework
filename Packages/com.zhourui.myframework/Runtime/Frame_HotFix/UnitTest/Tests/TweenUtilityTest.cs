using UnityEngine;
using static TweenUtility;
using static TestAssert;

// TweenUtility 补间工具函数测试
public static class TweenUtilityTest
{
	public static void Run()
	{
		testEvaluate();
		testEvaluateCurve();
	}

	private static void testEvaluate()
	{
		Vector3 start = new(0, 0, 0);
		Vector3 target = new(10, 10, 10);

		// value=0 应返回 start
		Vector3 r0 = Evaluate(start, target, 0);
		assertEqual(0f, r0.x, "Evaluate t=0 x");
		assertEqual(0f, r0.y, "Evaluate t=0 y");
		assertEqual(0f, r0.z, "Evaluate t=0 z");

		// value=1 应返回 target
		Vector3 r1 = Evaluate(start, target, 1);
		assertEqual(10f, r1.x, "Evaluate t=1 x");
		assertEqual(10f, r1.y, "Evaluate t=1 y");
		assertEqual(10f, r1.z, "Evaluate t=1 z");

		// value=0.5 应返回中点
		Vector3 r05 = Evaluate(start, target, 0.5f);
		assertEqual(5f, r05.x, "Evaluate t=0.5 x");
		assertEqual(5f, r05.y, "Evaluate t=0.5 y");
		assertEqual(5f, r05.z, "Evaluate t=0.5 z");

		// value<0 外插 (LerpUnclamped)
		Vector3 rNeg = Evaluate(start, target, -0.5f);
		assertEqual(-5f, rNeg.x, "Evaluate t=-0.5 x");

		// value>1 外插
		Vector3 rOver = Evaluate(start, target, 2f);
		assertEqual(20f, rOver.x, "Evaluate t=2 x");
	}

	private static void testEvaluateCurve()
	{
		// 构造简单线性 MyCurve: evaluate 返回 percent 本身
		var curve = new LinearTestCurve();
		assertEqual(0f, EvaluateCurve(curve, 0), "EvaluateCurve t=0");
		assertEqual(0.5f, EvaluateCurve(curve, 0.5f), "EvaluateCurve t=0.5");
		assertEqual(1f, EvaluateCurve(curve, 1), "EvaluateCurve t=1");
	}
}

// 测试用简单线性曲线: evaluate(percent) = percent
class LinearTestCurve : MyCurve
{
	public override float evaluate(float time) { return time; }
	public override void resetProperty() { base.resetProperty(); }
}
