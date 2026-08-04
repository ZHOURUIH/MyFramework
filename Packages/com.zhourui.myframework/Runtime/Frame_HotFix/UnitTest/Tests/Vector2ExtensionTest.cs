using UnityEngine;
using static TestAssert;

// Vector2Extension 纯数学扩展方法测试
public static class Vector2ExtensionTest
{
    public static void Run()
    {
        testCeil();
        testIsNaN();
        testRound();
        testFloor();
        testAbs();
        testResetX();
        testResetY();
        testReplaceX();
        testReplaceY();
        testReplaceZ();
        testIsZero();
        testGetLength();
        testGetSquaredLength();
        testLengthLess();
        testLengthLessEqual();
        testLengthGreater();
        testLengthGreaterEqual();
        testSetLength();
        testIsLess();
        testIsGreater();
        testIsEqual();
        testNormalize();
        testClampMinMax();
        testInRange();
        testMulti();
        testDivide();
        testGetAngle();
        testDot();
    }

    // ---- ceil ----
    static void testCeil()
    {
        Vector2 v = new Vector2(3.2f, -1.2f).ceil();
        assertTrue(v.x.isEqual(4.0f), "ceil x 3.2 -> 4");
        assertTrue(v.y.isEqual(-1.0f), "ceil y -1.2 -> -1");
    }

    // ---- isNaN ----
    static void testIsNaN()
    {
        assertFalse(new Vector2(1.0f, 2.0f).isNaN(), "not NaN");
        assertTrue(new Vector2(float.NaN, 2.0f).isNaN(), "x NaN -> true");
        assertTrue(new Vector2(1.0f, float.NaN).isNaN(), "y NaN -> true");
    }

    // ---- round (returns Vector3 per source) ----
    static void testRound()
    {
        Vector3 v = new Vector2(3.4f, -2.6f).round();
        assertTrue(v.x.isEqual(3.0f), "round x 3.4 -> 3");
        assertTrue(v.y.isEqual(-3.0f), "round y -2.6 -> -3");
    }

    // ---- floor (returns Vector3 per source) ----
    static void testFloor()
    {
        Vector3 v = new Vector2(3.8f, -2.2f).floor();
        assertTrue(v.x.isEqual(3.0f), "floor x 3.8 -> 3");
        assertTrue(v.y.isEqual(-3.0f), "floor y -2.2 -> -3");
    }

    // ---- abs (returns Vector3 per source) ----
    static void testAbs()
    {
        Vector3 v = new Vector2(-3.0f, 4.0f).abs();
        assertTrue(v.x.isEqual(3.0f), "abs x -3 -> 3");
        assertTrue(v.y.isEqual(4.0f), "abs y 4 -> 4");
    }

    // ---- resetX/resetY ----
    static void testResetX()
    {
        Vector2 v = new Vector2(5.0f, 3.0f).resetX();
        assertTrue(v.x.isZero(), "resetX x=0");
        assertTrue(v.y.isEqual(3.0f), "resetX y stays");
    }

    static void testResetY()
    {
        Vector2 v = new Vector2(5.0f, 3.0f).resetY();
        assertTrue(v.x.isEqual(5.0f), "resetY x stays");
        assertTrue(v.y.isZero(), "resetY y=0");
    }

    // ---- replaceX/replaceY ----
    static void testReplaceX()
    {
        Vector2 v = new Vector2(5.0f, 3.0f).replaceX(10.0f);
        assertTrue(v.x.isEqual(10.0f), "replaceX x=10");
        assertTrue(v.y.isEqual(3.0f), "replaceX y stays");
    }

    static void testReplaceY()
    {
        Vector2 v = new Vector2(5.0f, 3.0f).replaceY(10.0f);
        assertTrue(v.x.isEqual(5.0f), "replaceY x stays");
        assertTrue(v.y.isEqual(10.0f), "replaceY y=10");
    }

