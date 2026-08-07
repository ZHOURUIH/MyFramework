using UnityEngine;
using static TestAssert;

// Vector3Extension 纯数学扩展方法测试
public static class Vector3ExtensionTest
{
    public static void Run()
    {
        testCeil();
        testIsNaN();
        testSaturate();
        testRound();
        testFloor();
        testAbs();
        testCheckFloat();
        testCheckInt();
        testClampLength();
        testResetX();
        testResetY();
        testResetZ();
        testReplaceX();
        testReplaceY();
        testReplaceZ();
        testIsZero();
        testGetLength();
        testGetLengthIgnoreY();
        testGetSquaredLength();
        testGetSquaredLengthIgnoreY();
        testLengthLess();
        testLengthLessEqual();
        testLengthGreater();
        testLengthGreaterEqual();
        testLengthLessIgnoreY();
        testLengthLessEqualIgnoreY();
        testLengthGreaterIgnoreY();
        testLengthGreaterEqualIgnoreY();
        testSetLength();
        testIsLess();
        testIsGreater();
        testIsEqual();
        testNormalize();
        testToRadianToDegree();
        testClampMinMax();
        testInRange();
        testMulti();
        testDivide();
        testAdjustRadian180();
        testAdjustAngle180();
        testAdjustRadian360();
        testAdjustAngle360();
        testGetAngle();
        testRotate();
        testDot();
        testCross();
    }

    // ---- ceil ----
    static void testCeil()
    {
        Vector3 v = new Vector3(3.2f, -1.2f, 5.8f).ceil();
        assertTrue(v.x.isEqual(4.0f), "ceil x 3.2 -> 4");
        assertTrue(v.y.isEqual(-1.0f), "ceil y -1.2 -> -1");
        assertTrue(v.z.isEqual(6.0f), "ceil z 5.8 -> 6");
    }

    // ---- isNaN ----
    static void testIsNaN()
    {
        assertFalse(new Vector3(1.0f, 2.0f, 3.0f).isNaN(), "not NaN");
        assertTrue(new Vector3(float.NaN, 2.0f, 3.0f).isNaN(), "x NaN -> true");
        assertTrue(new Vector3(1.0f, float.NaN, 3.0f).isNaN(), "y NaN -> true");
        assertTrue(new Vector3(1.0f, 2.0f, float.NaN).isNaN(), "z NaN -> true");
    }

    // ---- saturate ----
    static void testSaturate()
    {
        Vector3 v = new Vector3(0.5f, 1.5f, -0.5f).saturate();
        assertTrue(v.x.isEqual(0.5f), "saturate x 0.5");
        assertTrue(v.y.isEqual(1.0f), "saturate y 1.5->1");
        assertTrue(v.z.isZero(), "saturate z -0.5->0");
    }

    // ---- round ----
    static void testRound()
    {
        Vector3 v = new Vector3(3.4f, -2.6f, 0.5f).round();
        assertTrue(v.x.isEqual(3.0f), "round x 3.4->3");
        assertTrue(v.y.isEqual(-3.0f), "round y -2.6->-3");
        assertTrue(v.z.isEqual(1.0f), "round z 0.5->1");
    }

    // ---- floor ----
    static void testFloor()
    {
        Vector3 v = new Vector3(3.8f, -2.2f, 0.1f).floor();
        assertTrue(v.x.isEqual(3.0f), "floor x 3.8->3");
        assertTrue(v.y.isEqual(-3.0f), "floor y -2.2->-3");
        assertTrue(v.z.isZero(), "floor z 0.1->0");
    }

    // ---- abs ----
    static void testAbs()
    {
        Vector3 v = new Vector3(-3.0f, 4.0f, -5.0f).abs();
        assertTrue(v.x.isEqual(3.0f), "abs x -3->3");
        assertTrue(v.y.isEqual(4.0f), "abs y 4->4");
        assertTrue(v.z.isEqual(5.0f), "abs z -5->5");
    }

