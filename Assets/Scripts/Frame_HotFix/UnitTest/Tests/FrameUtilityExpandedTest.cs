#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static StringUtility;
using static MathUtility;

// FrameUtility 扩展测试
public static class FrameUtilityExpandedTest
{
	public static void Run()
	{
		testBoolToString();
		testSign();
		testClampFloat();
	}

	private static void testBoolToString()
	{
		AssertEqual("true", boolToString(true));
		AssertEqual("false", boolToString(false));
	}

	private static void testSign()
	{
		AssertEqual(-1, sign(-5));
		AssertEqual(0, sign(0));
		AssertEqual(1, sign(10));
	}

	private static void testClampFloat()
	{
		AssertEqual(5f, clamp(5f, 0f, 10f));
		AssertEqual(0f, clamp(-1f, 0f, 10f));
		AssertEqual(10f, clamp(15f, 0f, 10f));
	}

	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
