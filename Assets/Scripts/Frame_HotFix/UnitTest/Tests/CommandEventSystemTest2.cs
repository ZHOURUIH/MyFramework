#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static MathUtility;

// Command/Event 基础工具测试
public static class CommandEventSystemTest2
{
	public static void Run()
	{
		testSign();
		testClamp();
	}

	private static void testSign()
	{
		AssertEqual(-1, sign(-100));
		AssertEqual(0, sign(0));
		AssertEqual(1, sign(100));
	}

	private static void testClamp()
	{
		AssertEqual(5, clamp(5, 0, 10));
		AssertEqual(0, clamp(-1, 0, 10));
		AssertEqual(10, clamp(15, 0, 10));
	}
	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
