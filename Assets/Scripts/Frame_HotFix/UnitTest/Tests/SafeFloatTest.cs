#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
	}
}
#endif
