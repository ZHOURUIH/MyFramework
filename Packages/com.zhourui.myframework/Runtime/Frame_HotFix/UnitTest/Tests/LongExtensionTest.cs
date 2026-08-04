using static TestAssert;

// LongExtension 纯数学扩展方法测试
public static class LongExtensionTest
{
    public static void Run()
    {
        testAbs();
        testClamp();
        testClampMin();
        testClampMax();
        testDivideFloat();
        testDivideLong();
        testDivideLongLong();
        testClampMinULong();
        testClampMaxULong();
    }

    // ---- abs ----
    static void testAbs()
    {
        assertEqual(5L, 5L.abs(), "abs 5 -> 5");
        assertEqual(5L, (-5L).abs(), "abs -5 -> 5");
        assertEqual(0L, 0L.abs(), "abs 0 -> 0");
        assertEqual(1L, (-1L).abs(), "abs -1 -> 1");
        // 注意: long.MinValue.abs() 在 C# 中会溢出回 MinValue (unchecked 行为)
        // 不对此边界情况做断言
    }

    // ---- clamp ----
    static void testClamp()
    {
        // 正常范围
        assertEqual(5L, 5L.clamp(1L, 10L), "clamp 5 [1,10] -> 5");
        assertEqual(1L, 0L.clamp(1L, 10L), "clamp 0 [1,10] -> 1");
        assertEqual(10L, 11L.clamp(1L, 10L), "clamp 11 [1,10] -> 10");
        assertEqual(1L, 1L.clamp(1L, 10L), "clamp 1 [1,10] -> 1 (边界)");
        assertEqual(10L, 10L.clamp(1L, 10L), "clamp 10 [1,10] -> 10 (边界)");

        // min > max 时返回原值
        assertEqual(5L, 5L.clamp(10L, 1L), "clamp 5 [10,1] min>max -> 5");
        assertEqual(0L, 0L.clamp(10L, 1L), "clamp 0 [10,1] min>max -> 0");

        // min == max
        assertEqual(5L, 100L.clamp(5L, 5L), "clamp 100 [5,5] min==max -> 5");
        assertEqual(5L, 0L.clamp(5L, 5L), "clamp 0 [5,5] min==max -> 5");
        assertEqual(5L, 5L.clamp(5L, 5L), "clamp 5 [5,5] min==max -> 5");

        // 负数范围
        assertEqual(-5L, (-5L).clamp(-10L, -1L), "clamp -5 [-10,-1] -> -5");
        assertEqual(-10L, (-15L).clamp(-10L, -1L), "clamp -15 [-10,-1] -> -10");
        assertEqual(-1L, 0L.clamp(-10L, -1L), "clamp 0 [-10,-1] -> -1");
    }

    // ---- clampMin (long) ----
    static void testClampMin()
    {
        assertEqual(5L, 5L.clampMin(3L), "clampMin 5 min=3 -> 5");
        assertEqual(3L, 2L.clampMin(3L), "clampMin 2 min=3 -> 3");
        assertEqual(3L, 3L.clampMin(3L), "clampMin 3 min=3 -> 3 (边界)");
        // 默认 min=0
        assertEqual(5L, 5L.clampMin(), "clampMin 5 默认min=0 -> 5");
        assertEqual(0L, (-1L).clampMin(), "clampMin -1 默认min=0 -> 0");
        assertEqual(0L, 0L.clampMin(), "clampMin 0 默认min=0 -> 0");
        // 负数 min
        assertEqual(-1L, (-1L).clampMin(-5L), "clampMin -1 min=-5 -> -1");
        assertEqual(-5L, (-10L).clampMin(-5L), "clampMin -10 min=-5 -> -5");
    }

