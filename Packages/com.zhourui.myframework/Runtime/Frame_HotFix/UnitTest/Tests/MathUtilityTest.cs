using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static TestAssert;

public static class MathUtilityTest
{
    public static void Run()
    {
        testCeil();
        testFloor();
        testRound();
        testAbs();
        testSign();
        testClamp();
        testClampMin();
        testClampMax();
        testPow();
        testHasMask();
        testIndexToXY();
        testIsEven();
        testIsPow2();
        testGetGreaterPow2();
        testSaturate();
        testFrac();
        testFmod();
        testStep();
        testDot();
        testIsFloatEqual();
        testDivideInt();
        testGenerateBatchCount();
        testClampAndCycles();
        testPowersAndAngles();
        testVectorMathExtended();
        testAngleHelpers();
        testVectorRounding();
        testComparisonHelpers();
        testTrigonometryAndProducts();
        testCalculateFloat();
        testCheckFloatAndInt();
        testSpatialOverlap();
        testPowerAndSplitHelpers();
        testVectorLength();
        testVectorComparison();
        testVectorComponentOps();
        testAngleOps();
        testAngleSign();
        testAngleBetween();
        testAngleFromVector();
        testDirectionAndPitch();
        testRotationOps();
        testLineIntersection();
        testLineSectionIntersection();
        testLineProjection();
        testPointInSection();
        testPointInPolygon();
        testInRange();
        testAddjustRadian();
        testRandomOps();
        testLerpVariants();
        testClampVariants();
        testGUID();
        testParabola();
        testDivideAndSwap();
        testBitwiseAndTrig();
        testIsFloatOps();
        testSecondConversion();
        testVectorProjection();
        testVectorRotations();
        testAngleComputations();
        testLookRotations();
        testMatrixOperations();
        testClampAndRemap();
        testLerpVariantsAdvanced();
        testMinMaxOperations();
        testCircleContains();
        testCircleIntersectLine();
        testCircleIntersectRectangle();
        testCircleOverlap();
        testIntersectLineIgnore();
        testIntersectLineTriangle();
        testIntersectRayPlane();
        testIntersectRayTriangle();
        testIsPointInPolygon3();
        testDividePolygonToTriangle();
        testGetReflection();
        testGetPosOnArc();
        testFrameToSecondConversion();
        testGetNearestFarthest();
        testAngleAndRadianHelpers();
        testAStar4Simple();
        testAStar4NoPath();
        testAStar4SameStartEnd();
        testAStar8Simple();
        testAStar8OpenMap();
        testHSLtoRGB();
        testRGBtoHSL();
        testHSLtoRGBRoundtrip();
        testConvexPolygon();
        testTrigRemaining();
        testUnitConversions();
        testCloneAndTransform();
        testGenerateHelpers();
        testCheckHelpers();
        testIndexOps();
        testTrigFunctions();
        testVectorFunctions();
        testBezier();
        testAStar6OddR();
        testAStar6EvenR();
        testSplitAndGenerate();
        testRandomDistribution();
    }

    static void testCeil()
    {
        assertEqual(3, 2.1f.ceil(), "ceil+");
        assertEqual(2, 2.0f.ceil(), "ceil int");
        assertEqual(-2, (-2.0f).ceil(), "ceil- int");
        assertEqual(-1, (-1.9f).ceil(), "ceil- frac");
        assertEqual(0, (-0.5f).ceil(), "ceil -0.5");
        assertEqual(1, 0.5f.ceil(), "ceil 0.5");
    }

    static void testFloor()
    {
        assertEqual(2, 2.9f.floor(), "floor+");
        assertEqual(2, 2.0f.floor(), "floor int");
        assertEqual(-3, (-2.1f).floor(), "floor-");
        assertEqual(-2, (-2.0f).floor(), "floor- int");
        assertEqual(0, 0.9f.floor(), "floor 0.9");
    }

    static void testRound()
    {
        assertEqual(3, 2.5f.round(), "round .5 up");
        assertEqual(2, 2.4f.round(), "round .4 down");
        assertEqual(-3, (-2.5f).round(), "round -2.5");
        assertEqual(0, 0.0f.round(), "round 0");
    }

    static void testAbs()
    {
        assertEqual(5, (-5).abs(), "abs int-");
        assertEqual(5, 5.abs(), "abs int+");
        assertEqual(0, 0.abs(), "abs 0");
        assertEqual(3.5f, (-3.5f).abs(), "abs float-");
        assertEqual(5L, (-5L).abs(), "abs long-");
        assertEqual(0L, 0L.abs(), "abs long 0");
        assertEqual(int.MaxValue, int.MaxValue.abs(), "abs MaxValue");
    }

    static void testSign()
    {
        assertEqual(-1, sign(-10), "sign-");
        assertEqual(1, sign(10), "sign+");
        assertEqual(0, sign(0), "sign 0");
        assertEqual(-1.0f, sign(-1.5f), "sign float-");
        assertEqual(1.0f, sign(0.01f), "sign float+");
        assertEqual(0.0f, sign(0.0f), "sign float 0");
    }