    // ---- replaceZ ----
    static void testReplaceZ()
    {
        Vector3 v = new Vector2(5.0f, 3.0f).replaceZ(7.0f);
        assertTrue(v.x.isEqual(5.0f), "replaceZ x stays");
        assertTrue(v.y.isEqual(3.0f), "replaceZ y stays");
        assertTrue(v.z.isEqual(7.0f), "replaceZ z=7");
    }

    // ---- isZero ----
    static void testIsZero()
    {
        assertTrue(Vector2.zero.isZero(), "zero is zero");
        assertFalse(new Vector2(1.0f, 0.0f).isZero(), "not zero");
    }

    // ---- getLength/getSquaredLength ----
    static void testGetLength()
    {
        assertTrue(new Vector2(3.0f, 4.0f).getLength().isEqual(5.0f), "|3,4| = 5");
        assertTrue(Vector2.zero.getLength().isZero(), "|0| = 0");
    }

    static void testGetSquaredLength()
    {
        assertTrue(new Vector2(3.0f, 4.0f).getSquaredLength().isEqual(25.0f), "sqlen 3,4 = 25");
    }

    // ---- lengthLess ----
    static void testLengthLess()
    {
        assertTrue(new Vector2(3.0f, 4.0f).lengthLess(6.0f), "5 < 6");
        assertFalse(new Vector2(3.0f, 4.0f).lengthLess(4.0f), "5 < 4 false");
        assertTrue(new Vector2(1.0f, 1.0f).lengthLess(new Vector2(3.0f, 3.0f)), "vec less");
    }

    // ---- lengthLessEqual ----
    static void testLengthLessEqual()
    {
        assertTrue(new Vector2(3.0f, 4.0f).lengthLessEqual(5.0f), "5 <= 5");
        assertTrue(new Vector2(3.0f, 4.0f).lengthLessEqual(6.0f), "5 <= 6");
        assertFalse(new Vector2(3.0f, 4.0f).lengthLessEqual(4.0f), "5 <= 4 false");
    }

    // ---- lengthGreater ----
    static void testLengthGreater()
    {
        assertTrue(new Vector2(3.0f, 4.0f).lengthGreater(4.0f), "5 > 4");
        assertFalse(new Vector2(3.0f, 4.0f).lengthGreater(6.0f), "5 > 6 false");
        assertTrue(new Vector2(3.0f, 3.0f).lengthGreater(new Vector2(1.0f, 1.0f)), "vec greater");
    }

    // ---- lengthGreaterEqual ----
    static void testLengthGreaterEqual()
    {
        assertTrue(new Vector2(3.0f, 4.0f).lengthGreaterEqual(5.0f), "5 >= 5");
        assertTrue(new Vector2(3.0f, 4.0f).lengthGreaterEqual(4.0f), "5 >= 4");
        assertFalse(new Vector2(3.0f, 4.0f).lengthGreaterEqual(6.0f), "5 >= 6 false");
    }

    // ---- setLength ----
    static void testSetLength()
    {
        Vector2 v = new Vector2(3.0f, 4.0f).setLength(10.0f);
        assertTrue(v.getLength().isEqual(10.0f, 0.001f), "setLength 5->10");
        // 方向不变
        assertTrue(v.x.isEqual(6.0f, 0.001f), "setLength x 3->6");
        assertTrue(v.y.isEqual(8.0f, 0.001f), "setLength y 4->8");
    }

    // ---- isLess/isGreater ----
    static void testIsLess()
    {
        assertTrue(new Vector2(1.0f, 2.0f).isLess(new Vector2(3.0f, 4.0f)), "all less");
        assertFalse(new Vector2(1.0f, 5.0f).isLess(new Vector2(3.0f, 4.0f)), "y not less");
    }

    static void testIsGreater()
    {
        assertTrue(new Vector2(5.0f, 6.0f).isGreater(new Vector2(3.0f, 4.0f)), "all greater");
        assertFalse(new Vector2(5.0f, 2.0f).isGreater(new Vector2(3.0f, 4.0f)), "y not greater");
    }

