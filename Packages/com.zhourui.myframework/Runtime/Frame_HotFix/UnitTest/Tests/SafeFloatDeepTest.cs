using static TestAssert;

// SafeFloat 深度测试
// SafeFloat 内部以 value*1000 四舍五入后作密文存储，精度约3位小数。
// 聚焦：典型值往返、负值、零、小数边界、反复读写稳定、多实例交错、大值。
// 断言均使用 0.001f 容差以匹配内部 1000 倍量化。
public static class SafeFloatDeepTest
{
	public static void Run()
	{
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
