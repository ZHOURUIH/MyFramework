using static TestAssert;

// MostSafeFloat 深度测试
// MostSafeFloat 由两个 SafeFloat 双重存储，内部 1000 倍量化（3位小数精度）。
// 聚焦：典型值往返（0.001 容差）、负值、零、反复读写稳定、多实例交错、大值。
public static class MostSafeFloatDeepTest
{
	public static void Run()
	{
		testTypicalValues();
		testNegativeValues();
		testZeroAndOne();
		testRepeatedSetGet_Stable();
		testManyRoundTrips();
		testInterleavedInstances();
		testThousandths();
		testLargeValues();
		testDualCopyConsistencyPath();
		testSmallSteps();
	}

	// ─── 典型值 ──────────────────────────────────────────────────
	private static void testTypicalValues()
	{
		float[] vals = { 1.5f, 2.25f, 3.75f, 100.5f, 0.125f, 99.99f };
		foreach (float v in vals)
		{
			var m = new MostSafeFloat(v);
			assertEqual(v, m.get(), 0.001f, $"典型值往返 {v}");
		}
	}

	// ─── 负值 ────────────────────────────────────────────────────
	private static void testNegativeValues()
	{
		float[] vals = { -1.5f, -0.25f, -100.5f, -3.75f };
		foreach (float v in vals)
		{
			var m = new MostSafeFloat(v);
			assertEqual(v, m.get(), 0.001f, $"负值往返 {v}");
		}
	}

	// ─── 零与一 ──────────────────────────────────────────────────
	private static void testZeroAndOne()
	{
		var zero = new MostSafeFloat(0f);
		assertEqual(0f, zero.get(), 0.001f, "零往返");
		var one = new MostSafeFloat(1f);
		assertEqual(1f, one.get(), 0.001f, "一往返");
	}

	// ─── set 多次后稳定 ──────────────────────────────────────────
	private static void testRepeatedSetGet_Stable()
	{
		var m = new MostSafeFloat(0f);
		m.set(7.5f);
		for (int i = 0; i < 40; ++i)
		{
			assertEqual(7.5f, m.get(), 0.001f, $"set 后多次 get 稳定[{i}]");
		}
	}

	// ─── 大量 set/get ────────────────────────────────────────────
	private static void testManyRoundTrips()
	{
		var m = new MostSafeFloat(0f);
		for (int i = 0; i < 80; ++i)
		{
			float v = i * 0.5f;
			m.set(v);
			assertEqual(v, m.get(), 0.001f, $"连续 set/get[{i}] = {v}");
		}
	}

	// ─── 多实例交错 ──────────────────────────────────────────────
	private static void testInterleavedInstances()
	{
		var a = new MostSafeFloat(1.5f);
		var b = new MostSafeFloat(2.5f);
		a.set(10.5f);
		b.set(20.5f);
		assertEqual(10.5f, a.get(), 0.001f, "a=10.5");
		assertEqual(20.5f, b.get(), 0.001f, "b=20.5");
		a.set(0.5f);
		assertEqual(0.5f, a.get(), 0.001f, "a 改 0.5");
		assertEqual(20.5f, b.get(), 0.001f, "b 不受影响");
	}

	// ─── 精确千分位 ──────────────────────────────────────────────
	private static void testThousandths()
	{
		float[] vals = { 0.001f, 0.999f, 1.001f, 10.5f, 255.255f };
		foreach (float v in vals)
		{
			var m = new MostSafeFloat(v);
			assertEqual(v, m.get(), 0.001f, $"千分位往返 {v}");
		}
	}

	// ─── 大值 ────────────────────────────────────────────────────
	private static void testLargeValues()
	{
		// 内部 SafeFloat 用 value*1000 量化, 大值(>~2^23 精确整数区间外)的 float32 存储误差
		// 约 = v * 5.96e-8, 对 123456 读回误差可达 ~0.0078 > 0.001f。故大值用 0.01f 容差。
		float[] vals = { 10000.5f, 123456.0f, 99999.99f };
		foreach (float v in vals)
		{
			var m = new MostSafeFloat(v);
			assertEqual(v, m.get(), 0.01f, $"大值往返 {v}");
		}
	}

	// ─── 双副本一致性（get 同时校验两路径） ──────────────────────
	private static void testDualCopyConsistencyPath()
	{
		var m = new MostSafeFloat(3.333f);
		// get() 内部同时读取两份 SafeFloat 并比对，通过即表示双副本一致
		assertEqual(3.333f, m.get(), 0.001f, "双副本一致性 get");
		m.set(88.88f);
		assertEqual(88.88f, m.get(), 0.001f, "重设后双副本一致");
	}

	// ─── 小步进 ──────────────────────────────────────────────────
	private static void testSmallSteps()
	{
		var m = new MostSafeFloat(0f);
		for (int i = 1; i <= 30; ++i)
		{
			float v = i * 0.1f;
			m.set(v);
			assertEqual(v, m.get(), 0.001f, $"小步进 {v}");
		}
	}
}
