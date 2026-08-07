using UnityEngine;
using static TestAssert;

// IntExtension 纯逻辑扩展方法测试
public static class IntExtensionTest
{
    public static void Run()
    {
        testAbs();
        testSqrt();
        testClamp();
        testClampMin();
        testClampMax();
        testInverse();
        testDivideInt();
        testDivideFloat();
        testDivideIntInt();
        testClampCycle();
        testInRangeInt();
        testInRangeFloat();
        testInRangeFixedInt();
        testInRangeFixedFloat();
        testGenerateBatchCount();
        testIndexToX();
        testIndexToY();
        testIndexToIntPos();
        testHasMask();
        testInversePow10();
        testPow10();
        testInversePow10Long();
        testPow10Long();
        testPow2();
        testIsPow2();
        testIsEven();
        testGetGreaterPowValue();
        testGetGreaterPow2();
        // 其他整数类型扩展
        testSByteAbs();
        testShortAbs();
        testByteClampMin();
        testSByteClampMin();
        testShortClampMin();
        testUShortClampMin();
        testUIntClampMin();
        testByteClampMax();
        testSByteClampMax();
        testShortClampMax();
        testUShortClampMax();
        testUIntClampMax();
    }

    // ---- abs ----
    static void testAbs()
    {
        assertEqual(5, 5.abs(), "abs 5 -> 5");
        assertEqual(5, (-5).abs(), "abs -5 -> 5");
        assertEqual(0, 0.abs(), "abs 0 -> 0");
        assertEqual(1, (-1).abs(), "abs -1 -> 1");
    }

    // ---- sqrt ----
    static void testSqrt()
    {
        assertTrue((4.sqrt() - 2.0f).isZero(), "sqrt 4 -> 2");
        assertTrue((9.sqrt() - 3.0f).isZero(), "sqrt 9 -> 3");
        assertTrue((0.sqrt()).isZero(), "sqrt 0 -> 0");
        assertTrue((1.sqrt() - 1.0f).isZero(), "sqrt 1 -> 1");
    }

    // ---- clamp ----
    static void testClamp()
    {
        assertEqual(5, 5.clamp(1, 10), "clamp 5 [1,10] -> 5");
        assertEqual(1, 0.clamp(1, 10), "clamp 0 [1,10] -> 1");
        assertEqual(10, 11.clamp(1, 10), "clamp 11 [1,10] -> 10");
        assertEqual(1, 1.clamp(1, 10), "clamp 1 [1,10] -> 1 (边界)");
        assertEqual(10, 10.clamp(1, 10), "clamp 10 [1,10] -> 10 (边界)");
        // min > max 返回原值
        assertEqual(5, 5.clamp(10, 1), "clamp 5 [10,1] min>max -> 5");
        // min == max
        assertEqual(5, 100.clamp(5, 5), "clamp 100 [5,5] -> 5");
        assertEqual(5, 0.clamp(5, 5), "clamp 0 [5,5] -> 5");
    }

    // ---- clampMin ----
    static void testClampMin()
    {
        assertEqual(5, 5.clampMin(3), "clampMin 5 min=3 -> 5");
        assertEqual(3, 2.clampMin(3), "clampMin 2 min=3 -> 3");
        assertEqual(3, 3.clampMin(3), "clampMin 3 min=3 -> 3");
        // 默认 min=0
        assertEqual(5, 5.clampMin(), "clampMin 5 default -> 5");
        assertEqual(0, (-1).clampMin(), "clampMin -1 default -> 0");
    }

    // ---- clampMax ----
    static void testClampMax()
    {
        assertEqual(5, 5.clampMax(10), "clampMax 5 max=10 -> 5");
        assertEqual(10, 15.clampMax(10), "clampMax 15 max=10 -> 10");
        assertEqual(10, 10.clampMax(10), "clampMax 10 max=10 -> 10");
        assertEqual(0, 5.clampMax(0), "clampMax 5 max=0 -> 0");
    }

    // ---- inverse ----
    static void testInverse()
    {
        assertTrue((2.inverse() - 0.5f).isZero(), "inverse 2 -> 0.5");
        assertTrue((4.inverse() - 0.25f).isZero(), "inverse 4 -> 0.25");
        assertTrue(1.inverse().isEqual(1.0f), "inverse 1 -> 1");
        assertTrue((-2.inverse() + 0.5f).isZero(), "inverse -2 -> -0.5");
        assertEqual(0.0f, 0.inverse(), "inverse 0 -> 0");
    }

