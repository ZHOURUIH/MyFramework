#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;
using static MathUtility;

// FrameUtility + MathUtility 剩余测试
public static class FrameUtilityRemainingTest
{
	public static void Run()
	{
		testSign();
		testSwap();
		testClampInt();
	}

	private static void testSign()
	{
		AssertEqual(-1, sign(-5));
		AssertEqual(0, sign(0));
		AssertEqual(1, sign(10));
	}

	private static void testSwap()
	{
		int a = 1;
		int b = 2;
		swap(ref a, ref b);
		AssertEqual(2, a);
		AssertEqual(1, b);
	}

	private static void testClampInt()
	{
		AssertEqual(5, clamp(5, 0, 10));
		AssertEqual(0, clamp(-5, 0, 10));
		AssertEqual(10, clamp(15, 0, 10));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
