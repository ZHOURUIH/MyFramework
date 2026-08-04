using UnityEngine;
using static TestAssert;

// UnityCurve 穷举测试
public static class UnityCurveTest
{
	public static void Run()
	{
		testWrapAndEvaluate();
		testGetLength();
		testNullCurve();
		testResetProperty();
		testGetAnimationCurve();
		testOutOfRange();
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

	// ─── getLength ───────────────────────────────────────────────────────
	private static void testGetLength()
	{
		var animCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		var curve = new UnityCurve(animCurve);
		float len = curve.getLength();
		assertTrue(len > 0, "getLength 应 > 0");
	}

	// ─── null curve ──────────────────────────────────────────────────────
	private static void testNullCurve()
	{
		// 构造时不传 AnimationCurve 会怎样？mCurve=null
		var curve = new UnityCurve(null);
		assertEqual(0.0f, curve.evaluate(0.5f), "null curve evaluate 返回 0");
		assertEqual(0.0f, curve.getLength(), "null curve getLength 返回 0");
	}

	// ─── resetProperty ───────────────────────────────────────────────────
	private static void testResetProperty()
	{
		var animCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		var curve = new UnityCurve(animCurve);
		curve.resetProperty();
		assertEqual(0.0f, curve.evaluate(0.5f), "reset 后 evaluate 返回 0");
		assertEqual(0.0f, curve.getLength(), "reset 后 getLength 返回 0");
	}

	// ─── getAnimationCurve ───────────────────────────────────────────────
	private static void testGetAnimationCurve()
	{
		var animCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		var curve = new UnityCurve(animCurve);
		var got = curve.getAnimationCurve();
		assertNotNull(got, "getAnimationCurve 不应返回 null");
	}

	// ─── 越界值 ──────────────────────────────────────────────────────────
	private static void testOutOfRange()
	{
		var animCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		var curve = new UnityCurve(animCurve);
		// 不 clamp 时不崩溃
		float neg = curve.evaluate(-0.5f);
		float big = curve.evaluate(2.0f);
	}
}
