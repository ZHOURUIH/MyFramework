#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using static TestAssert;

// UnityCurve 单元测试
// 包裹 AnimationCurve、evaluate 求值
public static class UnityCurveTest
{
	public static void Run()
	{
		testWrapAndEvaluate();
	}

	// ─── 包裹与求值 ──────────────────────────────────────────────────────

	private static void testWrapAndEvaluate()
	{
		var animCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		var curve = new UnityCurve(animCurve);
		assertNotNull(curve, "UnityCurve 实例不应为空");
		assertEqual(0f, curve.evaluate(0f), "evaluate(0) 应返回 0");
		assertEqual(1f, curve.evaluate(1f), "evaluate(1) 应返回 1");
	}
}
#endif
