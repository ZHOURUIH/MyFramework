#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static MathUtility;

// MathUtility 缓动/插值函数测试
public static class UndoWaitingAsyncTest
{
	public static void Run()
	{
		testLerpUnclamped();
		testInverseLerp();
	}

	private static void testLerpUnclamped()
	{
		AssertEqual(5f, lerp(0f, 10f, 0.5f));
	}

	private static void testInverseLerp()
	{
		AssertEqual(0f, inverseLerp(0f, 10f, 0f));
		AssertEqual(0.5f, inverseLerp(0f, 10f, 5f));
		AssertEqual(1f, inverseLerp(0f, 10f, 10f));
	}

	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
