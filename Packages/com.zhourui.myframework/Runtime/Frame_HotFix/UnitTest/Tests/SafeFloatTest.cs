using static TestAssert;

// SafeFloat 安全浮点测试
// SafeFloat 内部使用多份密文 + 密钥校验，需在主线程运行
public static class SafeFloatTest
{
	// 浮点精度容差
	private const float EPSILON = 1e-4f;

	public static void Run()
	{
		testSetAndGet();
		testDefaultValue();
		testNegativeValue();
		testZero();
		testLargeValue();
		testSmallFraction();
		testOverwrite();
		testMultipleInstances();
		testEquals();
	

		testTypicalValues();
		testNegativeValues();
		testZeroAndOne();
		testRepeatedSetGet_Stable();
		testManyRoundTrips();
		testBoundaryThousandths();
		testSingleAndHalf();
		testInterleavedInstances();
		testSmallPositiveSteps();
		testLargeValues();
	}

	// ─── 工具 ────────────────────────────────────────────────────────────

	private static bool isFloatEqual(float a, float b, float precision = 0.0001f)
	{
		return abs(a - b) <= precision;
	}

	private static float abs(float v) => v < 0 ? -v : v;

	// ─── set / get 基本读写 ──────────────────────────────────────────────

	private static void testSetAndGet()
	{
		var sf = new SafeFloat(3.14f);
		float val = sf.get();
		assert(isFloatEqual(val, 3.14f), $"set/get: 期望 3.14，实际 {val}");
	}

	private static void testDefaultValue()
	{
		var sf = new SafeFloat(0f);
		float val = sf.get();
		assert(isFloatEqual(val, 0f), "默认值: get 应返回 0");
	}

	private static void testNegativeValue()
	{
		var sf = new SafeFloat(-99.5f);
		float val = sf.get();
		assert(isFloatEqual(val, -99.5f), $"负值: 期望 -99.5，实际 {val}");
	}

	private static void testZero()
	{
		var sf = new SafeFloat(1000f);
		sf.set(0f);
		float val = sf.get();
		assert(isFloatEqual(val, 0f), "set(0): get 应返回 0");
	}

	private static void testLargeValue()
	{
		// SafeFloat 内部用 (int)(value*10000) 做明文校验，abs(value) 需 < 214748
		float large = 10000f;
		var sf = new SafeFloat(large);
		float val = sf.get();
		assert(isFloatEqual(val, large, large * 1e-4f + 0.001f), $"大数值: 期望 {large}，实际 {val}");
	}

	private static void testSmallFraction()
	{
		// 验证小数值精度
		float small = 0.00123f;
		var sf = new SafeFloat(small);
		float val = sf.get();
		assert(isFloatEqual(val, small, 0.001f), $"小数值: 期望 {small}，实际 {val}");
	}

	private static void testOverwrite()
	{
		var sf = new SafeFloat(1.0f);
		sf.set(2.0f);
		sf.set(3.0f);
		float val = sf.get();
		assert(isFloatEqual(val, 3.0f), $"连续 set: 期望 3.0，实际 {val}");
	}

	// ─── 多实例独立 ───────────────────────────────────────────────────────

	private static void testMultipleInstances()
	{
		var a = new SafeFloat(1.5f);
		var b = new SafeFloat(2.5f);
		assert(isFloatEqual(a.get(), 1.5f), "多实例 a: 期望 1.5");
		assert(isFloatEqual(b.get(), 2.5f), "多实例 b: 期望 2.5");

		a.set(10.5f);
		b.set(20.5f);
		assert(isFloatEqual(a.get(), 10.5f), "多实例修改后 a: 期望 10.5");
		assert(isFloatEqual(b.get(), 20.5f), "多实例修改后 b: 期望 20.5");
	}

	// ─── Equals ──────────────────────────────────────────────────────────

	private static void testEquals()
	{
		// SafeFloat.Equals 比较密文字段，不同实例即使值相同密文也不同
		// 通过 get() 比较浮点值
		var a = new SafeFloat(3.14f);
		var b = new SafeFloat(3.14f);
		var c = new SafeFloat(5.0f);
		assert(isFloatEqual(a.get(), b.get()), "Equals via get: 相同值应相等");
		assert(!isFloatEqual(a.get(), c.get()), "Equals via get: 不同值不应相等");
		// Equals 的确定性语义(不依赖随机):
		assertTrue(a.Equals(a), "反身性 a.Equals(a) 必为 true");
		SafeFloat copy = a;
		assertTrue(copy.Equals(a), "结构体复制 copy=a 后 copy.Equals(a) 必为 true");
		assertTrue(a.Equals(copy), "结构体复制后 a.Equals(copy) 必为 true");
	}


	

