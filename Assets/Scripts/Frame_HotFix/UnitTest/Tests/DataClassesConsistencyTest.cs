#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static MathUtility;

// MathUtility 一致性测试
public static class DataClassesConsistencyTest
{
	public static void Run()
	{
		testMinMax();
		testClampRange();
		testLerpRange();
	}

	private static void testMinMax()
	{
		AssertEqual(10, getMax(10, 5));
		AssertEqual(5, getMin(10, 5));
	}

	private static void testClampRange()
	{
		for (int i = -5; i <= 15; i++)
		{
			int c = clamp(i, 0, 10);
			Assert(c >= 0 && c <= 10);
		}
	}

	private static void testLerpRange()
	{
		AssertEqual(0f, lerp(0f, 10f, 0f));
		AssertEqual(10f, lerp(0f, 10f, 1f));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
