using UnityEngine;
using static TestAssert;

// FloatExtension 纯数学扩展方法测试
public static class FloatExtensionTest
{
    public static void Run()
    {
        testCeil();
        testIsNaN();
        testSaturate();
        testFloor();
        testRound();
        testStep();
        testFmod();
        testFrac();
        testAbs();
        testSinCosTan();
        testAsinAcosAtan();
        testSqrt();
        testCheckFloat();
        testCheckInt();
        testToRadianToDegree();
        testClamp();
        testClampMinMax();
        testIsZero();
        testIsEqual();
        testInverse();
        testDivide();
        testClampCycle();
        testInRange();
        testInRangeFixed();
        testAdjustRadian180();
        testAdjustAngle180();
        testAdjustRadian360();
        testAdjustAngle360();
        testGetVectorFromAngle();
        testGetVector2FromAngle();
        testKMHtoMSAndMStoKMH();
        testMtoKM();
        testPow();
    }

    // ---- ceil ----
    static void testCeil()
    {
        assertEqual(4, 3.2f.ceil(), "ceil 3.2 -> 4");
        assertEqual(4, 3.8f.ceil(), "ceil 3.8 -> 4");
        assertEqual(3, 3.0f.ceil(), "ceil 3.0 -> 3");
        assertEqual(-3, (-3.2f).ceil(), "ceil -3.2 -> -3");
        assertEqual(-3, (-3.8f).ceil(), "ceil -3.8 -> -3");
        assertEqual(-4, (-4.0f).ceil(), "ceil -4.0 -> -4");
        assertEqual(0, 0.0f.ceil(), "ceil 0 -> 0");
        assertEqual(-1, (-1.0f).ceil(), "ceil -1.0 -> -1");
    }

    // ---- isNaN ----
    static void testIsNaN()
    {
        assertTrue(float.NaN.isNaN(), "NaN is NaN");
        assertFalse(0.0f.isNaN(), "0 is not NaN");
        assertFalse(1.5f.isNaN(), "1.5 is not NaN");
        assertFalse(float.PositiveInfinity.isNaN(), "+Inf is not NaN");
    }

    // ---- saturate ----
    static void testSaturate()
    {
        assertTrue(0.5f.saturate().isEqual(0.5f), "saturate 0.5");
        assertTrue(1.5f.saturate().isEqual(1.0f), "saturate 1.5 -> 1");
        assertTrue((-0.5f).saturate().isEqual(0.0f), "saturate -0.5 -> 0");
        assertTrue(0.0f.saturate().isEqual(0.0f), "saturate 0");
        assertTrue(1.0f.saturate().isEqual(1.0f), "saturate 1");
    }

    // ---- floor ----
    static void testFloor()
    {
        assertEqual(3, 3.2f.floor(), "floor 3.2 -> 3");
        assertEqual(3, 3.8f.floor(), "floor 3.8 -> 3");
        assertEqual(3, 3.0f.floor(), "floor 3.0 -> 3");
        assertEqual(-4, (-3.2f).floor(), "floor -3.2 -> -4");
        assertEqual(-4, (-3.8f).floor(), "floor -3.8 -> -4");
        assertEqual(-4, (-4.0f).floor(), "floor -4.0 -> -4");
        assertEqual(0, 0.0f.floor(), "floor 0 -> 0");
        assertEqual(-1, (-1.0f).floor(), "floor -1.0 -> -1");
    }

    // ---- round ----
    static void testRound()
    {
        assertEqual(3, 3.2f.round(), "round 3.2 -> 3");
        assertEqual(4, 3.8f.round(), "round 3.8 -> 4");
        assertEqual(4, 3.5f.round(), "round 3.5 -> 4");
        assertEqual(-3, (-3.2f).round(), "round -3.2 -> -3");
        assertEqual(-4, (-3.8f).round(), "round -3.8 -> -4");
        assertEqual(-4, (-3.5f).round(), "round -3.5 -> -4");
        assertEqual(0, 0.0f.round(), "round 0 -> 0");
        assertEqual(1, 0.5f.round(), "round 0.5 -> 1");
        assertEqual(-1, (-0.5f).round(), "round -0.5 -> -1");
    }

    // ---- step ----
    static void testStep()
    {
        assertEqual(1, 2.0f.step(3.0f), "step(2, 3) -> 1 (3>=2)");
        assertEqual(0, 2.0f.step(1.0f), "step(2, 1) -> 0 (1<2)");
        assertEqual(1, 2.0f.step(2.0f), "step(2, 2) -> 1 (2>=2)");
        assertEqual(1, (-1.0f).step(0.0f), "step(-1, 0) -> 1");
        assertEqual(0, 0.0f.step(-1.0f), "step(0, -1) -> 0");
    }