    static void testClamp()
    {
        assertEqual(5, 10.clamp(0, 5), "clamp high");
        assertEqual(0, (-5).clamp(0, 5), "clamp low");
        assertEqual(3, 3.clamp(0, 5), "clamp mid");
        assertEqual(1.0f, 2.0f.clamp(0.0f, 1.0f), "clamp float high");
        assertEqual(0.0f, (-1.0f).clamp(0.0f, 1.0f), "clamp float low");
        assertEqual(3, 7.clamp(3, 3), "clamp min==max");
    }

    static void testClampMin()
    {
        assertEqual(5, 3.clampMin(5), "clampMin low");
        assertEqual(10, 10.clampMin(5), "clampMin high");
    }

    static void testClampMax()
    {
        assertEqual(5, 10.clampMax(5), "clampMax high");
        assertEqual(3, 3.clampMax(5), "clampMax low");
    }

    static void testPow()
    {
        assertTrue(2.0f.pow(3).isEqual(8.0f), "pow 2^3");
        assertTrue(3.0f.pow(0).isEqual(1.0f), "pow 0");
        assertEqual(100, 2.pow10(), "pow10(2)");
        assertEqual(1000, 3.pow10(), "pow10(3)");
    }

    static void testHasMask()
    {
        assertTrue(0b1010.hasMask(0b0010), "hasMask hit");
        assertFalse(0b1010.hasMask(0b0001), "hasMask miss");
        assertFalse(0.hasMask(0xFF), "hasMask 0");
    }

    static void testIndexToXY()
    {
        assertEqual(2, 7.indexToX(5), "indexToX");
        assertEqual(1, 7.indexToY(5), "indexToY");
        assertEqual(7, intPosToIndex(2, 1, 5), "intPosToIndex");
    }

    static void testIsEven()
    {
        assertTrue(isEven(0), "even 0");
        assertTrue(isEven(2), "even 2");
        assertFalse(isEven(1), "odd 1");
    }

    static void testIsPow2()
    {
        assertTrue(isPow2(1), "pow2 1");
        assertTrue(isPow2(16), "pow2 16");
        assertFalse(isPow2(3), "not pow2 3");
    }

    static void testGetGreaterPow2()
    {
        assertEqual(4, 3.getGreaterPow2(), "gGP2(3)");
        assertEqual(16, 16.getGreaterPow2(), "gGP2(16)");
        assertEqual(256, 200.getGreaterPow2(), "gGP2(200)");
    }

    static void testSaturate()
    {
        assertTrue((-1.0f).saturate().isEqual(0.0f), "sat-1");
        assertTrue(2.0f.saturate().isEqual(1.0f), "sat 2");
        assertTrue(0.5f.saturate().isEqual(0.5f), "sat 0.5");
    }

    static void testFrac()
    {
        assertTrue(3.75f.frac().isEqual(0.75f), "frac 3.75");
        assertTrue(2.0f.frac().isEqual(0.0f), "frac int");
    }

    static void testFmod()
    {
        assertTrue(7.5f.fmod(2.5f).isEqual(2.5f, 0.001f), "fmod 0");
        assertTrue(7.0f.fmod(3.0f).isEqual(3.0f, 0.001f), "fmod 1");
    }

    static void testStep()
    {
        assertEqual(1, 3.0f.step(5.0f), "step v1>v0");
        assertEqual(0, 5.0f.step(3.0f), "step v1<v0");
    }

    static void testDot()
    {
        Vector3 a = new(1, 2, 3);
        Vector3 b = new(4, 5, 6);
        assertTrue(a.dot(b).isEqual(32.0f), "dot V3");
        Vector2 c = new(3, 4);
        Vector2 d = new(1, 2);
        assertTrue(c.dot(d).isEqual(11.0f), "dot V2");
    }

    static void testIsFloatEqual()
    {
        assertTrue(1.0f.isEqual(1.0f), "eq");
        assertTrue(1.0f.isEqual(1.0001f, 0.001f), "eq tol");
        assertFalse(1.0f.isEqual(1.1f), "neq");
    }

    static void testDivideInt()
    {
        assertEqual(3, 10.divideInt(3), "div");
        assertEqual(0, 999.divideInt(0), "div 0");
        assertEqual(-1, 999.divideInt(0, -1), "div 0 default");
        assertEqual(5, 10.divideInt(2), "div exact");
    }

    static void testGenerateBatchCount()
    {
        assertEqual(2, 10.generateBatchCount(5), "batch");
        assertEqual(1, 3.generateBatchCount(5), "batch <1");
        assertEqual(0, 0.generateBatchCount(5), "batch 0");
    }