	// ─── 典型值 ──────────────────────────────────────────────────────
	private static void testTypicalValues()
	{
		float[] vals = { 1.5f, 2.25f, 3.75f, 100.5f, 0.125f, 99.99f };
		foreach (float v in vals)
		{
			var sf = new SafeFloat(v);
			assertEqual(v, sf.get(), 0.001f, $"典型值往返 {v}");
		}
	}

	// ─── 负值 ───────────────────────────────────────────────────────
	private static void testNegativeValues()
	{
		float[] vals = { -1.5f, -0.25f, -100.5f, -3.75f };
		foreach (float v in vals)
		{
			var sf = new SafeFloat(v);
			assertEqual(v, sf.get(), 0.001f, $"负值往返 {v}");
		}
	}

	// ─── 零与一 ─────────────────────────────────────────────────────
	private static void testZeroAndOne()
	{
		var zero = new SafeFloat(0f);
		assertEqual(0f, zero.get(), 0.001f, "零往返");
		var one = new SafeFloat(1f);
		assertEqual(1f, one.get(), 0.001f, "一往返");
	}

	// ─── set 多次后 get 稳定 ────────────────────────────────────────
	private static void testRepeatedSetGet_Stable()
	{
		var sf = new SafeFloat(0f);
		sf.set(12.5f);
		for (int i = 0; i < 40; ++i)
		{
			assertEqual(12.5f, sf.get(), 0.001f, $"set 后多次 get 稳定[{i}]");
		}
	}

	// ─── 大量 set/get 往返 ──────────────────────────────────────────
	private static void testManyRoundTrips()
	{
		var sf = new SafeFloat(0f);
		for (int i = 0; i < 100; ++i)
		{
			float v = i * 0.5f;
			sf.set(v);
			assertEqual(v, sf.get(), 0.001f, $"连续 set/get[{i}] = {v}");
		}
	}

	// ─── 精确千分位值（无量化误差） ───────────────────────────────
	private static void testBoundaryThousandths()
	{
		float[] vals = { 0.001f, 0.999f, 1.001f, 10.5f, 255.255f };
		foreach (float v in vals)
		{
			var sf = new SafeFloat(v);
			assertEqual(v, sf.get(), 0.001f, $"千分位往返 {v}");
		}
	}

	// ─── 整数(0.5 等差)、二分之一 ─────────────────────────────────
	private static void testSingleAndHalf()
	{
		var sf = new SafeFloat(0.5f);
		assertEqual(0.5f, sf.get(), 0.001f, "0.5 往返");
		sf.set(2.5f);
		assertEqual(2.5f, sf.get(), 0.001f, "2.5 往返");
		sf.set(0.25f);
		assertEqual(0.25f, sf.get(), 0.001f, "0.25 往返");
	}

	// ─── 多实例交错，互不干扰 ──────────────────────────────────────
	private static void testInterleavedInstances()
	{
		var a = new SafeFloat(1.5f);
		var b = new SafeFloat(2.5f);
		var c = new SafeFloat(3.5f);
		a.set(11.5f);
		b.set(22.5f);
		c.set(33.5f);
		assertEqual(11.5f, a.get(), 0.001f, "a=11.5");
		assertEqual(22.5f, b.get(), 0.001f, "b=22.5");
		assertEqual(33.5f, c.get(), 0.001f, "c=33.5");
		a.set(1.5f);
		assertEqual(1.5f, a.get(), 0.001f, "a 改 1.5");
		assertEqual(22.5f, b.get(), 0.001f, "b 不受影响");
	}

	// ─── 小正增量序列 ──────────────────────────────────────────────
	private static void testSmallPositiveSteps()
	{
		var sf = new SafeFloat(0f);
		for (int i = 1; i <= 20; ++i)
		{
			float v = i * 0.125f;
			sf.set(v);
			assertEqual(v, sf.get(), 0.001f, $"小步进 {v}");
		}
	}

	// ─── 大值 ──────────────────────────────────────────────────────
	private static void testLargeValues()
	{
		// 内部 value*1000 量化, 大值(>~2^23=8388608 的精确整数区间外)的 float32 存储误差
		// 约 = v * 5.96e-8(ULP), 对 123456 读回误差可达 ~0.0078 > 0.001f。
		// 故大值断言用 0.01f 容差(小中值精确用 0.001f 是安全的)。
		float[] vals = { 10000.5f, 123456.0f, 99999.99f };
		foreach (float v in vals)
		{
			var sf = new SafeFloat(v);
			assertEqual(v, sf.get(), 0.01f, $"大值往返 {v}");
		}
	}
}