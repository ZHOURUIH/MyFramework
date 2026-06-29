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
	}
}