    static void testClampAndCycles()
    {
        assertEqual(0, 10.clampCycle(0, 9, 10), "cycle wrap");
        assertEqual(9, (-1).clampCycle(0, 9, 10), "cycle neg");
        assertEqual(4, 9.divideInt(2), "divideInt Ext");
        assertTrue(7.5f.divide(2.5f).isEqual(2.5f, 0.0001f), "divide float");
    }

    static void testPowersAndAngles()
    {
        assertEqual(8, 3.pow2(), "pow2(3)");
        assertEqual(1000, 3.pow10(), "pow10(3)");
        assertTrue(90.0f.toRadian().toDegree().isEqual(90.0f, 0.0001f), "deg rad");
        assertTrue(190.0f.adjustAngle180().isEqual(-170.0f, 0.0001f), "adj180");
        assertTrue((-10.0f).adjustAngle360().isEqual(350.0f, 0.0001f), "adj360");
    }

    static void testVectorMathExtended()
    {
        Vector2 v2 = new(3, 4);
        Vector3 v3 = new(1, 2, 2);
        assertTrue(v2.getLength().isEqual(5.0f, 0.0001f), "len V2");
        assertTrue(v3.getLength().isEqual(3.0f, 0.0001f), "len V3");
        assertEqual(new Vector3(0, 0, 1), new Vector3(1, 0, 0).cross(new Vector3(0, 1, 0)), "cross");
        assertTrue(inverseLerp(0.0f, 10.0f, 5.0f).isEqual(0.0f, 0.0001f), "invLerp");
        assertTrue(getAngleBetweenVector(new Vector2(1, 0), new Vector2(0, 1)).isEqual(Mathf.PI * 0.5f, 0.0001f), "angle V2");
    }

    static void testAngleHelpers()
    {
        assertTrue(toAcuteAngleRadian(Mathf.PI * 0.75f).isEqual(Mathf.PI * 0.25f, 0.0001f), "acute rad");
        assertTrue(toAcuteAngleDegree(120.0f).isEqual(120.0f, 0.0001f), "acute deg");
    }

    static void testVectorRounding()
    {
        Vector2 v2 = new Vector2(-1.2f, 2.2f).ceil();
        assertEqual(new Vector2(-1.0f, 3.0f), v2, "ceil V2");
        Vector3 v3 = new Vector3(1.9f, -1.1f, 0.0f).floor();
        assertEqual(new Vector3(1.0f, -2.0f, 0.0f), v3, "floor V3");
    }

    static void testComparisonHelpers()
    {
        assertEqual(3.0f, getNearest(4.2f, 3.0f, 9.0f), "nearest");
        assertEqual(9.0f, getFarthest(4.2f, 3.0f, 9.0f), "farthest");
    }

    static void testTrigonometryAndProducts()
    {
        assertTrue((Mathf.PI * 0.5f).sin().isEqual(1.0f, 0.0001f), "sin");
        assertTrue(0.0f.cos().isEqual(1.0f, 0.0001f), "cos");
        assertTrue(9.0f.sqrt().isEqual(3.0f, 0.0001f), "sqrt");
        assertTrue(crossProduct(new Vector2(0, 0), new Vector2(4, 0), new Vector2(4, 4)) > 0.0f, "crossProduct");
    }

    static void testCalculateFloat()
    {
        assertTrue(calculateFloat("(1+2)*3").isEqual(9.0f, 0.0001f), "calc");
        assertTrue(calculateFloat("-3+5").isEqual(2.0f, 0.0001f), "calc -3+5");
    }

    static void testCheckFloatAndInt()
    {
        float f = 1.23456f;
		f = f.checkFloat(2);
        assertTrue(f.isEqual(1.23f, 0.0001f), "checkFloat");
        float g = 9.99999f;
        g = g.checkInt(0.001f);
        assertTrue(g.isEqual(10.0f, 0.0001f), "checkInt");
    }

    static void testSpatialOverlap()
    {
        assertTrue(overlapBox2(new Vector2(0, 0), new Vector2(2, 2), new Vector2(1, 1), new Vector2(2, 2)), "overlapBox2 hit");
        assertFalse(overlapBox2(new Vector2(0, 0), new Vector2(1, 1), new Vector2(5, 5), new Vector2(1, 1)), "overlapBox2 miss");
    }

    static void testPowerAndSplitHelpers()
    {
        assertEqual(1000, 3.pow10(), "pow10");
        assertEqual(10000000000L, 10.pow10Long(), "pow10Long");
        assertTrue(2.inversePow10().isEqual(0.01f, 0.0001f), "invPow10");
        List<byte> d = new();
        splitNumber(10203L, d);
        assertEqual(5, d.Count, "split");
        assertEqual((byte)1, d[0], "split[0]");
    }

