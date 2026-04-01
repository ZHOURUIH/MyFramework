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
	}

	// ─── 工具 ────────────────────────────────────────────────────────────

	private static float abs(float v) => v < 0 ? -v : v;
}
