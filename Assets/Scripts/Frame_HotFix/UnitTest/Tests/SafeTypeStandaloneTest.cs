#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// SafeFloat/SafeInt/SafeLong 构造和 get/set 测试
public static class SafeTypeStandaloneTest
{
	public static void Run()
	{
		testSafeFloat();
		testSafeInt();
		testSafeLong();
	}

	private static void testSafeFloat()
	{
		var sf = new SafeFloat(3.14f);
		float v = sf.get();
		Assert(v > 3.13f && v < 3.15f);
		sf.set(0f);
		float z = sf.get();
		Assert(z > -0.01f && z < 0.01f);
	}

	private static void testSafeInt()
	{
		SafeInt si = new(42);
		AssertEqual(42, si.get());
		si.set(0);
		AssertEqual(0, si.get());
		si.set(-100);
		AssertEqual(-100, si.get());
	}

	private static void testSafeLong()
	{
		SafeLong sl = new(9999999999L);
		AssertEqual(9999999999L, sl.get());
		sl.set(0L);
		AssertEqual(0L, sl.get());
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(long e, long a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
