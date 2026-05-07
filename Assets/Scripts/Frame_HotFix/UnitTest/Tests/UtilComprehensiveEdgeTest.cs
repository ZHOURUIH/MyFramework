#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static MathUtility;

// MathUtility 边值函数测试：sign/clamp/lerp/repeat/pingPong 等
public static class UtilComprehensiveEdgeTest
{
	public static void Run()
	{
		testSign();
		testClamp();
		testLerp();
		testSaturate();
	}

	private static void testSign()
	{
		AssertEqual(-1, sign(-5f));
		AssertEqual(0, sign(0f));
		AssertEqual(1, sign(5f));
	}

	private static void testClamp()
	{
		AssertEqual(5f, clamp(5f, 0f, 10f));
		AssertEqual(0f, clamp(-1f, 0f, 10f));
		AssertEqual(10f, clamp(15f, 0f, 10f));
	}

	private static void testLerp()
	{
		AssertEqual(0f, lerp(0f, 10f, 0f));
		AssertEqual(5f, lerp(0f, 10f, 0.5f));
		AssertEqual(10f, lerp(0f, 10f, 1f));
	}
	private static void testSaturate()
	{
		AssertEqual(0f, saturate(-1f));
		AssertEqual(0.5f, saturate(0.5f));
		AssertEqual(1f, saturate(2f));
	}

	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