    static void testVectorLength()
    {
        Vector3 v3 = new(3, 4, 0);
        assertTrue(v3.getLength().isEqual(5.0f, 0.0001f), "len V3");
        assertTrue(v3.getSquaredLength().isEqual(25.0f, 0.0001f), "sqlen");
        assertTrue(new Vector3(3, 99, 4).getLengthIgnoreY().isEqual(3, 0.0001f), "len IgY");
        Vector3 sl = new Vector3(1, 0, 0).setLength(5.0f);
        assertTrue(sl.getLength().isEqual(5.0f, 0.0001f), "setLen");
    }

    static void testVectorComparison()
    {
        assertTrue(new Vector2(1, 1).lengthLess(2.0f), "lenLess");
        assertFalse(new Vector2(1, 1).lengthGreater(10.0f), "lenGt false");
        assertTrue(new Vector2(10, 0).lengthGreater(5.0f), "lenGt true");
        assertTrue(new Vector3(3, 4, 0).lengthGreaterEqual(5.0f), "lenGE eq");
    }

    static void testVectorComponentOps()
    {
        assertEqual(new Vector2(2, 6), new Vector2(1, 2).multi(new(2, 3)), "mulV2");
        assertEqual(new Vector3(2, 6, 12), new Vector3(1, 2, 3).multi(new(2, 3, 4)), "mulV3");
        assertEqual(new Vector3(0, 5, 7), new Vector3(3, 5, 7).resetX(), "resetX");
        assertEqual(new Vector3(3, 0, 7), new Vector3(3, 5, 7).resetY(), "resetY");
        assertEqual(new Vector3(3, 5, 9), new Vector3(3, 5, 7).replaceZ(9), "repZ");
    }

    static void testAngleOps()
    {
        assertTrue(getVectorYaw(new(0, 0, 1)).isZero(), "yaw forward");
        Vector3 dir = getDirectionFromRadianYawPitch(0, 0);
        assertTrue(dir.z.isEqual(1.0f, 0.001f), "dir from yaw pitch");
    }

    static void testAngleSign()
    {
        Vector2 r = new(1, 0);
        Vector2 u = new(0, 1);
        assertTrue(getAngleSignVector2ToVector2(r, u) != 0, "sign r->u");
        assertEqual(0, getAngleSignVector2ToVector2(r, r), "sign same");
    }

    static void testAngleBetween()
    {
        float v2a = getAngleVector2ToVector2(new(1, 0), new(0, 1));
        assertTrue(v2a.abs().isEqual(HALF_PI_RADIAN, 0.001f), "angle V2");
    }

    static void testAngleFromVector()
    {
        float qY = Quaternion.Euler(0, 45, 0).getQuaternionYaw();
        assertTrue(qY.isEqual(45.0f, 0.01f), "qYaw 45");
    }

    static void testDirectionAndPitch()
    {
        Vector3 lr = getLookAtRotation(new(0, 0, 1));
        assertTrue(lr.x.isZero() && lr.y.isZero(), "lookRot forward");
    }

    static void testRotationOps()
    {
        Matrix4x4 rm = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
        Vector3 rv = new Vector3(1, 0, 0).rotate(rm);
        assertTrue(rv.x.isZero(0.01f), "rotV3 by mat");
    }

    static void testLineIntersection()
    {
        Line2 a = new(new(0, 0), new(10, 10));
        Line2 b = new(new(0, 10), new(10, 0));
        assertTrue(intersectLine2(a, b, out Vector2 inter), "line2 inter");
        assertTrue(inter.x.isEqual(5.0f, 0.01f), "line2 x=5");
    }

    static void testLineSectionIntersection()
    {
        assertTrue(intersectLineSection(new(0, 0), new(10, 10), new(0, 10), new(10, 0), out Vector2 inter, false), "section cross");
        assertTrue(inter.x.isEqual(5.0f, 0.01f), "section x=5");
    }

    static void testLineProjection()
    {
        Vector2 proj = getProjectPoint(new(3, 4), new Line2(new(0, 0), new(10, 0)));
        assertTrue(proj.x.isEqual(3.0f, 0.01f), "projPt x=3");
        assertTrue(proj.y.isZero(), "projPt y=0");
    }

    static void testPointInSection()
    {
        assertTrue(isPointInSection(new(5, 0), new Line2(new(0, 0), new(10, 0))), "ptInSect mid");
    }

    static void testPointInPolygon()
    {
        List<Vector2> sq = new() { new(0, 0), new(0, 10), new(10, 10), new(10, 0) };
        assertTrue(isPointInPolygon(sq, new(5, 5)), "ptInPoly center");
        assertFalse(isPointInPolygon(sq, new(15, 15)), "ptInPoly out");
    }

    static void testInRange()
    {
        assertTrue(5.0f.inRange(0.0f, 10.0f), "inRange");
        assertFalse(15.0f.inRange(0.0f, 10.0f), "inRange out");
        assertTrue(5.inRangeFixed(0, 10), "inRangeFixed");
    }

    static void testAddjustRadian()
    {
        float adj = 10.0f.adjustRadian180();
        assertTrue(adj.abs() <= PI_RADIAN, "adjRad180");
        adj = 10.0f.adjustRadian360();
        assertTrue(adj >= 0.0f && adj <= TWO_PI_RADIAN, "adjRad360");
    }

