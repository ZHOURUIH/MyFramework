using static TestAssert;

// DoubleExtension 纯数学扩展方法测试
public static class DoubleExtensionTest
{
    public static void Run()
    {
        testFloor();
        testRound();
        testAbs();
        testClampMin();
        testClampMax();
        testIsZero();
        testIsEqual();
        testInverse();
        testDivide();
        testFloorNegative();
        testRoundHalf();
        testAbsNegativeZero();
        testClampMinMaxChain();
        testIsZeroPrecision();
        testIsEqualChain();
        testInverseZeroSafe();
        testDivideZeroDefault();
        testDivideNegative();
        testClampChainExtreme();
    }

    // ---- floor ----
    static void testFloor()
    {
        assertEqual(3, 3.2.floor(), "floor 3.2 -> 3");
        assertEqual(3, 3.8.floor(), "floor 3.8 -> 3");
        assertEqual(3, 3.0.floor(), "floor 3.0 -> 3");
        assertEqual(-4, (-3.2).floor(), "floor -3.2 -> -4");
        assertEqual(-4, (-3.8).floor(), "floor -3.8 -> -4");
        assertEqual(-4, (-4.0).floor(), "floor -4.0 -> -4");
        assertEqual(0, 0.0.floor(), "floor 0 -> 0");
        assertEqual(-1, (-1.0).floor(), "floor -1.0 -> -1");
        // 整数边界
        assertEqual(5, 5.0.floor(), "floor 5.0 -> 5");
        assertEqual(-5, (-5.0).floor(), "floor -5.0 -> -5");
    }

    // ---- round ----
    static void testRound()
    {
        assertEqual(3L, 3.2.round(), "round 3.2 -> 3");
        assertEqual(4L, 3.8.round(), "round 3.8 -> 4");
        assertEqual(4L, 3.5.round(), "round 3.5 -> 4");
        assertEqual(-3L, (-3.2).round(), "round -3.2 -> -3");
        assertEqual(-4L, (-3.8).round(), "round -3.8 -> -4");
        assertEqual(-4L, (-3.5).round(), "round -3.5 -> -4");
        assertEqual(0L, 0.0.round(), "round 0 -> 0");
        assertEqual(1L, 0.5.round(), "round 0.5 -> 1");
        assertEqual(-1L, (-0.5).round(), "round -0.5 -> -1");
    }

    // ---- abs ----
    static void testAbs()
    {
        assertEqual(5.0, 5.0.abs(), "abs 5 -> 5");
        assertEqual(5.0, (-5.0).abs(), "abs -5 -> 5");
        assertEqual(0.0, 0.0.abs(), "abs 0 -> 0");
        assertEqual(3.14, (-3.14).abs(), "abs -3.14 -> 3.14");
    }

    // ---- clampMin ----
    static void testClampMin()
    {
        assertEqual(5.0, 5.0.clampMin(3.0), "clampMin 5 min=3 -> 5");
        assertEqual(3.0, 2.0.clampMin(3.0), "clampMin 2 min=3 -> 3");
        assertEqual(3.0, 3.0.clampMin(3.0), "clampMin 3 min=3 -> 3 (边界)");
        // 默认 min=0
        assertEqual(5.0, 5.0.clampMin(), "clampMin 5 默认min=0 -> 5");
        assertEqual(0.0, (-1.0).clampMin(), "clampMin -1 默认min=0 -> 0");
        assertEqual(0.0, 0.0.clampMin(), "clampMin 0 默认min=0 -> 0");
        // 负数 min
        assertEqual(-1.0, (-1.0).clampMin(-5.0), "clampMin -1 min=-5 -> -1");
        assertEqual(-5.0, (-10.0).clampMin(-5.0), "clampMin -10 min=-5 -> -5");
    }

    // ---- clampMax ----
    static void testClampMax()
    {
        assertEqual(5.0, 5.0.clampMax(10.0), "clampMax 5 max=10 -> 5");
        assertEqual(10.0, 15.0.clampMax(10.0), "clampMax 15 max=10 -> 10");
        assertEqual(10.0, 10.0.clampMax(10.0), "clampMax 10 max=10 -> 10 (边界)");
        // 负数 max
        assertEqual(-5.0, (-5.0).clampMax(-1.0), "clampMax -5 max=-1 -> -5");
        assertEqual(-1.0, 0.0.clampMax(-1.0), "clampMax 0 max=-1 -> -1");
        // 零 max
        assertEqual(0.0, 5.0.clampMax(0.0), "clampMax 5 max=0 -> 0");
    }

    // ---- isZero ----
    static void testIsZero()
    {
        assertTrue(0.0.isZero(), "0 is zero");
        assertTrue(0.000000001.isZero(), "tiny is zero");
        assertTrue((-0.000000001).isZero(), "neg tiny is zero");
        assertFalse(0.001.isZero(), "0.001 not zero");
        assertFalse(1.0.isZero(), "1 not zero");
        assertFalse((-1.0).isZero(), "-1 not zero");
        // 自定义精度
        assertTrue(0.01.isZero(0.1), "0.01 zero with precision 0.1");
        assertFalse(0.01.isZero(0.001), "0.01 not zero with precision 0.001");
    }

    // ---- isEqual ----
    static void testIsEqual()
    {
        assertTrue(1.0.isEqual(1.0), "1 == 1");
        assertTrue(1.000000001.isEqual(1.0), "1+1e-9 == 1");
        assertFalse(0.001.isEqual(0.0), "0.001 != 0");
        assertFalse(1.0.isEqual(2.0), "1 != 2");
        assertTrue((-1.0).isEqual(-1.0), "-1 == -1");
        // 自定义精度
        assertTrue(0.01.isEqual(0.02, 0.1), "0.01==0.02 with precision 0.1");
        assertFalse(0.01.isEqual(0.02, 0.001), "0.01!=0.02 with precision 0.001");
    }

