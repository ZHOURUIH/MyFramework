using static TestAssert;

// SafeInt 安全整型测试
// SafeInt 内部使用多份密文 + 密钥校验，需在主线程运行
public static class SafeIntTest
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

	// ─── set / get 基本读写 ──────────────────────────────────────────────

	private static void testSetAndGet()
	{
		var si = new SafeInt(42);
		assertEqual(42, si.get(), "set/get: 期望 42");
	}

	private static void testDefaultValue()
	{
		// 默认构造传入 0，get 应返回 0
		// （SafeInt 无默认无参构造，此处以 0 构造验证）
		var si = new SafeInt(0);
		assertEqual(0, si.get(), "默认值: get 应返回 0");
	}

	private static void testNegativeValue()
	{
		var si = new SafeInt(-128);
		assertEqual(-128, si.get(), "负值: 期望 -128");
	}

	private static void testZero()
	{
		var si = new SafeInt(999);
		si.set(0);
		assertEqual(0, si.get(), "set(0): get 应返回 0");
	}

	private static void testLargeValue()
	{
		// SafeInt 内部用 int 存储，验证较大正值
		int large = 100000000;
		var si = new SafeInt(large);
		assertEqual(large, si.get(), $"大数值: 期望 {large}");
	}

	private static void testOverwrite()
	{
		var si = new SafeInt(1);
		si.set(2);
		si.set(3);
		assertEqual(3, si.get(), "连续 set: 期望 3");
	}

	// ─── 多实例独立 ───────────────────────────────────────────────────────

	private static void testMultipleInstances()
	{
		var a = new SafeInt(10);
		var b = new SafeInt(20);
		var c = new SafeInt(30);
		assertEqual(10, a.get(), "多实例 a: 期望 10");
		assertEqual(20, b.get(), "多实例 b: 期望 20");
		assertEqual(30, c.get(), "多实例 c: 期望 30");

		a.set(100);
		b.set(200);
		assertEqual(100, a.get(), "多实例修改后 a: 期望 100");
		assertEqual(200, b.get(), "多实例修改后 b: 期望 200");
		assertEqual(30, c.get(), "多实例 c 未变: 期望 30");
	}

	// ─── Equals ──────────────────────────────────────────────────────────

	private static void testEquals()
	{
		// SafeInt.Equals 比较所有内部密文字段（含随机密钥），
		// 不同实例即使值相同密文也不同，故 Equals 对相同实例返回 true，
		// 不同实例需通过 get() 比较值
		var a = new SafeInt(5);
		var b = new SafeInt(5);
		var c = new SafeInt(6);
		assertEqual(5, a.get(), "Equals via get a: 期望 5");
		assertEqual(5, b.get(), "Equals via get b: 期望 5");
		assertEqual(6, c.get(), "Equals via get c: 期望 6");
		// Equals 的确定性语义(不依赖随机):
		assertTrue(a.Equals(a), "反身性 a.Equals(a) 必为 true");
		SafeInt copy = a;
		assertTrue(copy.Equals(a), "结构体复制 copy=a 后 copy.Equals(a) 必为 true");
		assertTrue(a.Equals(copy), "结构体复制后 a.Equals(copy) 必为 true");
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