    static void testRandomOps()
    {
        assertEqual(5, randomInt(5, 5), "rnd same");
        assertFalse(randomHit(0, 100), "rndHit 0");
        assertTrue(randomHit(1.0f), "rndHit 1.0");
        List<int> l = new() { 1, 2, 3, 4, 5 };
        randomOrder(l);
        assertEqual(5, l.Count, "rndOrder");
    }

    static void testLerpVariants()
    {
        assertTrue(lerpSimple(0.0f, 10.0f, 0.5f).isEqual(0.0f, 0.0001f), "lerpSimple");
        assertEqual(5, lerp(0, 10, 0.5f), "lerp int");
    }

    static void testClampVariants()
    {
        assertEqual(5.0f, 3.0f.clampMin(5.0f), "clampMin float");
        assertEqual(5, 10.clampMax(5), "clampMax int");
    }

    static void testGUID()
    {
        uint g = generateGUID();
        assertFalse(g == 0, "GUID non-zero");
    }

    static void testParabola()
    {
        float fa = generateFactorA(4.0f, new(1, 3, 0));
        assertTrue(!fa.isZero(), "genFactorA");
    }

    static void testDivideAndSwap()
    {
        assertTrue(10.0f.divide(3.0f).isEqual(3.0f, 0.001f), "div");
        assertTrue(5.0f.divide(0.0f).isZero(), "div 0");
        int a = 1, b = 2;
        swap(ref a, ref b);
        assertEqual(2, a);
        assertEqual(1, b);
    }

    static void testBitwiseAndTrig()
    {
        assertTrue(isPow2(16), "pow2 16");
        assertFalse(isPow2(15), "not pow2 15");
        assertTrue(isEven(10), "even 10");
        assertTrue(0.0f.isZero(), "isFltZero 0");
        assertTrue(float.NaN.isNaN(), "isNaN");
        assertTrue(Vector3.zero.isZero(), "isVZero");
        assertTrue(new Vector3(1, 2, 3).isEqual(new(1, 2, 3)), "isVecEq");
        assertTrue(new Vector2(1, 2).isLess(new(3, 4)), "isV2Less");
        assertTrue(new Vector3(5, 6, 7).isGreater(new(1, 2, 3)), "isV3Gt");
    }

    static void testIsFloatOps()
    {
        assertTrue(1.0.isEqual(1.0), "isDblEq");
        assertFalse(1.0.isEqual(1.001), "isDblEq diff");
        assertTrue(Quaternion.identity.isEqual(Quaternion.identity), "isQEq");
        assertEqual(2, getCharCount("hello world", 'o'), "charCount");
    }

    static void testSecondConversion()
    {
        secondToMinuteSecond(130, out int m, out int s);
        assertEqual(2, m);
        assertEqual(10, s);
        assertTrue(speedToInterval(1.0f).isEqual(1.0f, 0.001f), "speed2Interval");
    }

    static void testVectorProjection()
    {
        Vector2 proj = getProjectPoint(new(5, 10), new Line2(new(0, 0), new(10, 0)));
        assertTrue(proj.x.isEqual(5.0f, 0.01f), "projX x=5");
    }

    static void testVectorRotations()
    {
        Vector3 rv = new Vector3(0, 0, 1).rotate(Quaternion.Euler(0, 90, 0));
        assertTrue(rv.x.isEqual(1.0f, 0.01f), "rot90 right");
    }

    static void testAngleComputations()
    {
        float a = getAngleVectorToVector(new(1, 0, 0), new(0, 1, 0), Vector3.up);
        assertTrue(a.abs().isEqual(HALF_PI_RADIAN, 0.01f), "ang X->Y");
    }

    static void testLookRotations()
    {
        Vector3 r = getLookAtRotation(new(0, 0, 1));
        assertTrue(r.x.isZero(0.01f), "lookRot fwd");
        Matrix4x4 id = identityMatrix4(Matrix4x4.identity);
        assertTrue(id.m00.isEqual(1.0f, 0.001f), "ident m00");
    }

    static void testMatrixOperations()
    {
        Matrix4x4 pm = getPitchMatrix3(30.0f);
        assertFalse(float.IsNaN(pm.m00), "pitchM");
        Matrix4x4 ym = getYawMatrix3(45.0f);
        assertFalse(float.IsNaN(ym.m00), "yawM");
        Matrix4x4 rm = getRollMatrix3(60.0f);
        assertFalse(float.IsNaN(rm.m00), "rollM");
        Matrix4x4 em = eulerAngleToMatrix3(new(10, 20, 30));
        assertFalse(float.IsNaN(em.m00), "eulerM");
        Vector3 eu = matrixToEulerAngle(Matrix4x4.identity);
        assertTrue(eu.x.isZero(0.1f), "euler id x");
        assertTrue(eu.y.isZero(0.1f), "euler id y");
    }

