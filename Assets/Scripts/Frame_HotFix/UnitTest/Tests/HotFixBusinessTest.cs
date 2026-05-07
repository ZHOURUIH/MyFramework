#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static MathUtility;

// HotFix 业务函数测试
public static class HotFixBusinessTest
{
	public static void Run()
	{
		testIsEven();
		testGetGreaterPow2();
	}

	private static void testIsEven()
	{
		Assert(isEven(0));
		Assert(isEven(2));
		Assert(isEven(-4));
		Assert(!isEven(1));
	}

	private static void testGetGreaterPow2()
	{
		AssertEqual(2, getGreaterPow2(1));
		AssertEqual(4, getGreaterPow2(3));
		AssertEqual(8, getGreaterPow2(7));
		AssertEqual(16, getGreaterPow2(9));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
