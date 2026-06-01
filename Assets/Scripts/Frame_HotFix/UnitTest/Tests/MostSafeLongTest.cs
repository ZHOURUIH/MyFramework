#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
}
#endif
