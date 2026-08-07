using static TestAssert;

// MostSafeInt 深度测试
// MostSafeInt 由两个 SafeInt 双重存储（读写值 + 校验值）。
// 聚焦：往返一致性、双副本（校验值路径）始终与读写值相等、
// int 极值、负值、反复读写、多实例交错。
public static class MostSafeIntDeepTest
{
	public static void Run()
	{
		testIntMinMax();
		testNegativeValues();
		testRepeatedSetGet_Stable();
		testManyRoundTrips();
		testInterleavedInstances();
		testBoundaryValues();
		testAlternatingSetGet();
		testZeroAndSequence();
		testSameValueAcrossInstances();
		testWideRangeSteps();
	}

	// ─── int 极值 ──────────────────────────────────────────────────
	private static void testIntMinMax()
	{
		var min = new MostSafeInt(int.MinValue);
		var max = new MostSafeInt(int.MaxValue);
		assertEqual(int.MinValue, min.get(), "int.MinValue 往返");
		assertEqual(int.MaxValue, max.get(), "int.MaxValue 往返");
	}

	// ─── 负值 ─────────────────────────────────────────────────────
	private static void testNegativeValues()
	{
		int[] vals = { -1, -128, -10000, -2147483648 };
		foreach (int v in vals)
		{
			var msi = new MostSafeInt(v);
			assertEqual(v, msi.get(), $"负值往返 {v}");
		}
	}

	// ─── set 多次后稳定 ───────────────────────────────────────────
	private static void testRepeatedSetGet_Stable()
	{
		var msi = new MostSafeInt(0);
		msi.set(888);
		for (int i = 0; i < 50; ++i)
		{
			assertEqual(888, msi.get(), $"set 后多次 get 稳定[{i}]");
		}
	}

	// ─── 大量 set/get ─────────────────────────────────────────────
	private static void testManyRoundTrips()
	{
		var msi = new MostSafeInt(0);
		for (int i = 0; i < 150; ++i)
		{
			msi.set(i * 7);
			assertEqual(i * 7, msi.get(), $"连续 set/get[{i}]");
		}
	}

	// ─── 多实例交错 ───────────────────────────────────────────────
	private static void testInterleavedInstances()
	{
		var a = new MostSafeInt(11);
		var b = new MostSafeInt(22);
		var c = new MostSafeInt(33);
		a.set(111);
		b.set(222);
		c.set(333);
		assertEqual(111, a.get(), "a=111");
		assertEqual(222, b.get(), "b=222");
		assertEqual(333, c.get(), "c=333");
		a.set(1);
		assertEqual(1, a.get(), "a 改 1");
		assertEqual(222, b.get(), "b 不受影响");
	}

	// ─── 边界值序列 ──────────────────────────────────────────────
	private static void testBoundaryValues()
	{
		int[] vals = { 0, 1, 2147483646, -2147483647, 1073741823, -1073741824 };
		foreach (int v in vals)
		{
			var msi = new MostSafeInt(v);
			assertEqual(v, msi.get(), $"边界往返 {v}");
		}
	}

	// ─── 交错 set ─────────────────────────────────────────────────
	private static void testAlternatingSetGet()
	{
		var msi = new MostSafeInt(0);
		msi.set(100);
		assertEqual(100, msi.get(), "先 100");
		msi.set(-200);
		assertEqual(-200, msi.get(), "再 -200");
		msi.set(0);
		assertEqual(0, msi.get(), "再 0");
	}

	// ─── 零与递增序列 ─────────────────────────────────────────────
	private static void testZeroAndSequence()
	{
		int[] vals = { 0, 5, 10, 50, 500, 5000 };
		var msi = new MostSafeInt(0);
		foreach (int v in vals)
		{
			msi.set(v);
			assertEqual(v, msi.get(), $"递增序列 {v}");
		}
	}

	// ─── 相同值多实例 ─────────────────────────────────────────────
	private static void testSameValueAcrossInstances()
	{
		var a = new MostSafeInt(42);
		var b = new MostSafeInt(42);
		var c = new MostSafeInt(42);
		assertEqual(42, a.get(), "a=42");
		assertEqual(42, b.get(), "b=42");
		assertEqual(42, c.get(), "c=42");
	}

	// ─── 宽范围步进 ──────────────────────────────────────────────
	private static void testWideRangeSteps()
	{
		var msi = new MostSafeInt(0);
		foreach (int v in new int[] { 1, 10, 100, 1000, 10000, 100000, 1000000 })
		{
			msi.set(v);
			assertEqual(v, msi.get(), $"宽范围步进 {v}");
		}
	}
}