    // ---- checkFloat/checkInt ----
    static void testCheckFloat()
    {
        Vector3 v = new Vector3(3.14159f, 2.71828f, 1.41421f).checkFloat(2);
        assertTrue(v.x.isEqual(3.14f, 0.01f), "checkFloat x ~3.14");
        assertTrue(v.y.isEqual(2.72f, 0.01f), "checkFloat y ~2.72");
        assertTrue(v.z.isEqual(1.41f, 0.01f), "checkFloat z ~1.41");
    }

    static void testCheckInt()
    {
        Vector3 v = new Vector3(1.0000001f, 0.9999999f, 3.5f).checkInt();
        assertTrue(v.x.isEqual(1.0f), "checkInt x -> 1");
        assertTrue(v.y.isEqual(1.0f), "checkInt y -> 1");
        assertTrue(v.z.isEqual(3.5f), "checkInt z stays 3.5");
    }

    // ---- clampLength ----
    static void testClampLength()
    {
        Vector3 v = new Vector3(3.0f, 4.0f, 0.0f).clampLength(2.0f);
        assertTrue(v.getLength().isEqual(2.0f, 0.001f), "clampLength 5->2");

        Vector3 v2 = new Vector3(1.0f, 0.0f, 0.0f).clampLength(5.0f);
        assertTrue(v2.getLength().isEqual(1.0f, 0.001f), "clampLength 1 stays (no clamp)");
    }

    // ---- reset ----
    static void testResetX()
    {
        Vector3 v = new Vector3(5.0f, 3.0f, 7.0f).resetX();
        assertTrue(v.x.isZero(), "resetX x=0");
        assertTrue(v.y.isEqual(3.0f), "resetX y stays");
        assertTrue(v.z.isEqual(7.0f), "resetX z stays");
    }

    static void testResetY()
    {
        Vector3 v = new Vector3(5.0f, 3.0f, 7.0f).resetY();
        assertTrue(v.x.isEqual(5.0f), "resetY x stays");
        assertTrue(v.y.isZero(), "resetY y=0");
        assertTrue(v.z.isEqual(7.0f), "resetY z stays");
    }

    static void testResetZ()
    {
        Vector3 v = new Vector3(5.0f, 3.0f, 7.0f).resetZ();
        assertTrue(v.x.isEqual(5.0f), "resetZ x stays");
        assertTrue(v.y.isEqual(3.0f), "resetZ y stays");
        assertTrue(v.z.isZero(), "resetZ z=0");
    }

    // ---- replace ----
    static void testReplaceX()
    {
        Vector3 v = new Vector3(5.0f, 3.0f, 7.0f).replaceX(10.0f);
        assertTrue(v.x.isEqual(10.0f), "replaceX x=10");
        assertTrue(v.y.isEqual(3.0f), "replaceX y stays");
        assertTrue(v.z.isEqual(7.0f), "replaceX z stays");
    }

    static void testReplaceY()
    {
        Vector3 v = new Vector3(5.0f, 3.0f, 7.0f).replaceY(10.0f);
        assertTrue(v.x.isEqual(5.0f), "replaceY x stays");
        assertTrue(v.y.isEqual(10.0f), "replaceY y=10");
        assertTrue(v.z.isEqual(7.0f), "replaceY z stays");
    }

    static void testReplaceZ()
    {
        Vector3 v = new Vector3(5.0f, 3.0f, 7.0f).replaceZ(10.0f);
        assertTrue(v.x.isEqual(5.0f), "replaceZ x stays");
        assertTrue(v.y.isEqual(3.0f), "replaceZ y stays");
        assertTrue(v.z.isEqual(10.0f), "replaceZ z=10");
    }

