using static TestAssert;

public static class MostSafeLongTest
{
	public static void Run()
	{
		testSetAndGet();
		testDefaultValue();
		testNegative();
		testZero();
		testOverwrite();
		testMaxValue();
		testMinValue();
		testMultipleInstances();
		testLargeValue();
	}

	private static void testSetAndGet()
	{
		MostSafeLong v = new();
		v.set(42L);
		assertEqual(42L, v.get(), "set/get 42");
	}

	private static void testDefaultValue()
	{
		MostSafeLong v = new(0L);
		assertEqual(0L, v.get(), "default 0");
	}

	private static void testNegative()
	{
		MostSafeLong v = new(-9876543210L);
		assertEqual(-9876543210L, v.get(), "negative");
	}

	private static void testZero()
	{
		MostSafeLong v = new();
		v.set(100L);
		v.set(0L);
		assertEqual(0L, v.get(), "set to 0");
	}

	private static void testOverwrite()
	{
		MostSafeLong v = new();
		v.set(10L);
		v.set(20L);
		v.set(30L);
		assertEqual(30L, v.get(), "overwrite 10→20→30");
	}

	private static void testMaxValue()
	{
		MostSafeLong v = new(long.MaxValue);
		assertEqual(long.MaxValue, v.get(), "MaxValue");
	}

	private static void testMinValue()
	{
		MostSafeLong v = new(long.MinValue);
		assertEqual(long.MinValue, v.get(), "MinValue");
	}

	private static void testMultipleInstances()
	{
		MostSafeLong a = new(100L);
		MostSafeLong b = new(200L);
		assertEqual(100L, a.get());
		assertEqual(200L, b.get());
		a.set(300L);
		assertEqual(300L, a.get());
		assertEqual(200L, b.get()); // b 不受影响
	}

	private static void testLargeValue()
	{
		MostSafeLong v = new(9223372036854775807L);
		assertEqual(9223372036854775807L, v.get(), "large value");
	}
}
