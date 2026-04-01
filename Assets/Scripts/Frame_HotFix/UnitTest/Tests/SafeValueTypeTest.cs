#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;
using static MathUtility;

// 安全值类型测试
// 覆盖：SafeInt / SafeFloat / SafeLong / MostSafeInt / MostSafeFloat / MostSafeLong
// 注：所有 Safe* 类型只能在主线程使用，这里的测试是在 onPostInit 主线程中执行的。
public static class SafeValueTypeTest
{
    public static void Run()
    {
        testSafeInt();
        testSafeFloat();
        testSafeLong();
        testMostSafeInt();
        testMostSafeFloat();
        testMostSafeLong();
    }

    // ─── SafeInt ─────────────────────────────────────────────────────────────
    private static void testSafeInt()
    {
        // 基本 set/get
        SafeInt si = new(42);
        assertEqual(42, si.get(), "SafeInt 初值");

        si.set(100);
        assertEqual(100, si.get(), "SafeInt set 100");

        si.set(-1);
        assertEqual(-1, si.get(), "SafeInt 负数");

        si.set(0);
        assertEqual(0, si.get(), "SafeInt 零");

        si.set(int.MaxValue);
        assertEqual(int.MaxValue, si.get(), "SafeInt MaxValue");

        si.set(int.MinValue);
        assertEqual(int.MinValue, si.get(), "SafeInt MinValue");

        // 多次 set/get 保持正确
        for (int i = 0; i < 10; ++i)
        {
            si.set(i * 13 - 50);
            assertEqual(i * 13 - 50, si.get(), $"SafeInt 循环第{i}次");
        }

        // Equals 测试
        SafeInt a = new(999);
        SafeInt b = new(999);
        SafeInt c = new(998);
        assert(a.get() == b.get(), "SafeInt 相同值get相同");
        assert(a.get() != c.get(), "SafeInt 不同值get不同");

        // MaxValue / MinValue 往返
        SafeInt mx = new(int.MaxValue);
        assertEqual(int.MaxValue, mx.get(), "SafeInt MaxValue 往返");
        SafeInt mn = new(int.MinValue);
        assertEqual(int.MinValue, mn.get(), "SafeInt MinValue 往返");

        // 同一实例反复 set 在极限边界切换
        SafeInt edge = new(0);
        edge.set(int.MaxValue);
        assertEqual(int.MaxValue, edge.get(), "SafeInt MaxValue set后get");
        edge.set(int.MinValue);
        assertEqual(int.MinValue, edge.get(), "SafeInt MinValue set后get");
        edge.set(0);
        assertEqual(0, edge.get(), "SafeInt 极限后归零");
    }

    // ─── SafeFloat ───────────────────────────────────────────────────────────
    private static void testSafeFloat()
    {
        SafeFloat sf = new(3.14f);
        assert(isFloatEqual(sf.get(), 3.14f, 0.001f), "SafeFloat 初值");

        sf.set(0.0f);
        assert(isFloatEqual(sf.get(), 0.0f), "SafeFloat 零");

        sf.set(-99.5f);
        assert(isFloatEqual(sf.get(), -99.5f, 0.001f), "SafeFloat 负数");

        // SafeFloat 内部用 (int)(value*10000) 存明文校验，abs(value) 需 < 214748，取安全范围内的大值
        sf.set(10000.0f);
        assert(isFloatEqual(sf.get(), 10000.0f, 0.1f), "SafeFloat 大值");

        // 精度：SafeFloat 内部用 *1000 存储，精度约 0.001
        sf.set(1.234f);
        assert(isFloatEqual(sf.get(), 1.234f, 0.002f), "SafeFloat 精度0.002");

        // SafeFloat 明文校验上限：abs(value)*10000 必须 < int.MaxValue ≈ 214748
        // 使用保守安全值（200000*10000=2e9，太接近 int.MaxValue，改用 100000）
        sf.set(100000.0f);
        assert(isFloatEqual(sf.get(), 100000.0f, 1.0f), "SafeFloat 安全大值100000");

        // 极小值（接近 float 精度底线）
        sf.set(0.001f);
        assert(isFloatEqual(sf.get(), 0.001f, 0.002f), "SafeFloat 极小值0.001");

        // 多次极限交替 set
        sf.set(10000.0f);
        assert(isFloatEqual(sf.get(), 10000.0f, 0.1f), "SafeFloat 大值10000");
        sf.set(-10000.0f);
        assert(isFloatEqual(sf.get(), -10000.0f, 0.1f), "SafeFloat 负大值-10000");
        sf.set(0.0f);
        assert(isFloatEqual(sf.get(), 0.0f), "SafeFloat 极限后归零");

        // 多次 set
        for (int i = 0; i < 10; ++i)
        {
            float expected = i * 0.1f - 0.5f;
            sf.set(expected);
            assert(isFloatEqual(sf.get(), expected, 0.002f), $"SafeFloat 循环第{i}次");
        }
    }

