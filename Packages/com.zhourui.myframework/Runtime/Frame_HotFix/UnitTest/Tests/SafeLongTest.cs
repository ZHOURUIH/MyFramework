using static TestAssert;

// SafeLong 安全长整型测试
// SafeLong 内部使用多份密文 + 密钥校验，需在主线程运行
public static class SafeLongTest
{
	public static void Run()
	{
		testSetAndGet();
		testDefaultValue();
		testNegativeValue();
		testZero();
		testLargeValue();
		testOverwrite();
		testMultipleInstances();
		testEquals();
	

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

	// ─── set / get 基本读写 ──────────────────────────────────────────────

	private static void testSetAndGet()
	{
		var sl = new SafeLong(42L);
		assertEqual(42L, sl.get(), "set/get: 期望 42");
	}

	private static void testDefaultValue()
	{
		var sl = new SafeLong(0L);
		assertEqual(0L, sl.get(), "默认值: get 应返回 0");
	}

	private static void testNegativeValue()
	{
		var sl = new SafeLong(-9876543210L);
		assertEqual(-9876543210L, sl.get(), "负值: 期望 -9876543210");
	}

	private static void testZero()
	{
		var sl = new SafeLong(999999L);
		sl.set(0L);
		assertEqual(0L, sl.get(), "set(0): get 应返回 0");
	}

	private static void testLargeValue()
	{
		// SafeLong 内部用 long 存储，验证较大值
		long large = 922337203685477L;  // 避免超过 long 范围的边界值
		var sl = new SafeLong(large);
		assertEqual(large, sl.get(), $"大数值: 期望 {large}");
	}

	private static void testOverwrite()
	{
		var sl = new SafeLong(100L);
		sl.set(200L);
		sl.set(300L);
		assertEqual(300L, sl.get(), "连续 set: 期望 300");
	}

	// ─── 多实例独立 ───────────────────────────────────────────────────────

	private static void testMultipleInstances()
	{
		var a = new SafeLong(1000L);
		var b = new SafeLong(2000L);
		var c = new SafeLong(3000L);
		assertEqual(1000L, a.get(), "多实例 a: 期望 1000");
		assertEqual(2000L, b.get(), "多实例 b: 期望 2000");
		assertEqual(3000L, c.get(), "多实例 c: 期望 3000");

		a.set(9999L);
		b.set(8888L);
		assertEqual(9999L, a.get(), "多实例修改后 a: 期望 9999");
		assertEqual(8888L, b.get(), "多实例修改后 b: 期望 8888");
		assertEqual(3000L, c.get(), "多实例 c 未变: 期望 3000");
	}

	// ─── Equals ──────────────────────────────────────────────────────────

	private static void testEquals()
	{
		// SafeLong.Equals 比较密文字段，不同实例即使值相同密文也不同
		var a = new SafeLong(12345L);
		var b = new SafeLong(12345L);
		var c = new SafeLong(54321L);
		assertEqual(12345L, a.get(), "Equals via get a: 期望 12345");
		assertEqual(12345L, b.get(), "Equals via get b: 期望 12345");
		assertEqual(54321L, c.get(), "Equals via get c: 期望 54321");
		// Equals 的确定性语义(不依赖随机):
		// 1) 反身性: 同一实例与自身 Equals 必为 true
		assertTrue(a.Equals(a), "反身性 a.Equals(a) 必为 true");
		// 2) 结构体逐字段复制后 Equals 必为 true(含全部私有加密字段原样拷贝)
		SafeLong copy = a;
		assertTrue(copy.Equals(a), "结构体复制 copy=a 后 copy.Equals(a) 必为 true");
		assertTrue(a.Equals(copy), "结构体复制后 a.Equals(copy) 必为 true");
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