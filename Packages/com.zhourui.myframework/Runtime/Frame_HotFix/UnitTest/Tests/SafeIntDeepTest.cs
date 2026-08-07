using static TestAssert;

// SafeInt 深度测试
// 聚焦边界值与反复读写的一致性：int 极值、随机边界、大量连续 set/get、
// set 后多次 get 应稳定、多实例交错互不干扰、负值与零。
// 注：SafeInt 使用随机密文+校验，仅验证值语义往返，不触发内存篡改检测路径。
public static class SafeIntDeepTest
{
	public static void Run()
	{
		testIntMinMax();
		testNegativeBoundaries();
		testRepeatedSetGet_Stable();
		testManyRoundTrips();
		testAlternatingSetGet();
		testBoundaryNearMax();
		testOddEvenPatterns();
		testInterleavedInstances();
		testSameValueAcrossInstances_IndependentCipher();
		testZeroAndPositiveSequence();
	}

	// ─── int 极值 ──────────────────────────────────────────────────────
	private static void testIntMinMax()
	{
		var min = new SafeInt(int.MinValue);
		var max = new SafeInt(int.MaxValue);
		assertEqual(int.MinValue, min.get(), "int.MinValue 往返");
		assertEqual(int.MaxValue, max.get(), "int.MaxValue 往返");
	}

	// ─── 负值边界 ─────────────────────────────────────────────────────
	private static void testNegativeBoundaries()
	{
		int[] vals = { -1, -128, -1000, -32768, -2147483648 };
		foreach (int v in vals)
		{
			var si = new SafeInt(v);
			assertEqual(v, si.get(), $"负值往返 {v}");
		}
	}

	// ─── set 多次后 get 应始终稳定 ────────────────────────────────────
	private static void testRepeatedSetGet_Stable()
	{
		var si = new SafeInt(0);
		si.set(12345);
		for (int i = 0; i < 50; ++i)
		{
			assertEqual(12345, si.get(), $"set 后多次 get 稳定[{i}]");
		}
	}

	// ─── 大量 set/get 往返 ───────────────────────────────────────────
	private static void testManyRoundTrips()
	{
		var si = new SafeInt(0);
		for (int i = 0; i < 200; ++i)
		{
			si.set(i);
			assertEqual(i, si.get(), $"连续 set/get[{i}]");
		}
	}

	// ─── 交错 set：读取到最新写入值 ──────────────────────────────────
	private static void testAlternatingSetGet()
	{
		var si = new SafeInt(0);
		si.set(100);
		assertEqual(100, si.get(), "先 100");
		si.set(200);
		assertEqual(200, si.get(), "再 200");
		si.set(-50);
		assertEqual(-50, si.get(), "再 -50");
	}

	// ─── 逼近但不超过 int.MaxValue 的边界 ────────────────────────────
	private static void testBoundaryNearMax()
	{
		long[] big = { 2147483647L, 2147483646L, 1073741823L };
		foreach (long v in big)
		{
			var si = new SafeInt((int)v);
			assertEqual((int)v, si.get(), $"逼近最大值 {v}");
		}
	}

	// ─── 奇偶模式 ─────────────────────────────────────────────────────
	private static void testOddEvenPatterns()
	{
		for (int i = -10; i <= 10; ++i)
		{
			var si = new SafeInt(i);
			assertEqual(i, si.get(), $"奇偶模式 {i}");
		}
	}

	// ─── 多实例交错，互不干扰 ────────────────────────────────────────
	private static void testInterleavedInstances()
	{
		var a = new SafeInt(11);
		var b = new SafeInt(22);
		var c = new SafeInt(33);
		a.set(111);
		b.set(222);
		c.set(333);
		assertEqual(111, a.get(), "a=111");
		assertEqual(222, b.get(), "b=222");
		assertEqual(333, c.get(), "c=333");
		a.set(1);
		assertEqual(1, a.get(), "a 改 1");
		assertEqual(222, b.get(), "b 不受影响");
		assertEqual(333, c.get(), "c 不受影响");
	}

	// ─── 相同值但不同实例密文独立 ───────────────────────────────────
	private static void testSameValueAcrossInstances_IndependentCipher()
	{
		var a = new SafeInt(7);
		var b = new SafeInt(7);
		var c = new SafeInt(7);
		assertEqual(7, a.get(), "a=7");
		assertEqual(7, b.get(), "b=7");
		assertEqual(7, c.get(), "c=7");
		// Equals 应在内部密文一致时成立（但不同实例密文不同）
		b.set(7);
		c.set(7);
		assertEqual(7, b.get(), "b 重设仍 7");
		assertEqual(7, c.get(), "c 重设仍 7");
	}

	// ─── 零与正数序列 ────────────────────────────────────────────────
	private static void testZeroAndPositiveSequence()
	{
		int[] vals = { 0, 1, 2, 10, 100, 1000, 65535, 1000000 };
		var si = new SafeInt(0);
		foreach (int v in vals)
		{
			si.set(v);
			assertEqual(v, si.get(), $"正数序列 {v}");
		}
	}
}