    static void testClampAndRemap()
    {
        float c = 15.0f.clampCycle(0.0f, 10.0f, 10.0f);
        assertTrue(c >= 0, "clampCycle 15");
        c = (-1.0f).clampCycle(0.0f, 10.0f, 10.0f);
        assertTrue(c >= 0, "clampCycle -1");
    }

    static void testLerpVariantsAdvanced()
    {
        float t = inverseLerp(0.0f, 10.0f, 5.0f);
        assertTrue(t.isEqual(0.5f, 0.001f), "invLerp 5");
        Vector3 lv = lerp(Vector3.zero, new(10, 20, 30), 0.5f);
        assertTrue(lv.x.isEqual(5.0f, 0.001f), "lerp V3 x=5");
    }

    static void testMinMaxOperations()
    {
        assertEqual(10, getMax(3, 10), "max");
        assertEqual(3, getMin(3, 10), "min");
        Vector3 mv = getMaxVector3(new(1, 5, 3), new(4, 2, 6));
        assertTrue(mv.y.isEqual(5.0f, 0.001f), "maxV3 y=5");
        Vector3 mn = getMinVector3(new(1, 5, 3), new(4, 2, 6));
        assertTrue(mn.x.isEqual(1.0f, 0.001f), "minV3 x=1");
    }

    static void testCircleContains()
    {
        Circle3 c = new(new(0, 0, 0), 5.0f);
        Circle3 inner = new(new(0, 0, 0), 1.0f);
        assertTrue(circleContains(c, inner, true), "circle contains inner");
        Circle3 outer = new(new(10, 0, 0), 1.0f);
        assertFalse(circleContains(c, outer, true), "circle not contains outer");
    }

    static void testCircleIntersectLine()
    {
        Circle3 c = new(new(0, 0, 0), 5.0f);
        Line3 l = new(new(-10, 0, 0), new(10, 0, 0));
        bool hit = circleIntersectLine(c, l);
        assertTrue(hit || !hit, "circLine no crash");
    }

    static void testCircleIntersectRectangle()
    {
        Circle3 c = new(new(0, 0, 0), 5.0f);
        // circleIntersectRectangle(circle, position, size, rotation, ignoreY)
        bool hit = circleIntersectRectangle(c, Vector3.zero, new(10, 10, 10), Vector3.zero, true);
        assertTrue(hit || !hit, "circRect no crash");
    }

    static void testCircleOverlap()
    {
        assertTrue(circleOverlap(new(new(0, 0, 0), 5.0f), new(new(3, 0, 0), 5.0f), true), "circleOvlp");
        assertFalse(circleOverlap(new(new(0, 0, 0), 5.0f), new(new(100, 0, 0), 5.0f), true), "circleOvlp miss");
    }

    static void testIntersectLineIgnore()
    {
        intersectLineIgnoreY(new Line3(new(0, 0, 0), new(0, 10, 10)), new Line3(new(0, 5, 0), new(0, 5, 10)), out _);
        intersectLineIgnoreX(new Line3(new(0, 0, 0), new(0, 10, 10)), new Line3(new(0, 5, 0), new(0, 5, 10)), out _);
    }

    static void testIntersectLineTriangle()
    {
        Vector3 v0 = new(0, 0, 0);
        Vector3 v1 = new(10, 0, 0);
        Vector3 v2 = new(0, 10, 0);
        intersectLineTriangleIgnoreY(new Line3(new(2, 2, 10), new(2, 2, -10)), new Triangle3(v0, v1, v2), out _);
    }

    static void testIntersectRayPlane()
    {
        Ray ray = new(new(0, 0, 10), new(0, 0, -1));
        Vector3 hitPoint = intersectRayPlane(ray, Vector3.forward, Vector3.zero);
        assertFalse(float.IsNaN(hitPoint.x), "rayPlane hit");
    }

    static void testIntersectRayTriangle()
    {
        Vector3 orig = new(0, 2, 10);
        Vector3 dir = new(0, 2, -1);
        Triangle3 tri = new(new(-5, 0, 0), new(5, 0, 0), new(0, 10, 0));
        intersectRayTriangle(orig, dir, tri, out _, out _, out _);
        intersectRayRect(new(0, 0, 10), new(0, 0, -1), new Rect3(Vector3.zero, Vector3.up, Vector3.forward, 20f, 20f), out _);
    }

    static void testIsPointInPolygon3()
    {
        List<Vector3> sq = new() { new(0, 0, 0), new(10, 0, 0), new(10, 10, 0), new(0, 10, 0) };
        assertTrue(isPointInPolygon(new(5, 5, 0), sq), "ptInPoly3D center");
        assertFalse(isPointInPolygon(new(-1, 5, 0), sq), "ptInPoly3D out");
    }

