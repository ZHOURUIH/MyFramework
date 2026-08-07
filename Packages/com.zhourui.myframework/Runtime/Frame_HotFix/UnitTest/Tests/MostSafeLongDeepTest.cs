using static TestAssert;

// MostSafeLong 深度测试
// MostSafeLong 由两个 SafeLong 双重存储，支持完整 64 位。
// 聚焦：long 极值、int 范围外大值、负值、反复读写、多实例交错、宽范围序列。
public static class MostSafeLongDeepTest
{
	public static void Run()
	{
		testLongMinMax();
		testBeyondIntRange();
		testNegativeValues();
		testRepeatedSetGet_Stable();
		testManyRoundTrips();
		testInterleavedInstances();
		testBoundaryValues();
		testDualCopyConsistencyPath();
		testZeroSequence();
		testWideRangeSteps();
	}

	// ─── long 极值 ────────────────────────────────────────────────
	private static void testLongMinMax()
	{
		var min = new MostSafeLong(long.MinValue);
		var max = new MostSafeLong(long.MaxValue);
		assertEqual(long.MinValue, min.get(), "long.MinValue 往返");
		assertEqual(long.MaxValue, max.get(), "long.MaxValue 往返");
	}

	// ─── int 范围外 ───────────────────────────────────────────────
	private static void testBeyondIntRange()
	{
		long[] vals = { 2147483648L, 4294967296L, 8589934592L, -2147483649L, -4294967296L };
		foreach (long v in vals)
		{
			var m = new MostSafeLong(v);
			assertEqual(v, m.get(), $"int 范围外往返 {v}");
		}
	}

	// ─── 负值 ─────────────────────────────────────────────────────
	private static void testNegativeValues()
	{
		long[] vals = { -1L, -1000L, -100000000L, -9876543210L };
		foreach (long v in vals)
		{
			var m = new MostSafeLong(v);
			assertEqual(v, m.get(), $"负值往返 {v}");
		}
	}

	// ─── set 多次后稳定 ───────────────────────────────────────────
	private static void testRepeatedSetGet_Stable()
	{
		var m = new MostSafeLong(0L);
		m.set(9999999999L);
		for (int i = 0; i < 40; ++i)
		{
			assertEqual(9999999999L, m.get(), $"set 后多次 get 稳定[{i}]");
		}
	}

	// ─── 大量 set/get ─────────────────────────────────────────────
	private static void testManyRoundTrips()
	{
		var m = new MostSafeLong(0L);
		for (int i = 0; i < 120; ++i)
		{
			long v = (long)i * 5000000L + i;
			m.set(v);
			assertEqual(v, m.get(), $"连续 set/get[{i}]");
		}
	}

	// ─── 多实例交错 ───────────────────────────────────────────────
	private static void testInterleavedInstances()
	{
		var a = new MostSafeLong(11L);
		var b = new MostSafeLong(22L);
		var c = new MostSafeLong(33L);
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

	// ─── 边界值 ───────────────────────────────────────────────────
	private static void testBoundaryValues()
	{
		long[] vals = { 0L, 1L, 9223372036854770L, -9223372036854770L, 1073741823L };
		foreach (long v in vals)
		{
			var m = new MostSafeLong(v);
			assertEqual(v, m.get(), $"边界往返 {v}");
		}
	}

	// ─── 双副本一致性 ─────────────────────────────────────────────
	private static void testDualCopyConsistencyPath()
	{
		var m = new MostSafeLong(123456789012L);
		// get() 内部同时读取两份 SafeLong 并比对
		assertEqual(123456789012L, m.get(), "双副本一致性 get");
		m.set(9876543210L);
		assertEqual(9876543210L, m.get(), "重设后双副本一致");
	}

	// ─── 零序列 ───────────────────────────────────────────────────
	private static void testZeroSequence()
	{
		var m = new MostSafeLong(0L);
		assertEqual(0L, m.get(), "初始 0");
		m.set(0L);
		assertEqual(0L, m.get(), "显式 set 0");
	}

	// ─── 宽范围步进 ───────────────────────────────────────────────
	private static void testWideRangeSteps()
	{
		var m = new MostSafeLong(0L);
		long[] vals = { 1L, 10L, 100L, 1000L, 100000L, 10000000L, 1000000000L };
		foreach (long v in vals)
		{
			m.set(v);
			assertEqual(v, m.get(), $"宽范围步进 {v}");
		}
	}
}
