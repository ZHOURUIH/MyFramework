using static TestAssert;

// SafeLong 深度测试
// 聚焦 long 位宽边界：long.Min/Max、超 int 范围的 64 位值、
// 负值、反复读写稳定、多实例交错、逐次累加往返。
public static class SafeLongDeepTest
{
	public static void Run()
	{
		testLongMinMax();
		testBeyondIntRange();
		testNegativeValues();
		testRepeatedSetGet_Stable();
		testManyRoundTrips();
		testLargePositiveValues();
		testInterleavedInstances();
		testZeroSequence();
		testWideRangeValues();
		testSequentialAccumulation();
	}

	// ─── long 极值 ──────────────────────────────────────────────────
	private static void testLongMinMax()
	{
		var min = new SafeLong(long.MinValue);
		var max = new SafeLong(long.MaxValue);
		assertEqual(long.MinValue, min.get(), "long.MinValue 往返");
		assertEqual(long.MaxValue, max.get(), "long.MaxValue 往返");
	}

	// ─── 超出 int 范围的值 ─────────────────────────────────────────
	private static void testBeyondIntRange()
	{
		long[] vals = { 2147483648L, 4294967296L, 8589934592L, -2147483649L, -4294967296L };
		foreach (long v in vals)
		{
			var sl = new SafeLong(v);
			assertEqual(v, sl.get(), $"int 范围外往返 {v}");
		}
	}

	// ─── 负值边界 ──────────────────────────────────────────────────
	private static void testNegativeValues()
	{
		long[] vals = { -1L, -100L, -100000L, -9876543210L };
		foreach (long v in vals)
		{
			var sl = new SafeLong(v);
			assertEqual(v, sl.get(), $"负值往返 {v}");
		}
	}

	// ─── set 多次后 get 稳定 ───────────────────────────────────────
	private static void testRepeatedSetGet_Stable()
	{
		var sl = new SafeLong(0L);
		sl.set(123456789012L);
		for (int i = 0; i < 40; ++i)
		{
			assertEqual(123456789012L, sl.get(), $"set 后多次 get 稳定[{i}]");
		}
	}

	// ─── 大量 set/get 往返 ─────────────────────────────────────────
	private static void testManyRoundTrips()
	{
		var sl = new SafeLong(0L);
		for (int i = 0; i < 150; ++i)
		{
			long v = (long)i * 1000000L + i;
			sl.set(v);
			assertEqual(v, sl.get(), $"连续 set/get[{i}]");
		}
	}

	// ─── 大正值 ────────────────────────────────────────────────────
	private static void testLargePositiveValues()
	{
		long[] vals = { 999999999999L, 1000000000000L, 9223372036854770L };
		foreach (long v in vals)
		{
			var sl = new SafeLong(v);
			assertEqual(v, sl.get(), $"大正值往返 {v}");
		}
	}

	// ─── 多实例交错 ────────────────────────────────────────────────
	private static void testInterleavedInstances()
	{
		var a = new SafeLong(11L);
		var b = new SafeLong(22L);
		var c = new SafeLong(33L);
		a.set(111L);
		b.set(222L);
		c.set(333L);
		assertEqual(111L, a.get(), "a=111");
		assertEqual(222L, b.get(), "b=222");
		assertEqual(333L, c.get(), "c=333");
		a.set(1L);
		assertEqual(1L, a.get(), "a 改 1");
		assertEqual(222L, b.get(), "b 不受影响");
	}

	// ─── 零序列 ────────────────────────────────────────────────────
	private static void testZeroSequence()
	{
		var sl = new SafeLong(0L);
		assertEqual(0L, sl.get(), "初始 0");
		sl.set(0L);
		assertEqual(0L, sl.get(), "显式 set 0");
	}

	// ─── 宽范围交错高/低值 ─────────────────────────────────────────
	private static void testWideRangeValues()
	{
		long[] vals = { 0L, 1L, long.MaxValue, -1L, long.MinValue, 123456L, -78910L };
		var sl = new SafeLong(0L);
		foreach (long v in vals)
		{
			sl.set(v);
			assertEqual(v, sl.get(), $"宽范围往返 {v}");
		}
	}

	// ─── 逐次累加（计数器语义） ────────────────────────────────────
	private static void testSequentialAccumulation()
	{
		var sl = new SafeLong(0L);
		long expected = 0;
		for (int i = 1; i <= 100; ++i)
		{
			sl.set(expected += 1000000000000L);
			assertEqual(expected, sl.get(), $"累加第 {i} 次");
		}
	}
}
