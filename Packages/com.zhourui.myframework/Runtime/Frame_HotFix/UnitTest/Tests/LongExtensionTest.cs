using static TestAssert;

// LongExtension 中 MathExtensionTest 未覆盖的方法测试
// 已覆盖: clamp(long), clampMin(long), clampMax(long), divide(long/float), divide(long/long), divideLong
public static class LongExtensionTest
{
    public static void Run()
    {
        testAbs();
        testClampMinUlong();
        testClampMaxUlong();
    }

    // ---- abs ----
    static void testAbs()
    {
        assertEqual(5L, 5L.abs(), "abs 5");
        assertEqual(5L, (-5L).abs(), "abs -5");
        assertEqual(0L, 0L.abs(), "abs 0");
        assertEqual(10000000000L, (-10000000000L).abs(), "abs large negative");
    }

    // ---- clampMin(ulong) ----
    static void testClampMinUlong()
    {
        assertEqual(5UL, 5UL.clampMin(3UL), "clampMin ulong no clamp");
        assertEqual(3UL, 1UL.clampMin(3UL), "clampMin ulong clamped");
        assertEqual(5UL, 5UL.clampMin(), "clampMin ulong default 0");
        assertEqual(0UL, 0UL.clampMin(), "clampMin ulong 0 default");
    }

    // ---- clampMax(ulong) ----
    static void testClampMaxUlong()
    {
        assertEqual(5UL, 5UL.clampMax(10UL), "clampMax ulong no clamp");
        assertEqual(10UL, 15UL.clampMax(10UL), "clampMax ulong clamped");
        assertEqual(0UL, 0UL.clampMax(10UL), "clampMax ulong 0");
    }
}