    // ---- isZero ----
    static void testIsZero()
    {
        assertTrue(Vector3.zero.isZero(), "zero is zero");
        assertFalse(new Vector3(1.0f, 0.0f, 0.0f).isZero(), "not zero");
        assertFalse(new Vector3(0.0f, 1.0f, 0.0f).isZero(), "y not zero");
        assertFalse(new Vector3(0.0f, 0.0f, 1.0f).isZero(), "z not zero");
    }

    // ---- getLength/getLengthIgnoreY ----
    static void testGetLength()
    {
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).getLength().isEqual(5.0f), "|3,4,0| = 5");
        assertTrue(new Vector3(1.0f, 2.0f, 3.0f).getLength().isEqual(3.7416f, 0.001f), "|1,2,3| ~ 3.742");
        assertTrue(Vector3.zero.getLength().isZero(), "|0| = 0");
    }

    static void testGetLengthIgnoreY()
    {
        assertTrue(new Vector3(3.0f, 100.0f, 4.0f).getLengthIgnoreY().isEqual(5.0f), "|3,4|_xz = 5");
    }

    // ---- getSquaredLength ----
    static void testGetSquaredLength()
    {
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).getSquaredLength().isEqual(25.0f), "sqlen 3,4,0 = 25");
    }

    static void testGetSquaredLengthIgnoreY()
    {
        assertTrue(new Vector3(3.0f, 100.0f, 4.0f).getSquaredLengthIgnoreY().isEqual(25.0f), "sqlen_xz 3,4 = 25");
    }

    // ---- lengthLess ----
    static void testLengthLess()
    {
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).lengthLess(6.0f), "5 < 6");
        assertFalse(new Vector3(3.0f, 4.0f, 0.0f).lengthLess(4.0f), "5 < 4 false");
        assertTrue(new Vector3(1.0f, 1.0f, 1.0f).lengthLess(new Vector3(3.0f, 3.0f, 3.0f)), "vec less");
    }

    // ---- lengthLessEqual ----
    static void testLengthLessEqual()
    {
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).lengthLessEqual(5.0f), "5 <= 5");
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).lengthLessEqual(6.0f), "5 <= 6");
        assertFalse(new Vector3(3.0f, 4.0f, 0.0f).lengthLessEqual(4.0f), "5 <= 4 false");
    }

    // ---- lengthGreater ----
    static void testLengthGreater()
    {
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).lengthGreater(4.0f), "5 > 4");
        assertFalse(new Vector3(3.0f, 4.0f, 0.0f).lengthGreater(6.0f), "5 > 6 false");
        assertTrue(new Vector3(3.0f, 3.0f, 3.0f).lengthGreater(new Vector3(1.0f, 1.0f, 1.0f)), "vec greater");
    }

    // ---- lengthGreaterEqual ----
    static void testLengthGreaterEqual()
    {
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).lengthGreaterEqual(5.0f), "5 >= 5");
        assertTrue(new Vector3(3.0f, 4.0f, 0.0f).lengthGreaterEqual(4.0f), "5 >= 4");
        assertFalse(new Vector3(3.0f, 4.0f, 0.0f).lengthGreaterEqual(6.0f), "5 >= 6 false");
    }

    // ---- lengthLessIgnoreY: 水平(xz)长度 < 阈值, 忽略 Y ----
    static void testLengthLessIgnoreY()
    {
        // |3,_,4|_xz = 5, 高度 100 不影响
        assertTrue(new Vector3(3.0f, 100.0f, 4.0f).lengthLessIgnoreY(6.0f), "xz=5 < 6, y 不影响");
        assertFalse(new Vector3(3.0f, -100.0f, 4.0f).lengthLessIgnoreY(4.0f), "xz=5 < 4 false");
        // 水平方向全 0
        assertTrue(new Vector3(0.0f, 999.0f, 0.0f).lengthLessIgnoreY(1.0f), "xz=0 < 1");
        // 与 getLengthIgnoreY 语义一致
        float len = new Vector3(3.0f, 0.0f, 4.0f).getLengthIgnoreY();
        assertEqual(len < 6.0f, new Vector3(3.0f, 0.0f, 4.0f).lengthLessIgnoreY(6.0f), "lengthLessIgnoreY 语义一致");
    }

    // ---- lengthLessEqualIgnoreY ----
    static void testLengthLessEqualIgnoreY()
    {
        assertTrue(new Vector3(3.0f, 50.0f, 4.0f).lengthLessEqualIgnoreY(5.0f), "xz=5 <= 5");
        assertTrue(new Vector3(3.0f, 50.0f, 4.0f).lengthLessEqualIgnoreY(6.0f), "xz=5 <= 6");
        assertFalse(new Vector3(3.0f, 50.0f, 4.0f).lengthLessEqualIgnoreY(4.0f), "xz=5 <= 4 false");
    }

    // ---- lengthGreaterIgnoreY ----
    static void testLengthGreaterIgnoreY()
    {
        assertTrue(new Vector3(3.0f, 77.0f, 4.0f).lengthGreaterIgnoreY(4.0f), "xz=5 > 4");
        assertFalse(new Vector3(3.0f, 77.0f, 4.0f).lengthGreaterIgnoreY(6.0f), "xz=5 > 6 false");
        assertFalse(new Vector3(0.0f, 77.0f, 0.0f).lengthGreaterIgnoreY(0.5f), "xz=0 > 0.5 false");
    }

    // ---- lengthGreaterEqualIgnoreY ----
    static void testLengthGreaterEqualIgnoreY()
    {
        assertTrue(new Vector3(3.0f, 20.0f, 4.0f).lengthGreaterEqualIgnoreY(5.0f), "xz=5 >= 5");
        assertTrue(new Vector3(3.0f, 20.0f, 4.0f).lengthGreaterEqualIgnoreY(4.0f), "xz=5 >= 4");
        assertFalse(new Vector3(3.0f, 20.0f, 4.0f).lengthGreaterEqualIgnoreY(6.0f), "xz=5 >= 6 false");
    }

    // ---- setLength ----
    static void testSetLength()
    {
        Vector3 v = new Vector3(3.0f, 4.0f, 0.0f).setLength(10.0f);
        assertTrue(v.getLength().isEqual(10.0f, 0.001f), "setLength 5->10");
        assertTrue(v.x.isEqual(6.0f, 0.001f), "x 3->6");
        assertTrue(v.y.isEqual(8.0f, 0.001f), "y 4->8");
    }

    // ---- isLess/isGreater ----
    static void testIsLess()
    {
        assertTrue(new Vector3(1.0f, 2.0f, 3.0f).isLess(new Vector3(4.0f, 5.0f, 6.0f)), "all less");
        assertFalse(new Vector3(1.0f, 5.0f, 3.0f).isLess(new Vector3(4.0f, 5.0f, 6.0f)), "y not less");
    }

    static void testIsGreater()
    {
        assertTrue(new Vector3(5.0f, 6.0f, 7.0f).isGreater(new Vector3(1.0f, 2.0f, 3.0f)), "all greater");
        assertFalse(new Vector3(5.0f, 2.0f, 7.0f).isGreater(new Vector3(1.0f, 2.0f, 3.0f)), "y not greater");
    }

    // ---- isEqual ----
    static void testIsEqual()
    {
        assertTrue(new Vector3(1.0f, 2.0f, 3.0f).isEqual(new Vector3(1.0f, 2.0f, 3.0f)), "equal");
        assertFalse(new Vector3(1.0f, 2.0f, 3.0f).isEqual(new Vector3(1.0f, 2.0f, 4.0f)), "not equal");
        assertTrue(new Vector3(1.00001f, 2.0f, 3.0f).isEqual(new Vector3(1.0f, 2.0f, 3.0f)), "approx equal");
    }

    // ---- normalize ----
    static void testNormalize()
    {
        Vector3 v = new Vector3(3.0f, 4.0f, 0.0f).normalize();
        assertTrue(v.getLength().isEqual(1.0f, 0.001f), "normalize length=1");
    }

    // ---- toRadian/toDegree ----
    static void testToRadianToDegree()
    {
        Vector3 v = new Vector3(180.0f, 90.0f, 0.0f).toRadian();
        assertTrue(v.x.isEqual(Mathf.PI, 0.001f), "180->pi");
        assertTrue(v.y.isEqual(Mathf.PI * 0.5f, 0.001f), "90->pi/2");

        Vector3 v2 = new Vector3(Mathf.PI, Mathf.PI * 0.5f, 0.0f).toDegree();
        assertTrue(v2.x.isEqual(180.0f, 0.01f), "pi->180");
        assertTrue(v2.y.isEqual(90.0f, 0.01f), "pi/2->90");
    }

    // ---- clampMin/clampMax ----
    static void testClampMinMax()
    {
        Vector3 v = new Vector3(-1.0f, 5.0f, -3.0f).clampMin(0.0f);
        assertTrue(v.x.isZero(), "clampMin x -1->0");
        assertTrue(v.y.isEqual(5.0f), "clampMin y stays");
        assertTrue(v.z.isZero(), "clampMin z -3->0");

        Vector3 v2 = new Vector3(7.0f, 3.0f, 9.0f).clampMax(5.0f);
        assertTrue(v2.x.isEqual(5.0f), "clampMax x 7->5");
        assertTrue(v2.y.isEqual(3.0f), "clampMax y stays");
        assertTrue(v2.z.isEqual(5.0f), "clampMax z 9->5");
    }

    // ---- inRange ----
    static void testInRange()
    {
        Vector3 min = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 max = new Vector3(10.0f, 10.0f, 10.0f);
        assertTrue(new Vector3(5.0f, 5.0f, 5.0f).inRange(min, max), "in range");
        assertFalse(new Vector3(15.0f, 5.0f, 5.0f).inRange(min, max), "x out");
        assertFalse(new Vector3(5.0f, 5.0f, -5.0f).inRange(min, max), "z out");
        // ignoreY=true (default): y 不影响判断
        assertTrue(new Vector3(5.0f, 999.0f, 5.0f).inRange(min, max), "ignoreY ignores y");
        // ignoreY=false
        assertFalse(new Vector3(5.0f, 999.0f, 5.0f).inRange(min, max, false), "no ignoreY fails");
    }

    // ---- multi ----
    static void testMulti()
    {
        Vector3 v = new Vector3(2.0f, 3.0f, 4.0f).multi(new Vector3(5.0f, 6.0f, 7.0f));
        assertTrue(v.x.isEqual(10.0f), "multi x 2*5=10");
        assertTrue(v.y.isEqual(18.0f), "multi y 3*6=18");
        assertTrue(v.z.isEqual(28.0f), "multi z 4*7=28");
    }

    // ---- divide ----
    static void testDivide()
    {
        Vector3 v = new Vector3(8.0f, 15.0f, 21.0f).divide(new Vector3(2.0f, 3.0f, 7.0f));
        assertTrue(v.x.isEqual(4.0f), "divide x 8/2=4");
        assertTrue(v.y.isEqual(5.0f), "divide y 15/3=5");
        assertTrue(v.z.isEqual(3.0f), "divide z 21/7=3");

        Vector3 v2 = new Vector3(8.0f, 15.0f, 21.0f).divide(2.0f);
        assertTrue(v2.x.isEqual(4.0f), "divide scalar x");
        assertTrue(v2.y.isEqual(7.5f), "divide scalar y");
        assertTrue(v2.z.isEqual(10.5f), "divide scalar z");
    }

    // ---- adjustRadian180 ----
    static void testAdjustRadian180()
    {
        Vector3 v = new Vector3(Mathf.PI * 1.5f, -Mathf.PI * 1.5f, Mathf.PI * 0.5f).adjustRadian180();
        assertTrue(v.x.isEqual(-Mathf.PI * 0.5f, 0.01f), "270->-90");
        assertTrue(v.y.isEqual(Mathf.PI * 0.5f, 0.01f), "-270->90");
        assertTrue(v.z.isEqual(Mathf.PI * 0.5f, 0.01f), "90 stays");
    }

    // ---- adjustAngle180 ----
    static void testAdjustAngle180()
    {
        Vector3 v = new Vector3(270.0f, -270.0f, 90.0f).adjustAngle180();
        assertTrue(v.x.isEqual(-90.0f, 0.01f), "270->-90");
        assertTrue(v.y.isEqual(90.0f, 0.01f), "-270->90");
        assertTrue(v.z.isEqual(90.0f), "90 stays");
    }

    // ---- adjustRadian360 ----
    static void testAdjustRadian360()
    {
        float twoPi = 2.0f * Mathf.PI;
        Vector3 v = new Vector3(-0.5f, twoPi + 1.0f, 1.0f).adjustRadian360();
        assertTrue(v.x.isEqual(twoPi - 0.5f, 0.01f), "-0.5->2pi-0.5");
        assertTrue(v.y.isEqual(1.0f, 0.01f), "2pi+1->1");
        assertTrue(v.z.isEqual(1.0f), "1 stays");
    }

    // ---- adjustAngle360 ----
    static void testAdjustAngle360()
    {
        Vector3 v = new Vector3(-10.0f, 370.0f, 180.0f).adjustAngle360();
        assertTrue(v.x.isEqual(350.0f, 0.01f), "-10->350");
        assertTrue(v.y.isEqual(10.0f, 0.01f), "370->10");
        assertTrue(v.z.isEqual(180.0f), "180 stays");
    }

    // ---- getAngle ----
    static void testGetAngle()
    {
        // 指向 Z 轴正方向
        float angle = new Vector3(0.0f, 0.0f, 1.0f).getAngle();
        assertTrue(angle.isZero(0.01f), "forward -> angle 0");

        // 指向 X 轴正方向, getAngle 内部取反后返回 pi/2
        float angle90 = new Vector3(1.0f, 0.0f, 0.0f).getAngle();
        assertTrue(angle90.isEqual(Mathf.PI * 0.5f, 0.01f), "right -> pi/2");

        // degree 模式
        float angleDeg = new Vector3(0.0f, 0.0f, 1.0f).getAngle(ANGLE.DEGREE);
        assertTrue(angleDeg.isZero(0.01f), "forward -> 0 deg");
    }

    // ---- rotate ----
    static void testRotate()
    {
        // 绕 Y 轴旋转 90 度 (x,0,z) -> (-z,0,x)
        Vector3 v = new Vector3(1.0f, 0.0f, 0.0f).rotate(Mathf.PI * 0.5f);
        assertTrue(v.x.isZero(0.01f), "rotate90 x -> 0");
        assertTrue(v.z.isEqual(-1.0f, 0.01f), "rotate90 z -> -1");
    }

    // ---- dot ----
    static void testDot()
    {
        float d = new Vector3(1.0f, 2.0f, 3.0f).dot(new Vector3(4.0f, 5.0f, 6.0f));
        assertTrue(d.isEqual(32.0f), "dot 1*4+2*5+3*6=32");

        float dOrth = new Vector3(1.0f, 0.0f, 0.0f).dot(new Vector3(0.0f, 1.0f, 0.0f));
        assertTrue(dOrth.isZero(), "orthogonal dot=0");
    }

    // ---- cross ----
    static void testCross()
    {
        Vector3 c = new Vector3(1.0f, 0.0f, 0.0f).cross(new Vector3(0.0f, 1.0f, 0.0f));
        assertTrue(c.x.isZero(), "cross x=0");
        assertTrue(c.y.isZero(), "cross y=0");
        assertTrue(c.z.isEqual(1.0f), "cross z=1 (right-handed)");
    }
}