    // ---- divide(int, int) ----
    static void testDivideInt()
    {
        assertTrue((10.divide(2) - 5.0f).isZero(), "divide 10/2 -> 5");
        assertEqual(0.0f, 10.divide(0), "divide 10/0 默认 -> 0");
        assertEqual(99.0f, 10.divide(0, 99.0f), "divide 10/0 自定义默认 -> 99");
        assertTrue((10.divide(2, 99.0f) - 5.0f).isZero(), "divide 10/2 带默认 -> 5");
    }

    // ---- divide(int, float) ----
    static void testDivideFloat()
    {
        assertTrue((10.divide(2.0f) - 5.0f).isZero(), "divide 10/2.0f -> 5");
        assertEqual(0.0f, 10.divide(0.0f), "divide 10/0.0 默认 -> 0");
        assertEqual(99.0f, 10.divide(0.0f, 99.0f), "divide 10/0.0 自定义默认 -> 99");
    }

    // ---- divideInt ----
    static void testDivideIntInt()
    {
        assertEqual(5, 10.divideInt(2), "divideInt 10/2 -> 5");
        assertEqual(3, 10.divideInt(3), "divideInt 10/3 -> 3");
        assertEqual(0, 5.divideInt(10), "divideInt 5/10 -> 0");
        assertEqual(0, 10.divideInt(0), "divideInt 10/0 默认 -> 0");
        assertEqual(99, 10.divideInt(0, 99), "divideInt 10/0 自定义默认 -> 99");
    }

    // ---- clampCycle ----
    static void testClampCycle()
    {
        // includeMax=true (default)
        assertEqual(5, 5.clampCycle(1, 10, 1), "clampCycle 5 [1,10] -> 5");
        assertEqual(1, 0.clampCycle(1, 10, 1), "clampCycle 0 [1,10] -> 1 (加1)");
        assertEqual(9, 11.clampCycle(1, 10, 2), "clampCycle 11 [1,10] cycle2 -> 9 (减2)");
        assertEqual(2, (-1).clampCycle(1, 10, 3), "clampCycle -1 [1,10] cycle3 -> 2");
        // includeMax=false
        assertEqual(8, 10.clampCycle(1, 10, 2, false), "clampCycle 10 [1,10) cycle2 -> 8");
        assertEqual(9, 9.clampCycle(1, 10, 1, false), "clampCycle 9 [1,10) -> 9 (9<10)");
        assertEqual(8, 12.clampCycle(1, 10, 2, false), "clampCycle 12 [1,10) cycle2 -> 8");
    }

    // ---- inRange(int, int) ----
    static void testInRangeInt()
    {
        assertTrue(5.inRange(1, 10), "5 in [1,10]");
        assertTrue(1.inRange(1, 10), "1 in [1,10] (边界)");
        assertTrue(10.inRange(1, 10), "10 in [1,10] (边界)");
        assertFalse(0.inRange(1, 10), "0 not in [1,10]");
        assertFalse(11.inRange(1, 10), "11 not in [1,10]");
        // 反向范围
        assertTrue(5.inRange(10, 1), "5 in [1,10] 反向参数");
    }

    // ---- inRange(int, float) ----
    static void testInRangeFloat()
    {
        assertTrue(5.inRange(1.5f, 10.5f), "5 in [1.5, 10.5]");
        assertFalse(1.inRange(1.5f, 10.5f), "1 not in [1.5, 10.5]");
        assertFalse(11.inRange(1.5f, 10.5f), "11 not in [1.5, 10.5]");
        // 反向
        assertTrue(5.inRange(10.5f, 1.5f), "5 in [1.5,10.5] 反向");
    }

    // ---- inRangeFixed(int, int) ----
    static void testInRangeFixedInt()
    {
        assertTrue(5.inRangeFixed(1, 10), "inRangeFixed 5 [1,10]");
        assertTrue(1.inRangeFixed(1, 10), "inRangeFixed 1 [1,10]");
        assertTrue(10.inRangeFixed(1, 10), "inRangeFixed 10 [1,10]");
        assertFalse(0.inRangeFixed(1, 10), "inRangeFixed 0 not in [1,10]");
        assertFalse(11.inRangeFixed(1, 10), "inRangeFixed 11 not in [1,10]");
    }

    // ---- inRangeFixed(int, float) ----
    static void testInRangeFixedFloat()
    {
        assertTrue(5.inRangeFixed(1.5f, 10.5f), "inRangeFixed 5 [1.5,10.5]");
        assertFalse(1.inRangeFixed(1.5f, 10.5f), "inRangeFixed 1 not in [1.5,10.5]");
    }

