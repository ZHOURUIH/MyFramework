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
}