    // ─── SafeLong ────────────────────────────────────────────────────────────
    private static void testSafeLong()
    {
        SafeLong sl = new(1234567890123L);
        assertEqual(1234567890123L, sl.get(), "SafeLong 初值");

        sl.set(0L);
        assertEqual(0L, sl.get(), "SafeLong 零");

        sl.set(-1L);
        assertEqual(-1L, sl.get(), "SafeLong 负数");

        sl.set(long.MaxValue);
        assertEqual(long.MaxValue, sl.get(), "SafeLong MaxValue");

        sl.set(long.MinValue);
        assertEqual(long.MinValue, sl.get(), "SafeLong MinValue");

        // MaxValue → MinValue → 0 切换
        sl.set(long.MaxValue);
        assertEqual(long.MaxValue, sl.get(), "SafeLong MaxValue 再次验证");
        sl.set(long.MinValue);
        assertEqual(long.MinValue, sl.get(), "SafeLong MinValue 再次验证");
        sl.set(0L);
        assertEqual(0L, sl.get(), "SafeLong 极限后归零");

        for (int i = 0; i < 10; ++i)
        {
            long expected = (long)i * 100000000L - 500000000L;
            sl.set(expected);
            assertEqual(expected, sl.get(), $"SafeLong 循环第{i}次");
        }
    }

    // ─── MostSafeInt ─────────────────────────────────────────────────────────
    private static void testMostSafeInt()
    {
        MostSafeInt mi = new(77);
        assertEqual(77, mi.get(), "MostSafeInt 初值");

        mi.set(0);
        assertEqual(0, mi.get(), "MostSafeInt 零");

        mi.set(-500);
        assertEqual(-500, mi.get(), "MostSafeInt 负数");

        mi.set(int.MaxValue);
        assertEqual(int.MaxValue, mi.get(), "MostSafeInt MaxValue");

        for (int i = 0; i < 8; ++i)
        {
            mi.set(i * 7 - 28);
            assertEqual(i * 7 - 28, mi.get(), $"MostSafeInt 循环第{i}次");
        }
    }

    // ─── MostSafeFloat ───────────────────────────────────────────────────────
    private static void testMostSafeFloat()
    {
        MostSafeFloat mf = new(2.5f);
        assert(isFloatEqual(mf.get(), 2.5f, 0.01f), "MostSafeFloat 初值");

        mf.set(0.0f);
        assert(isFloatEqual(mf.get(), 0.0f), "MostSafeFloat 零");

        mf.set(-10.0f);
        assert(isFloatEqual(mf.get(), -10.0f, 0.01f), "MostSafeFloat 负数");

        mf.set(0.001f);
        assert(isFloatEqual(mf.get(), 0.001f, 0.002f), "MostSafeFloat 小值");

        for (int i = 0; i < 8; ++i)
        {
            float expected = i * 0.5f - 2.0f;
            mf.set(expected);
            assert(isFloatEqual(mf.get(), expected, 0.01f), $"MostSafeFloat 循环第{i}次");
        }
    }

    // ─── MostSafeLong ────────────────────────────────────────────────────────
    private static void testMostSafeLong()
    {
        MostSafeLong ml = new(9876543210L);
        assertEqual(9876543210L, ml.get(), "MostSafeLong 初值");

        ml.set(0L);
        assertEqual(0L, ml.get(), "MostSafeLong 零");

        ml.set(-100L);
        assertEqual(-100L, ml.get(), "MostSafeLong 负数");

        ml.set(long.MaxValue);
        assertEqual(long.MaxValue, ml.get(), "MostSafeLong MaxValue");

        for (int i = 0; i < 8; ++i)
        {
            long expected = (long)i * 999999999L - 4000000000L;
            ml.set(expected);
            assertEqual(expected, ml.get(), $"MostSafeLong 循环第{i}次");
        }
    }
}
#endif