    // ---- fmod ----
    static void testFmod()
    {
        assertTrue(5.5f.fmod(2.0f).isEqual(1.5f), "fmod 5.5 % 2.0 = 1.5");
        assertTrue(5.0f.fmod(2.0f).isEqual(1.0f), "fmod 5.0 % 2.0 = 1.0");
        assertTrue((-5.5f).fmod(2.0f).isEqual(-1.5f), "fmod -5.5 % 2.0 = -1.5");
        assertTrue(3.0f.fmod(1.5f).isEqual(0.0f), "fmod 3.0 % 1.5 = 0");
    }

    // ---- frac ----
    static void testFrac()
    {
        assertTrue(3.14f.frac().isEqual(0.14f, 0.01f), "frac 3.14 ~ 0.14");
        assertTrue((-3.14f).frac().isEqual(-0.14f, 0.01f), "frac -3.14 ~ -0.14");
        assertTrue(5.0f.frac().isZero(), "frac 5.0 = 0");
        assertTrue(0.0f.frac().isZero(), "frac 0 = 0");
    }

    // ---- abs ----
    static void testAbs()
    {
        assertTrue(5.0f.abs().isEqual(5.0f), "abs 5");
        assertTrue((-5.0f).abs().isEqual(5.0f), "abs -5");
        assertTrue(0.0f.abs().isZero(), "abs 0");
        assertTrue(3.14f.abs().isEqual(3.14f), "abs 3.14");
    }

    // ---- sin/cos/tan ----
    static void testSinCosTan()
    {
        float pi = Mathf.PI;
        assertTrue(0.0f.sin().isZero(), "sin 0 = 0");
        assertTrue((pi / 2).cos().isZero(0.01f), "cos pi/2 ~ 0");
        assertTrue((pi / 4).tan().isEqual(1.0f, 0.01f), "tan pi/4 ~ 1");
    }

    // ---- asin/acos/atan ----
    static void testAsinAcosAtan()
    {
        assertTrue(0.0f.asin().isZero(), "asin 0 = 0");
        assertTrue(1.0f.acos().isZero(), "acos 1 = 0");
        assertTrue(0.0f.atan().isZero(), "atan 0 = 0");
        // asin/acos 会自动 clamp 到 [-1,1]，不会崩溃
        assertNotNull(2.0f.asin(), "asin 2 clamped");
        assertNotNull(2.0f.acos(), "acos 2 clamped");
    }

    // ---- sqrt ----
    static void testSqrt()
    {
        assertTrue(4.0f.sqrt().isEqual(2.0f), "sqrt 4 = 2");
        assertTrue(0.0f.sqrt().isZero(), "sqrt 0 = 0");
        assertTrue(2.0f.sqrt().isEqual(1.4142f, 0.001f), "sqrt 2 ~ 1.414");
    }

    // ---- checkFloat ----
    static void testCheckFloat()
    {
        assertTrue(3.1415926535f.checkFloat(2).isEqual(3.14f, 0.001f), "checkFloat 3.1415 prec2 ~ 3.14");
        assertTrue(3.1415926535f.checkFloat(0).isEqual(3.0f), "checkFloat 3.1415 prec0 ~ 3");
        assertTrue(3.0f.checkFloat(4).isEqual(3.0f), "checkFloat 3.0 prec4 = 3");
    }

    // ---- checkInt ----
    static void testCheckInt()
    {
        assertTrue(1.0000001f.checkInt().isEqual(1.0f), "checkInt 1.0000001 -> 1");
        assertTrue(0.9999999f.checkInt().isEqual(1.0f), "checkInt 0.9999999 -> 1");
        assertTrue(0.0f.checkInt().isZero(), "checkInt 0 -> 0");
        assertTrue(3.5f.checkInt().isEqual(3.5f), "checkInt 3.5 -> 3.5 (not near int)");
        assertTrue((-0.9999999f).checkInt().isEqual(-1.0f), "checkInt -0.9999999 -> -1");
    }

    // ---- toRadian/toDegree ----
    static void testToRadianToDegree()
    {
        assertTrue(180.0f.toRadian().isEqual(Mathf.PI, 0.0001f), "180 deg -> pi rad");
        assertTrue(Mathf.PI.toDegree().isEqual(180.0f, 0.0001f), "pi rad -> 180 deg");
        assertTrue(0.0f.toRadian().isZero(), "0 deg -> 0 rad");
        assertTrue(0.0f.toDegree().isZero(), "0 rad -> 0 deg");
    }

