#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static BinaryUtility;
using static MathUtility;

// Thread/Command 工具测试
public static class CommandEventThreadTest
{
	public static void Run()
	{
		testIsPow2();
		testGetGreaterPow2();
		testHasBit();
	}

	private static void testIsPow2()
	{
		Assert(isPow2(1));
		Assert(isPow2(2));
		Assert(isPow2(1024));
		Assert(isPow2(0));
		Assert(!isPow2(3));
	}

	private static void testGetGreaterPow2()
	{
		AssertEqual(2, getGreaterPow2(1));
		AssertEqual(2, getGreaterPow2(2));
		AssertEqual(4, getGreaterPow2(3));
		AssertEqual(8, getGreaterPow2(5));
	}

	private static void testHasBit()
	{
		Assert(hasBit(0x08, 3));
		Assert(!hasBit(0x08, 2));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