    // ---- isEqual ----
    static void testIsEqual()
    {
        assertTrue(new Vector2(1.0f, 2.0f).isEqual(new Vector2(1.0f, 2.0f)), "equal");
        assertFalse(new Vector2(1.0f, 2.0f).isEqual(new Vector2(1.0f, 3.0f)), "not equal");
        assertTrue(new Vector2(1.00001f, 2.0f).isEqual(new Vector2(1.0f, 2.0f)), "approx equal");
    }

    // ---- normalize ----
    static void testNormalize()
    {
        Vector2 v = new Vector2(3.0f, 4.0f).normalize();
        assertTrue(v.getLength().isEqual(1.0f, 0.001f), "normalize length=1");
    }

    // ---- clampMin/clampMax ----
    static void testClampMinMax()
    {
        Vector2 v = new Vector2(-1.0f, 5.0f).clampMin(0.0f);
        assertTrue(v.x.isZero(), "clampMin x -1->0");
        assertTrue(v.y.isEqual(5.0f), "clampMin y 5 stays");

        Vector2 v2 = new Vector2(7.0f, 3.0f).clampMax(5.0f);
        assertTrue(v2.x.isEqual(5.0f), "clampMax x 7->5");
        assertTrue(v2.y.isEqual(3.0f), "clampMax y 3 stays");
    }

    // ---- inRange ----
    static void testInRange()
    {
        Vector2 min = new Vector2(0.0f, 0.0f);
        Vector2 max = new Vector2(10.0f, 10.0f);
        assertTrue(new Vector2(5.0f, 5.0f).inRange(min, max), "in range");
        assertFalse(new Vector2(15.0f, 5.0f).inRange(min, max), "x out of range");
        assertFalse(new Vector2(5.0f, -5.0f).inRange(min, max), "y out of range");
    }

    // ---- multi ----
    static void testMulti()
    {
        Vector2 v = new Vector2(2.0f, 3.0f).multi(new Vector2(4.0f, 5.0f));
        assertTrue(v.x.isEqual(8.0f), "multi x 2*4=8");
        assertTrue(v.y.isEqual(15.0f), "multi y 3*5=15");
    }

    // ---- divide ----
    static void testDivide()
    {
        Vector2 v = new Vector2(8.0f, 15.0f).divide(new Vector2(2.0f, 3.0f));
        assertTrue(v.x.isEqual(4.0f), "divide x 8/2=4");
        assertTrue(v.y.isEqual(5.0f), "divide y 15/3=5");

        Vector2 v2 = new Vector2(8.0f, 15.0f).divide(2.0f);
        assertTrue(v2.x.isEqual(4.0f), "divide scalar x 8/2=4");
        assertTrue(v2.y.isEqual(7.5f), "divide scalar y 15/2=7.5");
    }

    // ---- getAngle ----
    static void testGetAngle()
    {
        // 指向 Z 轴正方向 (x=0, y=1 in Vector2 → x=0, z=1 in Vector3)
        float angle = new Vector2(0.0f, 1.0f).getAngle();
        assertTrue(angle.isZero(0.01f), "forward -> angle 0");

        // 指向 X 轴正方向 (x=1, y=0), getAngle 内部取反后返回 pi/2
        float angle90 = new Vector2(1.0f, 0.0f).getAngle();
        assertTrue(angle90.isEqual(Mathf.PI * 0.5f, 0.01f), "right -> pi/2");
    }

    // ---- dot ----
    static void testDot()
    {
        float d = new Vector2(1.0f, 2.0f).dot(new Vector2(3.0f, 4.0f));
        assertTrue(d.isEqual(11.0f), "dot 1*3+2*4=11");

        float dOrth = new Vector2(1.0f, 0.0f).dot(new Vector2(0.0f, 1.0f));
        assertTrue(dOrth.isZero(), "orthogonal dot=0");
    }
}
