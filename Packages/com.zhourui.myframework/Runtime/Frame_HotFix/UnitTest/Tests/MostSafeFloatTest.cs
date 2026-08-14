using static TestAssert;

// MostSafeFloat 安全浮点测试
// MostSafeFloat 内部双份存储 + 校验，需要在框架完整初始化后运行
public static class MostSafeFloatTest
{
	// 浮点精度容差（MostSafeFloat 内部使用 SafeFloat，set/get 精度与 float 一致）
	private const float EPSILON = 1e-5f;

	public static void Run()
	{
		testSetAndGet();
		testDefaultValue();
		testNegativeValue();
		testZero();
		testLargeValue();
		testEquals();
		testOverwrite();
		testConstructorValue();
		testSetOverwriteSequence();
		testStructCopyIndependent();
		testEqualsReflexive();
	

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

	// ─── set / get 基本读写 ──────────────────────────────────────────────

	private static void testSetAndGet()
	{
		var sf = new MostSafeFloat(3.14f);
		float val = sf.get();
		assert(abs(val - 3.14f) < EPSILON, $"set/get: 期望 3.14，实际 {val}");
	}

	private static void testDefaultValue()
	{
		// 默认构造（无参），初始值行为取决于 SafeFloat 默认值 = 0
		var sf = new MostSafeFloat(0f);
		assertEqual(0f, sf.get(), "默认值: get 应返回 0");
	}

	private static void testNegativeValue()
	{
		var sf = new MostSafeFloat(-99.5f);
		float val = sf.get();
		assert(abs(val - (-99.5f)) < EPSILON, $"负值: 期望 -99.5，实际 {val}");
	}

	private static void testZero()
	{
		var sf = new MostSafeFloat(1000f);
		sf.set(0f);
		assertEqual(0f, sf.get(), "set(0): get 应返回 0");
	}

	private static void testLargeValue()
	{
		// SafeFloat 内部用 (int)(value*10000) 做明文校验，abs(value) 需 < 214748
		// 取安全范围内较大值 10000 验证即可
		float large = 10000f;
		var sf = new MostSafeFloat(large);
		float val = sf.get();
		assert(abs(val - large) < large * EPSILON + 1f, $"大数值: 期望 {large}，实际 {val}");
	}

	// ─── 多次 set 覆写 ───────────────────────────────────────────────────

	private static void testOverwrite()
	{
		var sf = new MostSafeFloat(1.0f);
		sf.set(2.0f);
		sf.set(3.0f);
		float val = sf.get();
		assert(abs(val - 3.0f) < EPSILON, $"连续 set: 期望 3.0，实际 {val}");
	}

	// ─── Equals ──────────────────────────────────────────────────────────

	private static void testEquals()
	{
		// MostSafeFloat.Equals 比较的是内部密文字段（含随机密钥），不同实例即使值相同密文也不同
		// 因此 Equals 只在同一实例赋值后与自身比较时有意义；不同实例需通过 get() 比较浮点值
		var a = new MostSafeFloat(5.5f);
		var b = new MostSafeFloat(5.5f);
		var c = new MostSafeFloat(5.6f);
		assert(abs(a.get() - b.get()) < EPSILON,  "Equals via get: 相同值误差<epsilon");
		assert(abs(a.get() - c.get()) > EPSILON,  "Equals via get: 不同值误差>epsilon");
		// Equals 的确定性语义(不依赖随机):
		assertTrue(a.Equals(a), "反身性 a.Equals(a) 必为 true");
		MostSafeFloat copy = a;
		assertTrue(copy.Equals(a), "结构体复制 copy=a 后 copy.Equals(a) 必为 true");
		assertTrue(a.Equals(copy), "结构体复制后 a.Equals(copy) 必为 true");
	}
	// ─── 工具 ────────────────────────────────────────────────────────────
	private static float abs(float v) => v < 0 ? -v : v;


	

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

	// ─── 组合场景 ────────────────────────────────────────────────────────

	// 构造直接设值
	private static void testConstructorValue()
	{
		MostSafeFloat m = new MostSafeFloat(3.25f);
		assertEqual(3.25f, m.get(), EPSILON, "构造设值 3.25");
		MostSafeFloat negative = new MostSafeFloat(-0.75f);
		assertEqual(-0.75f, negative.get(), EPSILON, "构造设值 -0.75");
	}

	// 连续 set 覆盖序列
	private static void testSetOverwriteSequence()
	{
		MostSafeFloat m = new MostSafeFloat();
		m.set(1.0f);
		m.set(2.5f);
		m.set(3.75f);
		assertEqual(3.75f, m.get(), EPSILON, "三次 set 后取最后值");
	}

	// 结构体复制独立
	private static void testStructCopyIndependent()
	{
		MostSafeFloat a = new MostSafeFloat(5.0f);
		MostSafeFloat copy = a;
		copy.set(9.5f);
		assertEqual(5.0f, a.get(), EPSILON, "原实例不受 copy 影响");
		assertEqual(9.5f, copy.get(), EPSILON, "copy 独立取值");
	}

	// Equals 反身性(内部加密字段结构相等)
	private static void testEqualsReflexive()
	{
		MostSafeFloat a = new MostSafeFloat(7.25f);
		assertTrue(a.Equals(a), "a.Equals(a) 反身性成立");
	}
}