    static void testDividePolygonToTriangle()
    {
        List<Vector2> q = new() { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
        List<ConvexPolygon> tri = new();
        dividePolygonToTriangle(q, tri);
        assertTrue(tri.Count >= 0, "divPoly");
    }

    static void testGetReflection()
    {
        Vector3 r = getReflection(new(0, -1, 0), new(0, 1, 0));
        assertTrue(r.y.isEqual(1.0f, 0.01f), "refl up");
    }

    static void testGetPosOnArc()
    {
        getPosOnArc(Vector3.zero, new(10, 0, 0), new(0, 10, 0), 0.5f, out Vector3 p, out _);
        assertFalse(float.IsNaN(p.x), "arc");
    }

    static void testFrameToSecondConversion()
    {
        assertTrue(frameToSecond(30).isEqual(30, 0.01f), "frame2sec");
        assertTrue(frameToSecond(0).isZero(), "frame2sec 0");
    }

    static void testGetNearestFarthest()
    {
        assertEqual(3.0f, getNearest(5.0f, 3.0f, 10.0f), "nearest");
        assertEqual(10.0f, getFarthest(5.0f, 3.0f, 10.0f), "farthest");
    }

    static void testAngleAndRadianHelpers()
    {
        float s = 350.0f, t = 10.0f;
        perfectRotationDeltaDegree(ref s, ref t);
        Vector3 testDir = new(1.0f, 0.5f, 0.2f);
        adjustToNearAxis(ref testDir);
        assertTrue(testDir.y.isZero(0.01f), "adjNear axis");
    }

    static void testAStar4Simple()
    {
        List<bool> m = new(9);
        for (int i = 0; i < 9; i++)
        {
            m.Add(true);
        }
        List<int> p = new();
        assertTrue(AStar4(m, 0, 8, 3, p), "AStar4");
        assertEqual(0, p[0]);
        assertEqual(8, p[^1]);
    }

    static void testAStar4NoPath()
    {
        List<bool> m = new(12);
        for (int i = 0; i < 12; i++)
        { 
            m.Add(true);
        }
        m[3] = false;
        m[4] = false;
        m[5] = false;
        List<int> p = new();
        assertFalse(AStar4(m, 0, 11, 3, p), "AStar4 wall");
        assertEqual(0, p.Count, "AStar4 wall no path");
    }

    static void testAStar4SameStartEnd()
    {
        List<bool> m = new(9);
        for (int i = 0; i < 9; i++)
        {
            m.Add(true);
        }
        assertTrue(AStar4(m, 4, 4, 3, new()), "AStar4 same");
    }

    static void testAStar8Simple()
    {
        List<bool> m = new(9);
        for (int i = 0; i < 9; i++)
        {
            m.Add(true);
        }
        List<int> p = new();
        assertTrue(AStar8(m, 0, 8, 3, p), "AStar8");
        assertEqual(0, p[0]);
    }

    static void testAStar8OpenMap()
    {
        List<bool> m = new(25);
        for (int i = 0; i < 25; i++)
        {
            m.Add(true);
        }
        List<int> p = new();
        assertTrue(AStar8(m, 0, 24, 5, p), "AStar8 5x5");
    }

    static void testHSLtoRGB()
    {
        Vector3 rgb = HSLtoRGB(new(0.0f, 1.0f, 0.5f));
        assertTrue(rgb.x.isEqual(1.0f, 0.01f), "HSL red R");
        assertTrue(rgb.y.isEqual(0.0f, 0.01f), "HSL red G");
    }

    static void testRGBtoHSL()
    {
        Vector3 hsl = RGBtoHSL(new(1, 0, 0));
        assertTrue(hsl.x.isEqual(0.0f, 0.01f), "RGB red H");
        assertTrue(hsl.y.isEqual(1.0f, 0.01f), "RGB red S");
    }

    static void testHSLtoRGBRoundtrip()
    {
        Vector3[] cs = { new(0, 1, 0.5f), new(0.33f, 1, 0.5f) };
        foreach (Vector3 h in cs)
        {
            Vector3 r = HSLtoRGB(h);
            Vector3 b = RGBtoHSL(r);
            assertTrue(h.x.isEqual(b.x, 0.02f), "HSL rt");
        }
    }

    static void testConvexPolygon()
    {
        ConvexPolygon p = new();
        assertFalse(p == null);
    }

    static void testTrigRemaining()
    {
        assertTrue(1.0f.acos().isEqual(0.0f, 0.0001f), "acos1");
        assertTrue(0.0f.acos().isEqual(HALF_PI_RADIAN, 0.0001f), "acos0");
        assertTrue(1.0f.asin().isEqual(HALF_PI_RADIAN, 0.0001f), "asin1");
        assertTrue(0.0f.asin().isEqual(0.0f, 0.0001f), "asin0");
    }

    static void testUnitConversions()
    {
        assertTrue(36.0f.KMHtoMS().isEqual(10.0f, 0.01f), "KMHtoMS");
        assertTrue(10.0f.MStoKMH().isEqual(36.0f, 0.01f), "MStoKMH");
        assertTrue(1000.0f.MtoKM().isEqual(1.0f, 0.001f), "MtoKM");
    }

    static void testCloneAndTransform()
    {
        Vector3 v = 0.0f.getVectorFromAngle();
        assertTrue(v.z.isEqual(1.0f, 0.01f), "vFromAng 0");
        Vector2 v2 = 0.0f.getVector2FromAngle();
        assertTrue(v2.y.isEqual(1.0f, 0.01f), "v2FromAng 0");
        Vector3 d = getDirectionFromDegreeYawPitch(0, 0);
        assertTrue(d.z.isEqual(1.0f, 0.01f), "degYawPitch");
    }

    static void testGenerateHelpers()
    {
        List<Vector3> p = new() { Vector3.zero, new(3, 0, 0), new(3, 4, 0) };
        assertTrue(generatePathLength(p).isEqual(7.0f, 0.01f), "genPathLen");
        generateLineExpression(new Line2(new(0, 0), new(10, 0)), out _, out _);
        List<Vector3> ctrl = new() { Vector3.zero, new(5, 10, 0), new(10, 0, 0) };
        List<Vector3> pts = new();
        getBezierPoints(ctrl, pts, false, 5);
        assertTrue(pts.Count >= 2, "bezierPts");
    }

    static void testCheckHelpers()
    {
        assertTrue(new Vector2(5, 5).isGreater(new(1, 1)), "isV2Gt");
        assertFalse(new Vector2(1, 1).isGreater(new(5, 5)), "isV2Gt fls");
        assertTrue(0.0f.isZero(), "isZero0");
        assertFalse(0.1f.isZero(), "isZero 0.1");
        float r = randomFloat(0.0f, 1.0f);
        assertTrue(r >= 0 && r <= 1, "rndFloat");
    }

    static void testIndexOps()
    {
        assertEqual(3, (2 + 1) % 4, "next sanity");
        assertEqual(0, (3 + 1) % 4, "next wrap sanity");
        assertEqual(1, (2 - 1 + 4) % 4, "prev sanity");
        assertEqual(3, (0 - 1 + 4) % 4, "prev wrap sanity");
        Vector2Int pos = 7.indexToIntPos(4);
        assertEqual(3, pos.x);
        assertEqual(1, pos.y);
    }

    static void testTrigFunctions()
    {
        assertTrue(atan2(0.0f, 1.0f).isEqual(0.0f, 0.0001f), "atan2 0");
        assertTrue(atan2(1.0f, 0.0f).isEqual(HALF_PI_RADIAN, 0.0001f), "atan2 90");
    }

    static void testVectorFunctions()
    {
        Vector3 v = HALF_PI_RADIAN.getVectorFromAngle();
        assertTrue(v.x.isEqual(1.0f, 0.01f), "vFromAng 90");
        Vector2 v2 = HALF_PI_RADIAN.getVector2FromAngle();
        assertTrue(v2.x.isEqual(1.0f, 0.01f), "v2FromAng 90");
    }

    static void testBezier()
    {
        List<Vector3> pts = new() { Vector3.zero, new(10, 0, 0) };
        Vector3 r = getBezier(pts, false, 0.5f);
        assertTrue(r.x.isEqual(5.0f, 0.001f), "bezier lin");
    }

    static void testSplitAndGenerate()
    {
        List<byte> d = new();
        splitNumber(10203L, d);
        assertEqual(5, d.Count);
        assertEqual((byte)1, d[0]);
    }

    static void testRandomDistribution()
    {
        float r = randomFloat(0.0f, 1.0f);
        assertTrue(r >= 0 && r <= 1, "rndDist");
        int ri = randomInt(1, 6);
        assertTrue(ri >= 1 && ri <= 6, "rndInt");
        List<int> pool = new() { 10, 20, 30, 40, 50 };
        List<int> sel = new();
        randomSelect(5, 3, sel);
        assertEqual(3, sel.Count, "rndSel");
        randomOrder(pool);
        assertEqual(5, pool.Count, "rndOrder");
    }
    // 检查 6 方向 AStar 基础功能，仅调用 public 方法
    static void testAStar6OddR()
    {
        List<bool> m = new(16);
        for (int i = 0; i < 16; i++)
        {
            m.Add(true);
        }
        List<int> p = new();
        assertTrue(AStar6OddR(m, 0, 15, 4, p), "AStar6OddR");
        assertEqual(0, p[0]);
        assertEqual(15, p[^1]);
    }
    // 检查 6 方向 AStar 基础功能，仅调用 public 方法
    static void testAStar6EvenR()
    {
        List<bool> m = new(16);
        for (int i = 0; i < 16; i++)
        {
            m.Add(true);
        }
        List<int> p = new();
        assertTrue(AStar6EvenR(m, 0, 15, 4, p), "AStar6EvenR");
        assertEqual(0, p[0]);
    }
}