    // ---- clampMax (long) ----
    static void testClampMax()
    {
        assertEqual(5L, 5L.clampMax(10L), "clampMax 5 max=10 -> 5");
        assertEqual(10L, 15L.clampMax(10L), "clampMax 15 max=10 -> 10");
        assertEqual(10L, 10L.clampMax(10L), "clampMax 10 max=10 -> 10 (边界)");
        // 负数 max
        assertEqual(-5L, (-5L).clampMax(-1L), "clampMax -5 max=-1 -> -5");
        assertEqual(-1L, 0L.clampMax(-1L), "clampMax 0 max=-1 -> -1");
        // 零 max
        assertEqual(0L, 5L.clampMax(0L), "clampMax 5 max=0 -> 0");
        assertEqual(-5L, (-5L).clampMax(0L), "clampMax -5 max=0 -> -5");
    }

    // ---- divide(long, float) ----
    static void testDivideFloat()
    {
        assertEqual(5.0f, 10L.divide(2.0f), "divide 10/2.0 -> 5");
        assertEqual(3.33333f, 10L.divide(3.0f), 0.001f, "divide 10/3.0 ~ 3.333");
        // 除零返回默认值
        assertEqual(0.0f, 10L.divide(0.0f), "divide 10/0.0 默认 -> 0");
        assertEqual(99.0f, 10L.divide(0.0f, 99.0f), "divide 10/0.0 自定义默认 -> 99");
        // 正常带默认值（非零分母）
        assertEqual(5.0f, 10L.divide(2.0f, 99.0f), "divide 10/2.0 带默认 -> 5");
    }

    // ---- divide(long, long) ----
    static void testDivideLong()
    {
        assertEqual(5.0f, 10L.divide(2L), "divide 10/2 -> 5");
        assertEqual(3.0f, 9L.divide(3L), "divide 9/3 -> 3");
        // 除零返回默认值
        assertEqual(0.0f, 10L.divide(0L), "divide 10/0 默认 -> 0");
        assertEqual(99.0f, 10L.divide(0L, 99.0f), "divide 10/0 自定义默认 -> 99");
        // 正常带默认值
        assertEqual(5.0f, 10L.divide(2L, 99.0f), "divide 10/2 带默认 -> 5");
    }

    // ---- divideLong ----
    static void testDivideLongLong()
    {
        assertEqual(5L, 10L.divideLong(2L), "divideLong 10/2 -> 5");
        assertEqual(3L, 10L.divideLong(3L), "divideLong 10/3 -> 3 (整数除)");
        assertEqual(0L, 5L.divideLong(10L), "divideLong 5/10 -> 0");
        // 除零返回默认值
        assertEqual(0L, 10L.divideLong(0L), "divideLong 10/0 默认 -> 0");
        assertEqual(99L, 10L.divideLong(0L, 99L), "divideLong 10/0 自定义默认 -> 99");
        // 正常带默认值
        assertEqual(5L, 10L.divideLong(2L, 99L), "divideLong 10/2 带默认 -> 5");
    }

    // ---- clampMin (ulong) ----
    static void testClampMinULong()
    {
        assertEqual(5UL, 5UL.clampMin(3UL), "clampMin ulong 5 min=3 -> 5");
        assertEqual(3UL, 2UL.clampMin(3UL), "clampMin ulong 2 min=3 -> 3");
        assertEqual(3UL, 3UL.clampMin(3UL), "clampMin ulong 3 min=3 -> 3 (边界)");
        // 默认 min=0
        assertEqual(5UL, 5UL.clampMin(), "clampMin ulong 5 默认min=0 -> 5");
        assertEqual(0UL, 0UL.clampMin(), "clampMin ulong 0 默认min=0 -> 0");
    }

    // ---- clampMax (ulong) ----
    static void testClampMaxULong()
    {
        assertEqual(5UL, 5UL.clampMax(10UL), "clampMax ulong 5 max=10 -> 5");
        assertEqual(10UL, 15UL.clampMax(10UL), "clampMax ulong 15 max=10 -> 10");
        assertEqual(10UL, 10UL.clampMax(10UL), "clampMax ulong 10 max=10 -> 10 (边界)");
        assertEqual(0UL, 5UL.clampMax(0UL), "clampMax ulong 5 max=0 -> 0");
        assertEqual(0UL, 0UL.clampMax(0UL), "clampMax ulong 0 max=0 -> 0");
    }
}
