using UnityEngine;
using static TestAssert;

// Vector4Extension 纯数学扩展方法测试
public static class Vector4ExtensionTest
{
    public static void Run()
    {
        testAbs();
        testGetLength();
        testGetSquaredLength();
        testLengthLess();
        testClampMin();
        testClampMax();
    }

    // ---- abs ----
    static void testAbs()
    {
        Vector4 v = new Vector4(1, -2, 3, -4).abs();
        assertTrue(v.x.isEqual(1, 0.001f) && v.y.isEqual(2, 0.001f) &&
                   v.z.isEqual(3, 0.001f) && v.w.isEqual(4, 0.001f), "abs V4 mixed");
        Vector4 v2 = new Vector4(-1, -2, -3, -4).abs();
        assertTrue(v2.x.isEqual(1, 0.001f) && v2.y.isEqual(2, 0.001f) &&
                   v2.z.isEqual(3, 0.001f) && v2.w.isEqual(4, 0.001f), "abs V4 all negative");
        Vector4 v3 = new Vector4(0, 0, 0, 0).abs();
        assertTrue(v3.x.isZero() && v3.y.isZero() && v3.z.isZero() && v3.w.isZero(), "abs V4 zero");
    }

    // ---- getLength ----
    static void testGetLength()
    {
        // 1^2 + 2^2 + 2^2 + 4^2 = 1+4+4+16 = 25, sqrt=5
        Vector4 v = new Vector4(1, 2, 2, 4);
        assertTrue(v.getLength().isEqual(5.0f, 0.001f), "getLength 1-2-2-4");
        Vector4 zero = new Vector4(0, 0, 0, 0);
        assertTrue(zero.getLength().isZero(), "getLength zero");
        // 单位向量
        Vector4 unit = new Vector4(1, 0, 0, 0);
        assertTrue(unit.getLength().isEqual(1.0f, 0.001f), "getLength unit");
    }

    // ---- getSquaredLength ----
    static void testGetSquaredLength()
    {
        // 1^2 + 2^2 + 2^2 + 4^2 = 25
        Vector4 v = new Vector4(1, 2, 2, 4);
        assertTrue(v.getSquaredLength().isEqual(25.0f, 0.001f), "getSquaredLength 1-2-2-4");
        Vector4 zero = new Vector4(0, 0, 0, 0);
        assertTrue(zero.getSquaredLength().isZero(), "getSquaredLength zero");
        Vector4 neg = new Vector4(-1, -1, -1, -1);
        assertTrue(neg.getSquaredLength().isEqual(4.0f, 0.001f), "getSquaredLength negative");
    }

    // ---- lengthLess ----
    static void testLengthLess()
    {
        // 1^2 + 1^2 + 1^2 + 1^2 = 4, sqrt=2, 2 < 3
        Vector4 v = new Vector4(1, 1, 1, 1);
        assertTrue(v.lengthLess(3.0f), "lengthLess true");
        assertFalse(v.lengthLess(1.0f), "lengthLess false");
        assertFalse(v.lengthLess(2.0f), "lengthLess equal (strict <)");
        Vector4 zero = new Vector4(0, 0, 0, 0);
        assertTrue(zero.lengthLess(0.1f), "lengthLess zero");
    }

    // ---- clampMin ----
    static void testClampMin()
    {
        // clampMin with default min=0
        Vector4 v = new Vector4(-1, 2, -3, 4).clampMin();
        assertTrue(v.x.isZero() && v.y.isEqual(2, 0.001f) &&
                   v.z.isZero() && v.w.isEqual(4, 0.001f), "clampMin default 0");
        // clampMin with custom min
        Vector4 v2 = new Vector4(1, 5, 3, 2).clampMin(3.0f);
        assertTrue(v2.x.isEqual(3, 0.001f) && v2.y.isEqual(5, 0.001f) &&
                   v2.z.isEqual(3, 0.001f) && v2.w.isEqual(3, 0.001f), "clampMin custom 3");
        // no clamp
        Vector4 v3 = new Vector4(5, 6, 7, 8).clampMin(3.0f);
        assertTrue(v3.x.isEqual(5, 0.001f) && v3.y.isEqual(6, 0.001f) &&
                   v3.z.isEqual(7, 0.001f) && v3.w.isEqual(8, 0.001f), "clampMin no change");
    }

    // ---- clampMax ----
    static void testClampMax()
    {
        // NOTE: Vector4.clampMax 参数名为 min, 实际语义是 clampMax
        Vector4 v = new Vector4(5, 3, 7, 1).clampMax(4.0f);
        assertTrue(v.x.isEqual(4, 0.001f) && v.y.isEqual(3, 0.001f) &&
                   v.z.isEqual(4, 0.001f) && v.w.isEqual(1, 0.001f), "clampMax 4");
        // no clamp
        Vector4 v2 = new Vector4(1, 2, 3, 4).clampMax(5.0f);
        assertTrue(v2.x.isEqual(1, 0.001f) && v2.y.isEqual(2, 0.001f) &&
                   v2.z.isEqual(3, 0.001f) && v2.w.isEqual(4, 0.001f), "clampMax no change");
    }
}