    // ---- inverse ----
    static void testInverse()
    {
        assertTrue(0.5.isEqual(2.0.inverse()), "inverse 2 -> 0.5");
        assertTrue(2.0.isEqual(0.5.inverse()), "inverse 0.5 -> 2");
        assertTrue(1.0.isEqual(1.0.inverse()), "inverse 1 -> 1");
        assertTrue((-0.5).isEqual((-2.0).inverse()), "inverse -2 -> -0.5");
        // 零的倒数返回 0
        assertEqual(0.0, 0.0.inverse(), "inverse 0 -> 0");
    }

    // ---- divide ----
    static void testDivide()
    {
        assertTrue(5.0.isEqual(10.0.divide(2.0)), "divide 10/2 -> 5");
        assertTrue(3.33333.isEqual(10.0.divide(3.0), 0.001), "divide 10/3 ~ 3.333");
        // 除零返回默认值
        assertEqual(0.0, 10.0.divide(0.0), "divide 10/0 默认 -> 0");
        assertEqual(99.0, 10.0.divide(0.0, 99.0), "divide 10/0 自定义默认 -> 99");
        // 正常带默认值
        assertTrue(5.0.isEqual(10.0.divide(2.0, 99.0)), "divide 10/2 带默认 -> 5");
    }

    // ---- 组合场景 ----

    // floor 负值边界
    static void testFloorNegative()
    {
        assertEqual(-2, (-1.7).floor(), "floor(-1.7)=-2");
        assertEqual(-1, (-0.3).floor(), "floor(-0.3)=-1");
        assertEqual(0, 0.0.floor(), "floor(0)=0");
    }

    // round 四舍五入边界
    static void testRoundHalf()
    {
        assertEqual(3L, 2.5.round(), "round(2.5)=3");
        assertEqual(-3L, (-2.5).round(), "round(-2.5)=-3(实现: 负值 value-0.5 取整)");
        assertEqual(0L, 0.4.round(), "round(0.4)=0");
        assertEqual(1L, 0.6.round(), "round(0.6)=1");
    }

    // abs(-0.0) 与负值
    static void testAbsNegativeZero()
    {
        assertEqual(0.0, (-0.0).abs(), "abs(-0.0)=0");
        assertEqual(3.5, (-3.5).abs(), "abs(-3.5)=3.5");
    }

    // 链式: clampMin 后 clampMax
    static void testClampMinMaxChain()
    {
        double v = (-5.0).clampMin(0.0).clampMax(10.0);
        assertEqual(0.0, v, "clampMin(-5) 后 clampMax = 0");
        v = 15.0.clampMin(0.0).clampMax(10.0);
        assertEqual(10.0, v, "clampMax(15) 后 = 10");
        v = 5.0.clampMin(0.0).clampMax(10.0);
        assertEqual(5.0, v, "范围内值不变");
    }

    // isZero 精度参数
    static void testIsZeroPrecision()
    {
        assertTrue(0.000000001.isZero(), "默认精度下 1e-9 视为 0");
        assertFalse(0.000000001.isZero(0.0000000001), "更高精度下 1e-9 非 0");
        assertTrue(0.0.isZero(), "0 恒为 0");
    }

    // isEqual 链: 传递性近似比较
    static void testIsEqualChain()
    {
        double a = 1.0 / 3.0;
        double b = 0.3333333333;
        double c = 0.3333333334;
        assertTrue(a.isEqual(b, 0.0001), "a≈b");
        assertTrue(b.isEqual(c, 0.0001), "b≈c");
        assertTrue(a.isEqual(c, 0.0001), "a≈c(传递)");
    }

    // inverse(0) 安全返回 0
    static void testInverseZeroSafe()
    {
        assertEqual(0.0, 0.0.inverse(), "inverse(0)=0");
        assertTrue(0.5.isEqual(2.0.inverse(), 0.0001), "inverse(2)=0.5");
        assertTrue((-0.25).isEqual((-4.0).inverse(), 0.0001), "inverse(-4)=-0.25");
    }

    // divide 除零默认值
    static void testDivideZeroDefault()
    {
        assertEqual(0.0, 8.0.divide(0.0), "除零默认 0");
        assertEqual(-1.0, 8.0.divide(0.0, -1.0), "除零自定义默认 -1");
        assertEqual(4.0, 8.0.divide(2.0), "正常除法");
    }

    // divide 负数
    static void testDivideNegative()
    {
        assertEqual(-2.5, (-5.0).divide(2.0), "-5/2=-2.5");
        assertEqual(-2.5, 5.0.divide(-2.0), "5/-2=-2.5(负号保留)");
        assertEqual(2.0, (-8.0).divide(-4.0), "-8/-4=2");
    }

    // 极端链式: 负数 clamp 组合
    static void testClampChainExtreme()
    {
        double v = (-100.0).clampMin(-50.0).clampMax(50.0);
        assertEqual(-50.0, v, "clampMin(-100,-50) 后 = -50");
        v = 100.0.clampMin(-50.0).clampMax(50.0);
        assertEqual(50.0, v, "clampMax(100,50) 后 = 50");
        v = (-0.001).clampMin(0.0);
        assertEqual(0.0, v, "clampMin 负微值 → 0");
    }
}
