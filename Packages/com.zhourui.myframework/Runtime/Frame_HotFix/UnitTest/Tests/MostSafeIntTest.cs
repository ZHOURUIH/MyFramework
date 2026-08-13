using static TestAssert;

// MostSafeInt 双倍安全整型测试
// MostSafeInt 内部双份 SafeInt 存储（数据 + 校验），需在主线程运行
public static class MostSafeIntTest
{
	public static void Run()
	{
		testSetAndGet();
		testDefaultValue();
		testNegativeValue();
		testZero();
		testLargeValue();
		testEquals();
		testOverwrite();
	

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

	// ─── set / get 基本读写 ──────────────────────────────────────────────

	private static void testSetAndGet()
	{
		var ms = new MostSafeInt(42);
		assertEqual(42, ms.get(), "set/get: 期望 42");
	}

	private static void testDefaultValue()
	{
		var ms = new MostSafeInt(0);
		assertEqual(0, ms.get(), "默认值: get 应返回 0");
	}

	private static void testNegativeValue()
	{
		var ms = new MostSafeInt(-256);
		assertEqual(-256, ms.get(), "负值: 期望 -256");
	}

	private static void testZero()
	{
		var ms = new MostSafeInt(500);
		ms.set(0);
		assertEqual(0, ms.get(), "set(0): get 应返回 0");
	}

	private static void testLargeValue()
	{
		int large = 50000000;
		var ms = new MostSafeInt(large);
		assertEqual(large, ms.get(), $"大数值: 期望 {large}");
	}

	// ─── 多次 set 覆写 ───────────────────────────────────────────────────

	private static void testOverwrite()
	{
		var ms = new MostSafeInt(10);
		ms.set(20);
		ms.set(30);
		assertEqual(30, ms.get(), "连续 set: 期望 30");
	}

	// ─── Equals ──────────────────────────────────────────────────────────

	private static void testEquals()
	{
		// MostSafeInt.Equals 比较内部 SafeInt 字段（含随机密钥），
		// 不同实例即使值相同密文也不同，通过 get() 比较值
		var a = new MostSafeInt(100);
		var b = new MostSafeInt(100);
		var c = new MostSafeInt(200);
		assertEqual(100, a.get(), "Equals via get a: 期望 100");
		assertEqual(100, b.get(), "Equals via get b: 期望 100");
		assertEqual(200, c.get(), "Equals via get c: 期望 200");
		// Equals 的确定性语义(不依赖随机):
		assertTrue(a.Equals(a), "反身性 a.Equals(a) 必为 true");
		MostSafeInt copy = a;
		assertTrue(copy.Equals(a), "结构体复制 copy=a 后 copy.Equals(a) 必为 true");
		assertTrue(a.Equals(copy), "结构体复制后 a.Equals(copy) 必为 true");
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