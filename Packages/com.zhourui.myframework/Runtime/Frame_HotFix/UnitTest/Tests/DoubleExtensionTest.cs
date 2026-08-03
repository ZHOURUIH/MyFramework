using static TestAssert;

// DoubleExtension 纯数学扩展方法测试
public static class DoubleExtensionTest
{
    public static void Run()
    {
        testFloor();
        testRound();
        testAbs();
        testClampMinMax();
        testIsZero();
        testIsEqual();
        testInverse();
        testDivide();
    }

    // ---- floor ----
    static void testFloor()
    {
        // 正数向下取整
        assertEqual(3, 3.2.floor(), "floor 3.2 -> 3");
        assertEqual(3, 3.8.floor(), "floor 3.8 -> 3");
        assertEqual(3, 3.0.floor(), "floor 3.0 -> 3");
        // 负数向下取整（数轴上更小）
        assertEqual(-4, (-3.2).floor(), "floor -3.2 -> -4");
        assertEqual(-4, (-3.8).floor(), "floor -3.8 -> -4");
        assertEqual(-4, (-4.0).floor(), "floor -4.0 -> -4");
        // 0
        assertEqual(0, 0.0.floor(), "floor 0 -> 0");
        // 整数负数的 floor：(-1.0) 转 int 为 -1，条件 -1.0 < -1 为 false，不减 1
        assertEqual(-1, (-1.0).floor(), "floor -1.0 -> -1");
        assertEqual(-2, (-1.1).floor(), "floor -1.1 -> -2");
    }

    // ---- round ----
    static void testRound()
    {
        // 正数四舍五入
        assertEqual(3L, 3.2.round(), "round 3.2 -> 3");
        assertEqual(4L, 3.8.round(), "round 3.8 -> 4");
        assertEqual(4L, 3.5.round(), "round 3.5 -> 4");
        // 负数四舍五入
        assertEqual(-3L, (-3.2).round(), "round -3.2 -> -3");
        assertEqual(-4L, (-3.8).round(), "round -3.8 -> -4");
        assertEqual(-4L, (-3.5).round(), "round -3.5 -> -4");
        // 0
        assertEqual(0L, 0.0.round(), "round 0 -> 0");
        // 边界: 0.5
        assertEqual(1L, 0.5.round(), "round 0.5 -> 1");
        assertEqual(-1L, (-0.5).round(), "round -0.5 -> -1");
    }

    // ---- abs ----
    static void testAbs()
    {
        assertTrue(5.0.abs().isEqual(5.0, 0.00000001), "abs 5");
        assertTrue((-5.0).abs().isEqual(5.0, 0.00000001), "abs -5");
        assertTrue(0.0.abs().isZero(0.00000001), "abs 0");
        assertTrue(3.14.abs().isEqual(3.14, 0.00000001), "abs 3.14");
        assertTrue((-3.14).abs().isEqual(3.14, 0.00000001), "abs -3.14");
    }

    // ---- clampMin / clampMax ----
    static void testClampMinMax()
    {
        // clampMin
        assertTrue(5.0.clampMin(3.0).isEqual(5.0, 0.00000001), "clampMin no clamp");
        assertTrue(2.0.clampMin(3.0).isEqual(3.0, 0.00000001), "clampMin clamped");
        assertTrue(3.0.clampMin(3.0).isEqual(3.0, 0.00000001), "clampMin equal");
        assertTrue(5.0.clampMin().isEqual(5.0, 0.00000001), "clampMin default min=0");
        assertTrue((-1.0).clampMin().isZero(0.00000001), "clampMin negative to 0");

        // clampMax
        assertTrue(5.0.clampMax(10.0).isEqual(5.0, 0.00000001), "clampMax no clamp");
        assertTrue(15.0.clampMax(10.0).isEqual(10.0, 0.00000001), "clampMax clamped");
        assertTrue(10.0.clampMax(10.0).isEqual(10.0, 0.00000001), "clampMax equal");
        assertTrue((-5.0).clampMax(10.0).isEqual(-5.0, 0.00000001), "clampMax negative");
    }

    // ---- isZero ----
    static void testIsZero()
    {
        assertTrue(0.0.isZero(), "isZero 0");
        assertTrue(0.000000001.isZero(), "isZero within default precision");
        assertTrue((-0.000000001).isZero(), "isZero negative within precision");
        assertFalse(0.0001.isZero(), "isZero outside default precision");
        assertFalse(1.0.isZero(), "isZero 1");
        // 自定义精度
        assertTrue(0.001.isZero(0.01), "isZero custom precision");
        assertFalse(0.1.isZero(0.01), "isZero outside custom precision");
    }

    // ---- isEqual ----
    static void testIsEqual()
    {
        assertTrue(1.0.isEqual(1.0), "isEqual same");
        assertTrue(1.0.isEqual(1.0000000001), "isEqual within precision");
        assertFalse(1.0.isEqual(2.0), "isEqual different");
        // 自定义精度
        assertTrue(1.0.isEqual(1.1, 0.2), "isEqual custom precision");
        assertFalse(1.0.isEqual(1.3, 0.2), "isEqual outside custom precision");
    }

    // ---- inverse ----
    static void testInverse()
    {
        assertTrue(2.0.inverse().isEqual(0.5, 0.00000001), "inverse 2 -> 0.5");
        assertTrue(1.0.inverse().isEqual(1.0, 0.00000001), "inverse 1 -> 1");
        assertTrue(0.5.inverse().isEqual(2.0, 0.00000001), "inverse 0.5 -> 2");
        assertTrue(0.0.inverse().isZero(0.00000001), "inverse 0 -> 0");
        assertTrue((-2.0).inverse().isEqual(-0.5, 0.00000001), "inverse -2 -> -0.5");
    }

    // ---- divide ----
    static void testDivide()
    {
        // 正常除法
        assertTrue(10.0.divide(3.0).isEqual(3.33333333, 0.0000001), "divide 10/3");
        assertTrue(10.0.divide(2.0).isEqual(5.0, 0.00000001), "divide 10/2");
        assertTrue(0.0.divide(5.0).isZero(0.00000001), "divide 0/5");
        // 除零返回默认值
        assertTrue(10.0.divide(0.0).isZero(0.00000001), "divide by 0 default 0");
        assertTrue(10.0.divide(0.0, -1.0).isEqual(-1.0, 0.00000001), "divide by 0 custom default");
        // 负数
        assertTrue((-10.0).divide(3.0).isEqual(-3.33333333, 0.0000001), "divide -10/3");
        assertTrue(10.0.divide(-3.0).isEqual(-3.33333333, 0.0000001), "divide 10/-3");
    }
}