    // ---- generateBatchCount ----
    static void testGenerateBatchCount()
    {
        assertEqual(4, 10.generateBatchCount(3), "batch 10/3 -> 4 (3+3+3+1)");
        assertEqual(3, 9.generateBatchCount(3), "batch 9/3 -> 3 (3+3+3)");
        assertEqual(1, 1.generateBatchCount(3), "batch 1/3 -> 1");
        assertEqual(2, 3.generateBatchCount(2), "batch 3/2 -> 2 (2+1)");
        assertEqual(1, 3.generateBatchCount(5), "batch 3/5 -> 1");
    }

    // ---- indexToX ----
    static void testIndexToX()
    {
        assertEqual(0, 0.indexToX(3), "indexToX 0 w=3 -> 0");
        assertEqual(1, 1.indexToX(3), "indexToX 1 w=3 -> 1");
        assertEqual(2, 2.indexToX(3), "indexToX 2 w=3 -> 2");
        assertEqual(0, 3.indexToX(3), "indexToX 3 w=3 -> 0");
        assertEqual(1, 4.indexToX(3), "indexToX 4 w=3 -> 1");
    }

    // ---- indexToY ----
    static void testIndexToY()
    {
        assertEqual(0, 0.indexToY(3), "indexToY 0 w=3 -> 0");
        assertEqual(0, 1.indexToY(3), "indexToY 1 w=3 -> 0");
        assertEqual(0, 2.indexToY(3), "indexToY 2 w=3 -> 0");
        assertEqual(1, 3.indexToY(3), "indexToY 3 w=3 -> 1");
        assertEqual(1, 5.indexToY(3), "indexToY 5 w=3 -> 1");
    }

    // ---- indexToIntPos ----
    static void testIndexToIntPos()
    {
        assertEqual(new Vector2Int(0, 0), 0.indexToIntPos(3), "pos 0 w=3 -> (0,0)");
        assertEqual(new Vector2Int(1, 0), 1.indexToIntPos(3), "pos 1 w=3 -> (1,0)");
        assertEqual(new Vector2Int(2, 0), 2.indexToIntPos(3), "pos 2 w=3 -> (2,0)");
        assertEqual(new Vector2Int(0, 1), 3.indexToIntPos(3), "pos 3 w=3 -> (0,1)");
    }

    // ---- hasMask ----
    static void testHasMask()
    {
        assertTrue(0b1010.hasMask(0b1000), "1010 has 1000");
        assertTrue(0b1010.hasMask(0b0010), "1010 has 0010");
        assertTrue(0b1010.hasMask(0b1010), "1010 has 1010");
        assertFalse(0b1010.hasMask(0b0101), "1010 no 0101");
        assertFalse(0b1010.hasMask(0b0100), "1010 no 0100");
        assertFalse(0.hasMask(1), "0 no 1");
    }

    // ---- inversePow10 ----
    static void testInversePow10()
    {
        assertTrue(0.inversePow10().isEqual(1.0f), "inversePow10 0 -> 1");
        assertTrue(1.inversePow10().isEqual(0.1f), "inversePow10 1 -> 0.1");
        assertTrue(2.inversePow10().isEqual(0.01f), "inversePow10 2 -> 0.01");
        assertTrue(3.inversePow10().isEqual(0.001f), "inversePow10 3 -> 0.001");
    }

    // ---- pow10 ----
    static void testPow10()
    {
        assertEqual(1, 0.pow10(), "pow10 0 -> 1");
        assertEqual(10, 1.pow10(), "pow10 1 -> 10");
        assertEqual(100, 2.pow10(), "pow10 2 -> 100");
        assertEqual(1000, 3.pow10(), "pow10 3 -> 1000");
    }

    // ---- inversePow10Long ----
    static void testInversePow10Long()
    {
        assertTrue(0.inversePow10Long().isEqual(1.0), "inversePow10Long 0 -> 1");
        assertTrue(1.inversePow10Long().isEqual(0.1), "inversePow10Long 1 -> 0.1");
    }

    // ---- pow10Long ----
    static void testPow10Long()
    {
        assertEqual(1L, 0.pow10Long(), "pow10Long 0 -> 1");
        assertEqual(10L, 1.pow10Long(), "pow10Long 1 -> 10");
        assertEqual(100L, 2.pow10Long(), "pow10Long 2 -> 100");
    }

    // ---- pow2 ----
    static void testPow2()
    {
        assertTrue(0.pow2().isEqual(1.0f), "pow2 0 -> 1");
        assertTrue(1.pow2().isEqual(2.0f), "pow2 1 -> 2");
        assertTrue(2.pow2().isEqual(4.0f), "pow2 2 -> 4");
        assertTrue(3.pow2().isEqual(8.0f), "pow2 3 -> 8");
        assertTrue(4.pow2().isEqual(16.0f), "pow2 4 -> 16");
    }

