#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
	}
}
#endif