    // ---- clamp ----
    static void testClamp()
    {
        assertTrue(0.5f.clamp(0.0f, 1.0f).isEqual(0.5f), "clamp 0.5 in [0,1]");
        assertTrue(1.5f.clamp(0.0f, 1.0f).isEqual(1.0f), "clamp 1.5 in [0,1] -> 1");
        assertTrue((-0.5f).clamp(0.0f, 1.0f).isEqual(0.0f), "clamp -0.5 in [0,1] -> 0");
        // min > max 返回 min
        assertTrue(0.5f.clamp(1.0f, 0.0f).isEqual(1.0f), "clamp min>max -> min");
        // min == max 返回 min
        assertTrue(0.5f.clamp(1.0f, 1.0f).isEqual(1.0f), "clamp min==max -> min");
    }

    // ---- clampMin/clampMax ----
    static void testClampMinMax()
    {
        assertTrue((-1.0f).clampMin().isEqual(0.0f), "clampMin -1 default -> 0");
        assertTrue(2.0f.clampMin().isEqual(2.0f), "clampMin 2 default -> 2");
        assertTrue(3.0f.clampMin(5.0f).isEqual(5.0f), "clampMin 3 min5 -> 5");
        assertTrue(7.0f.clampMax(5.0f).isEqual(5.0f), "clampMax 7 max5 -> 5");
        assertTrue(3.0f.clampMax(5.0f).isEqual(3.0f), "clampMax 3 max5 -> 3");
    }

    // ---- isZero ----
    static void testIsZero()
    {
        assertTrue(0.0f.isZero(), "0 is zero");
        assertTrue(0.00001f.isZero(), "0.00001 is zero (default prec)");
        assertFalse(0.001f.isZero(), "0.001 is not zero");
        assertTrue(0.001f.isZero(0.01f), "0.001 is zero with prec 0.01");
    }

    // ---- isEqual ----
    static void testIsEqual()
    {
        assertTrue(1.0f.isEqual(1.0f), "1 == 1");
        assertTrue(1.00001f.isEqual(1.0f), "1.00001 ~ 1");
        assertFalse(1.0f.isEqual(2.0f), "1 != 2");
    }

    // ---- inverse ----
    static void testInverse()
    {
        assertTrue(2.0f.inverse().isEqual(0.5f), "inverse 2 = 0.5");
        assertTrue(0.0f.inverse().isZero(), "inverse 0 = 0 (safe)");
        assertTrue((-4.0f).inverse().isEqual(-0.25f), "inverse -4 = -0.25");
    }

    // ---- divide ----
    static void testDivide()
    {
        assertTrue(10.0f.divide(2.0f).isEqual(5.0f), "10/2 = 5");
        assertTrue(10.0f.divide(0.0f).isZero(), "10/0 = 0 (safe)");
        assertTrue(10.0f.divide(0.0f, -1.0f).isEqual(-1.0f), "10/0 default -1");
    }

    // ---- clampCycle ----
    static void testClampCycle()
    {
        // 角度规范化测试
        assertTrue(370.0f.clampCycle(0.0f, 360.0f, 360.0f).isEqual(10.0f, 0.01f), "370 cycle -> 10");
        assertTrue((-10.0f).clampCycle(0.0f, 360.0f, 360.0f).isEqual(350.0f, 0.01f), "-10 cycle -> 350");
        assertTrue(180.0f.clampCycle(0.0f, 360.0f, 360.0f).isEqual(180.0f), "180 in range");
        // includeMax=false
        assertTrue(360.0f.clampCycle(0.0f, 360.0f, 360.0f, false).isEqual(0.0f, 0.01f), "360 cycle excl -> 0");
    }

    // ---- inRange ----
    static void testInRange()
    {
        assertTrue(5.0f.inRange(0.0f, 10.0f), "5 in [0,10]");
        assertTrue(0.0f.inRange(0.0f, 10.0f), "0 in [0,10]");
        assertTrue(10.0f.inRange(0.0f, 10.0f), "10 in [0,10]");
        assertTrue(5.0f.inRange(10.0f, 0.0f), "5 in [10,0] (auto swap)");
        assertFalse(15.0f.inRange(0.0f, 10.0f), "15 not in [0,10]");
    }

    // ---- inRangeFixed ----
    static void testInRangeFixed()
    {
        assertTrue(5.0f.inRangeFixed(0.0f, 10.0f), "5 inFixed [0,10]");
        assertFalse(5.0f.inRangeFixed(10.0f, 0.0f), "5 not inFixed [10,0] (no swap)");
    }