    // ---- isPow2: value & (value-1) == 0 ----
    static void testIsPow2()
    {
        assertTrue(1.isPow2(), "1 isPow2");
        assertTrue(2.isPow2(), "2 isPow2");
        assertTrue(4.isPow2(), "4 isPow2");
        assertTrue(16.isPow2(), "16 isPow2");
        assertTrue(1024.isPow2(), "1024 isPow2");
        // 2^30
        assertTrue((1 << 30).isPow2(), "2^30 isPow2");
        // 负数位判定: 负数按位与会产生接近自身的负数, 不满足 (v & (v-1))==0
        assertFalse((-1).isPow2(), "-1 非");
        assertFalse((-2).isPow2(), "-2 非");
        assertFalse(3.isPow2(), "3 非");
        assertFalse(6.isPow2(), "6 非");
        assertFalse(15.isPow2(), "15 非");
        assertFalse(1023.isPow2(), "1023 非");
        // 边界: 按位公式 (0 & -1)==0 成立, 故 0 被判定为 true(记录此行为)
        assertTrue(0.isPow2(), "0 按位公式判定为 true(文档化当前行为)");
    }

    // ---- isEven: value & 1 == 0 ----
    static void testIsEven()
    {
        assertTrue(0.isEven(), "0 偶数");
        assertTrue(2.isEven(), "2 偶数");
        assertTrue(100.isEven(), "100 偶数");
        assertTrue((-4).isEven(), "-4 偶数");
        assertFalse(1.isEven(), "1 奇数");
        assertFalse(3.isEven(), "3 奇数");
        assertFalse(101.isEven(), "101 奇数");
        assertFalse((-3).isEven(), "-3 奇数");
    }

    // ---- getGreaterPowValue ----
    static void testGetGreaterPowValue()
    {
        assertEqual(1, 0.getGreaterPowValue(2), "greaterPow 0 pow2 -> 1");
        assertEqual(1, 1.getGreaterPowValue(2), "greaterPow 1 pow2 -> 1");
        assertEqual(2, 2.getGreaterPowValue(2), "greaterPow 2 pow2 -> 2");
        assertEqual(4, 3.getGreaterPowValue(2), "greaterPow 3 pow2 -> 4");
        assertEqual(8, 5.getGreaterPowValue(2), "greaterPow 5 pow2 -> 8");
        // pow3
        assertEqual(1, 0.getGreaterPowValue(3), "greaterPow 0 pow3 -> 1");
        assertEqual(3, 2.getGreaterPowValue(3), "greaterPow 2 pow3 -> 3");
        assertEqual(9, 4.getGreaterPowValue(3), "greaterPow 4 pow3 -> 9");
    }

    // ---- getGreaterPow2 ----
    static void testGetGreaterPow2()
    {
        assertEqual(2, 0.getGreaterPow2(), "greaterPow2 0 -> 2");
        assertEqual(2, 1.getGreaterPow2(), "greaterPow2 1 -> 2");
        assertEqual(2, 2.getGreaterPow2(), "greaterPow2 2 -> 2");
        assertEqual(4, 3.getGreaterPow2(), "greaterPow2 3 -> 4");
        assertEqual(4, 4.getGreaterPow2(), "greaterPow2 4 -> 4");
        assertEqual(8, 5.getGreaterPow2(), "greaterPow2 5 -> 8");
        assertEqual(256, 200.getGreaterPow2(), "greaterPow2 200 -> 256");
        assertEqual(256, 256.getGreaterPow2(), "greaterPow2 256 -> 256");
        assertEqual(512, 300.getGreaterPow2(), "greaterPow2 300 -> 512");
        assertEqual(512, 512.getGreaterPow2(), "greaterPow2 512 -> 512");
        // 大于512的值走顺序查找
        assertEqual(1024, 600.getGreaterPow2(), "greaterPow2 600 -> 1024");
        assertEqual(1024, 1024.getGreaterPow2(), "greaterPow2 1024 -> 1024");
        assertEqual(32768, 20000.getGreaterPow2(), "greaterPow2 20000 -> 32768");
        // 更大值
        assertEqual(65536, 40000.getGreaterPow2(), "greaterPow2 40000 -> 65536");
    }

    // ---- sbyte.abs ----
    static void testSByteAbs()
    {
        sbyte v = 5;
        assertEqual((sbyte)5, v.abs(), "sbyte abs 5 -> 5");
        v = -5;
        assertEqual((sbyte)5, v.abs(), "sbyte abs -5 -> 5");
        v = 0;
        assertEqual((sbyte)0, v.abs(), "sbyte abs 0 -> 0");
    }

