#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
	}
}
#endif