    // ---- adjustRadian180 ----
    static void testAdjustRadian180()
    {
        float pi = Mathf.PI;
        assertTrue((pi * 1.5f).adjustRadian180().isEqual(-pi * 0.5f, 0.01f), "270deg -> -90deg");
        assertTrue((-pi * 1.5f).adjustRadian180().isEqual(pi * 0.5f, 0.01f), "-270deg -> 90deg");
        assertTrue((pi * 0.5f).adjustRadian180().isEqual(pi * 0.5f, 0.01f), "90deg stays");
    }

    // ---- adjustAngle180 ----
    static void testAdjustAngle180()
    {
        assertTrue(270.0f.adjustAngle180().isEqual(-90.0f, 0.01f), "270 -> -90");
        assertTrue((-270.0f).adjustAngle180().isEqual(90.0f, 0.01f), "-270 -> 90");
        assertTrue(90.0f.adjustAngle180().isEqual(90.0f), "90 stays");
    }

    // ---- adjustRadian360 ----
    static void testAdjustRadian360()
    {
        float twoPi = 2.0f * Mathf.PI;
        assertTrue((-0.5f).adjustRadian360().isEqual(twoPi - 0.5f, 0.01f), "-0.5 -> 2pi-0.5");
        assertTrue((twoPi + 1.0f).adjustRadian360().isEqual(1.0f, 0.01f), "2pi+1 -> 1");
        assertTrue(1.0f.adjustRadian360().isEqual(1.0f), "1 stays");
    }

    // ---- adjustAngle360 ----
    static void testAdjustAngle360()
    {
        assertTrue((-10.0f).adjustAngle360().isEqual(350.0f, 0.01f), "-10 -> 350");
        assertTrue(370.0f.adjustAngle360().isEqual(10.0f, 0.01f), "370 -> 10");
        assertTrue(180.0f.adjustAngle360().isEqual(180.0f), "180 stays");
    }

    // ---- getVectorFromAngle ----
    static void testGetVectorFromAngle()
    {
        Vector3 v0 = 0.0f.getVectorFromAngle();
        assertTrue(v0.x.isZero(), "angle 0 -> x~0");
        assertTrue(v0.y.isZero(), "angle 0 -> y=0");
        assertTrue(v0.z.isEqual(1.0f), "angle 0 -> z~1");

        float halfPi = Mathf.PI * 0.5f;
        Vector3 v90 = halfPi.getVectorFromAngle();
        assertTrue(v90.x.isEqual(1.0f, 0.01f), "angle 90 -> x~1");
        assertTrue(v90.z.isZero(0.01f), "angle 90 -> z~0");
    }

    // ---- getVector2FromAngle ----
    static void testGetVector2FromAngle()
    {
        Vector2 v0 = 0.0f.getVector2FromAngle();
        assertTrue(v0.x.isZero(), "angle 0 -> x~0");
        assertTrue(v0.y.isEqual(1.0f), "angle 0 -> y~1");

        float halfPi = Mathf.PI * 0.5f;
        Vector2 v90 = halfPi.getVector2FromAngle();
        assertTrue(v90.x.isEqual(1.0f, 0.01f), "angle 90 -> x~1");
        assertTrue(v90.y.isZero(0.01f), "angle 90 -> y~0");
    }

    // ---- KMHtoMS / MStoKMH ----
    static void testKMHtoMSAndMStoKMH()
    {
        assertTrue(100.0f.KMHtoMS().isEqual(27.777f, 0.01f), "100 kmh -> 27.78 ms");
        assertTrue(27.777f.MStoKMH().isEqual(100.0f, 0.1f), "27.78 ms -> 100 kmh");
    }

    // ---- MtoKM ----
    static void testMtoKM()
    {
        assertTrue(1000.0f.MtoKM().isEqual(1.0f), "1000m -> 1km");
        assertTrue(0.0f.MtoKM().isZero(), "0m -> 0km");
    }

    // ---- pow ----
    static void testPow()
    {
        assertTrue(2.0f.pow(3).isEqual(8.0f), "2^3 = 8");
        assertTrue(2.0f.pow(0).isEqual(1.0f), "2^0 = 1");
        assertTrue(3.0f.pow(2.0f).isEqual(9.0f), "3^2.0 = 9 (float power)");
        assertTrue(4.0f.pow(0.5f).isEqual(2.0f), "4^0.5 = 2 (sqrt)");
    }
}