    // ---- short.abs ----
    static void testShortAbs()
    {
        short v = 5;
        assertEqual((short)5, v.abs(), "short abs 5 -> 5");
        v = -5;
        assertEqual((short)5, v.abs(), "short abs -5 -> 5");
        v = 0;
        assertEqual((short)0, v.abs(), "short abs 0 -> 0");
    }

    // ---- byte.clampMin ----
    static void testByteClampMin()
    {
        byte v = 5;
        assertEqual((byte)5, v.clampMin((byte)3), "byte clampMin 5 min=3 -> 5");
        assertEqual((byte)3, ((byte)2).clampMin((byte)3), "byte clampMin 2 min=3 -> 3");
        assertEqual((byte)5, v.clampMin(), "byte clampMin 5 default -> 5");
        assertEqual((byte)0, ((byte)0).clampMin(), "byte clampMin 0 default -> 0");
    }

    // ---- sbyte.clampMin ----
    static void testSByteClampMin()
    {
        sbyte v = 5;
        assertEqual((sbyte)5, v.clampMin((sbyte)3), "sbyte clampMin 5 min=3 -> 5");
        assertEqual((sbyte)3, ((sbyte)2).clampMin((sbyte)3), "sbyte clampMin 2 min=3 -> 3");
        assertEqual((sbyte)5, v.clampMin(), "sbyte clampMin 5 default -> 5");
        assertEqual((sbyte)0, ((sbyte)(-1)).clampMin(), "sbyte clampMin -1 default -> 0");
    }

    // ---- short.clampMin ----
    static void testShortClampMin()
    {
        short v = 5;
        assertEqual((short)5, v.clampMin((short)3), "short clampMin 5 min=3 -> 5");
        assertEqual((short)3, ((short)2).clampMin((short)3), "short clampMin 2 min=3 -> 3");
        assertEqual((short)0, ((short)(-1)).clampMin(), "short clampMin -1 default -> 0");
    }

    // ---- ushort.clampMin ----
    static void testUShortClampMin()
    {
        ushort v = 5;
        assertEqual((ushort)5, v.clampMin((ushort)3), "ushort clampMin 5 min=3 -> 5");
        assertEqual((ushort)3, ((ushort)2).clampMin((ushort)3), "ushort clampMin 2 min=3 -> 3");
        assertEqual((ushort)5, v.clampMin(), "ushort clampMin 5 default -> 5");
    }

    // ---- uint.clampMin ----
    static void testUIntClampMin()
    {
        uint v = 5;
        assertEqual(5u, v.clampMin(3u), "uint clampMin 5 min=3 -> 5");
        assertEqual(3u, 2u.clampMin(3u), "uint clampMin 2 min=3 -> 3");
        assertEqual(5u, v.clampMin(), "uint clampMin 5 default -> 5");
    }

    // ---- byte.clampMax ----
    static void testByteClampMax()
    {
        assertEqual((byte)5, ((byte)5).clampMax((byte)10), "byte clampMax 5 max=10 -> 5");
        assertEqual((byte)10, ((byte)15).clampMax((byte)10), "byte clampMax 15 max=10 -> 10");
    }

    // ---- sbyte.clampMax ----
    static void testSByteClampMax()
    {
        assertEqual((sbyte)5, ((sbyte)5).clampMax((sbyte)10), "sbyte clampMax 5 max=10 -> 5");
        assertEqual((sbyte)10, ((sbyte)15).clampMax((sbyte)10), "sbyte clampMax 15 max=10 -> 10");
    }

    // ---- short.clampMax ----
    static void testShortClampMax()
    {
        assertEqual((short)5, ((short)5).clampMax((short)10), "short clampMax 5 max=10 -> 5");
        assertEqual((short)10, ((short)15).clampMax((short)10), "short clampMax 15 max=10 -> 10");
    }

    // ---- ushort.clampMax ----
    static void testUShortClampMax()
    {
        assertEqual((ushort)5, ((ushort)5).clampMax((ushort)10), "ushort clampMax 5 max=10 -> 5");
        assertEqual((ushort)10, ((ushort)15).clampMax((ushort)10), "ushort clampMax 15 max=10 -> 10");
    }

    // ---- uint.clampMax ----
    static void testUIntClampMax()
    {
        assertEqual(5u, 5u.clampMax(10u), "uint clampMax 5 max=10 -> 5");
        assertEqual(10u, 15u.clampMax(10u), "uint clampMax 15 max=10 -> 10");
    }
}
