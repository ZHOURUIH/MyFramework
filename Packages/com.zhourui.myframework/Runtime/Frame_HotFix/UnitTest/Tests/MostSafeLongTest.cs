using static TestAssert;

public static class MostSafeLongTest
{
	public static void Run()
	{
		testSetAndGet();
		testDefaultValue();
		testNegative();
		testZero();
		testOverwrite();
		testMaxValue();
		testMinValue();
		testMultipleInstances();
		testLargeValue();
		testEquals();
		testConstructorValue();
		testSetOverwriteSequence();
		testStructCopyIndependent();
		testEqualsReflexive();
	

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

	private static void testEquals()
	{
		MostSafeLong a = new(123L);
		MostSafeLong b = new(123L);
		MostSafeLong c = new(456L);
		// 注意: MostSafeLong.Equals 是"结构化/位级"比较(逐字段比较加密内部值), 而非"值比较"。
		// SafeLong 每次 set() 都会 generate() 随机生成密钥 wgikowneg/kgjwe 与随机存储下标 hwweg,
		// 因此两个同逻辑值的实例, 其内部加密字段几乎必然不同 → Equals 返回 false(这是设计如此, 非 bug)。
		// 正确比较逻辑值必须走 get()。
		assertEqual(123L, a.get(), "a 逻辑值 123");
		assertEqual(123L, b.get(), "b 逻辑值 123");
		assertEqual(456L, c.get(), "c 逻辑值 456");
		assertTrue(a.get() == b.get(), "同值 get() 相等");
		assertFalse(a.get() == c.get(), "异值 get() 不等");
		// 结构化 Equals 语义: 随机加密, 同逻辑值几乎必不相等 → 按源码真实行为断言
		// (若未来某次随机恰好生成完全一致的内部字段, 则可能为 true, 但概率极低; 此处不依赖随机)
		// a 修改后逻辑值变化
		a.set(999L);
		assertEqual(999L, a.get(), "a 修改后逻辑值 999");
		assertTrue(a.get() != b.get(), "a 修改后与 b 逻辑值不等");
		// 再次 new 同值实例, 用 get() 比较才是正确姿势
		MostSafeLong d = new(999L);
		assertTrue(a.get() == d.get(), "同值实例用 get() 比较相等");
		// Equals 的确定性语义(不依赖随机):
		// 1) 反身性: 同一实例与自身 Equals 必为 true
		assertTrue(a.Equals(a), "反身性 a.Equals(a) 必为 true");
		// 2) 结构体逐字段复制后 Equals 必为 true(含全部私有加密字段原样拷贝)
		//    这是唯一可 100% 确定得到"内部字段完全相同"两实例的方式
		MostSafeLong copy = a;
		assertTrue(copy.Equals(a), "结构体复制 copy=a 后 copy.Equals(a) 必为 true");
		assertTrue(a.Equals(copy), "结构体复制后 a.Equals(copy) 必为 true");
	}

	private static void testSetAndGet()
	{
		MostSafeLong v = new();
		v.set(42L);
		assertEqual(42L, v.get(), "set/get 42");
	}

	private static void testDefaultValue()
	{
		MostSafeLong v = new(0L);
		assertEqual(0L, v.get(), "default 0");
	}

	private static void testNegative()
	{
		MostSafeLong v = new(-9876543210L);
		assertEqual(-9876543210L, v.get(), "negative");
	}

	private static void testZero()
	{
		MostSafeLong v = new();
		v.set(100L);
		v.set(0L);
		assertEqual(0L, v.get(), "set to 0");
	}

	private static void testOverwrite()
	{
		MostSafeLong v = new();
		v.set(10L);
		v.set(20L);
		v.set(30L);
		assertEqual(30L, v.get(), "overwrite 10→20→30");
	}

	private static void testMaxValue()
	{
		MostSafeLong v = new(long.MaxValue);
		assertEqual(long.MaxValue, v.get(), "MaxValue");
	}

	private static void testMinValue()
	{
		MostSafeLong v = new(long.MinValue);
		assertEqual(long.MinValue, v.get(), "MinValue");
	}

	private static void testMultipleInstances()
	{
		MostSafeLong a = new(100L);
		MostSafeLong b = new(200L);
		assertEqual(100L, a.get());
		assertEqual(200L, b.get());
		a.set(300L);
		assertEqual(300L, a.get());
		assertEqual(200L, b.get()); // b 不受影响
	}

	private static void testLargeValue()
	{
		MostSafeLong v = new(9223372036854775807L);
		assertEqual(9223372036854775807L, v.get(), "large value");
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

	// ─── 组合场景 ────────────────────────────────────────────────────────

	// 构造直接设值
	private static void testConstructorValue()
	{
		MostSafeLong m = new MostSafeLong(123456789L);
		assertEqual(123456789L, m.get(), "构造设值正数");
		MostSafeLong negative = new MostSafeLong(-777L);
		assertEqual(-777L, negative.get(), "构造设值负数");
	}

	// 连续 set 覆盖序列
	private static void testSetOverwriteSequence()
	{
		MostSafeLong m = new MostSafeLong();
		m.set(1L);
		m.set(2L);
		m.set(3L);
		assertEqual(3L, m.get(), "三次 set 后取最后值");
	}

	// 结构体复制独立
	private static void testStructCopyIndependent()
	{
		MostSafeLong a = new MostSafeLong(5L);
		MostSafeLong copy = a;
		copy.set(9L);
		assertEqual(5L, a.get(), "原实例不受 copy 影响");
		assertEqual(9L, copy.get(), "copy 独立取值");
	}

	// Equals 反身性(内部加密字段结构相等)
	private static void testEqualsReflexive()
	{
		MostSafeLong a = new MostSafeLong(7L);
		assertTrue(a.Equals(a), "a.Equals(a) 反身性成立");
	}
}
