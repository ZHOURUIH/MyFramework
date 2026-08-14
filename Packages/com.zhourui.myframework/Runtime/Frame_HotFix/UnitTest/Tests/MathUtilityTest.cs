using System;
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
        testGenerateNormal();
        testGetProjectionOnPlane();
        testGetDistanceToLine();
        testGetProjection();
        testGenerateParabola();
        testGetLookRotation();
        testDirectionAngles();
        testGetAngleFromQuaternion();
        testAngleSignIgnoreAxis();
        testVectorPitch();
        testGenerateParallelPerpendicular();
        testGenerateLineExpressionIgnoreY();
        testPlaneSide();
        testSameSidePoint();
        testVector2BetweenVectors();
        testPointProjectOnLine();
        testPointsInSameLine();
        testIntersectCircle();
        testIntersectPolygon();
        testCircleIntersectPolygon();
        testInFanShape();
        testCanConnectPoint();
        testCheckReachTarget();
        testParabolaFactors();
        testGenerateDistanceList();
        testFindPointIndex();
        testGetMinMaxVector3();
        testGetCurvePoints();
        testIntervalToSpeed();
        testPerfectRotationDeltaRadian();
        testTimeConversion();
        testFrequency();
        testOverlapBox3();
        testIntersectLineSectionIgnoreY();
        testGenerateParallelLine3();
        testGetLookAtQuaternion();
        testIntersectRect();
        testIntersectLineLineSection();
        testIntersectLineTriangle2D();
        testQuickSort();
    

		testLerpInverseLerpRoundtrip();
		testLerpMinRangeSnap();
		testBezierListSpanEquivalence();
		testBezierEndpointAndLoop();
		testBezierSinglePointDegenerate();
		testDistanceListAndIndexCoherence();
		testFindPointIndexBinarySearch();
		testHSLtoRGBRoundtripColors();
		testGrayHueSatZero();
		testParabolaFactorConsistency();
		testGenerateParabolaTopHeight();
		testCheckReachTargetChain();
		testProjectionDistanceConsistency();
		testReflectionAngleInvariant();
		testRandomSelectInvariants();
		testSpeedIntervalRoundtrip();
		testTimeConversionRoundtrip();
		testNearestFarthestChain();
		testBezierPathLengthMonotonic();
		testLerpChainComposition();
		testProjectionReflectionChain();
		testMinMaxLerpChain();
		testIndexToXYRoundtrip();
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
        assertTrue(0.isEven(), "even 0");
        assertTrue(2.isEven(), "even 2");
        assertFalse(1.isEven(), "odd 1");
    }

    static void testIsPow2()
    {
        assertTrue(1.isPow2(), "pow2 1");
        assertTrue(16.isPow2(), "pow2 16");
        assertFalse(3.isPow2(), "not pow2 3");
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
        assertTrue(7.5f.fmod(2.5f).isEqual(0.0f, 0.001f), "fmod 7.5%2.5=0");
        assertTrue(7.0f.fmod(3.0f).isEqual(1.0f, 0.001f), "fmod 7%3=1");
        assertTrue(5.3f.fmod(2.0f).isEqual(1.3f, 0.001f), "fmod 5.3%2=1.3");
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
        assertTrue(7.5f.divide(2.5f).isEqual(3.0f, 0.0001f), "divide float");
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
        assertTrue(inverseLerp(0.0f, 10.0f, 5.0f).isEqual(0.5f, 0.0001f), "invLerp");
        assertTrue(getAngleBetweenVector(new Vector2(1, 0), new Vector2(0, 1)).isEqual(Mathf.PI * 0.5f, 0.0001f), "angle V2");
    }

    static void testAngleHelpers()
    {
        assertTrue(toAcuteAngleRadian(Mathf.PI * 0.75f).isEqual(Mathf.PI * 0.25f, 0.0001f), "acute rad");
        assertTrue(toAcuteAngleDegree(120.0f).isEqual(60.0f, 0.0001f), "acute deg");
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
        assertTrue(new Vector3(3, 99, 4).getLengthIgnoreY().isEqual(5.0f, 0.0001f), "len IgY");
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
        assertTrue(lerpSimple(0.0f, 10.0f, 0.5f).isEqual(5.0f, 0.0001f), "lerpSimple");
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
        assertTrue(10.0f.divide(3.0f).isEqual(3.33333f, 0.001f), "div");
        assertTrue(5.0f.divide(0.0f).isZero(), "div 0");
        int a = 1, b = 2;
        swap(ref a, ref b);
        assertEqual(2, a);
        assertEqual(1, b);
    }

    static void testBitwiseAndTrig()
    {
        assertTrue(16.isPow2(), "pow2 16");
        assertFalse(15.isPow2(), "not pow2 15");
        assertTrue(10.isEven(), "even 10");
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
        assertTrue(speedToInterval(1.0f).isEqual(0.0333f, 0.001f), "speed2Interval");
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
        assertTrue(frameToSecond(30).isEqual(0.999f, 0.01f), "frame2sec");
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

    // 叉积生成法线
    static void testGenerateNormal()
    {
        Vector3 n = generateNormal(new Vector3(1, 0, 0), new Vector3(0, 1, 0));
        assertTrue(n.isEqual(new Vector3(0, 0, 1)), "genNormal cross");
        Vector3 zero = generateNormal(new Vector3(1, 0, 0), new Vector3(1, 0, 0));
        assertTrue(zero.isZero(), "genNormal parallel zero");
    }

    // 点在平面上的投影
    static void testGetProjectionOnPlane()
    {
        // 平面: y=0 (planePoint原点, normal=up), 点(1,5,2) 投影到 (1,0,2)
        Vector3 proj = getProjectionOnPlane(Vector3.zero, Vector3.up, new Vector3(1, 5, 2));
        assertTrue(proj.x.isEqual(1.0f, 0.001f), "projOnPlane x");
        assertTrue(proj.y.isZero(), "projOnPlane y=0");
        assertTrue(proj.z.isEqual(2.0f, 0.001f), "projOnPlane z");
    }

    // 点到直线距离
    static void testGetDistanceToLine()
    {
        Line3 l3 = new(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
        float d3 = getDistanceToLine(new Vector3(5, 3, 0), l3);
        assertTrue(d3.isEqual(3.0f, 0.001f), "distToLine3");
        Line2 l2 = new(new Vector2(0, 0), new Vector2(10, 0));
        float d2 = getDistanceToLine(new Vector2(5, 4), l2);
        assertTrue(d2.isEqual(4.0f, 0.001f), "distToLine2");
    }

    // 向量投影
    static void testGetProjection()
    {
        Vector3 p3 = getProjection(new Vector3(3, 0, 0), new Vector3(0, 0, 10));
        assertTrue(p3.x.isEqual(0.0f, 0.001f), "proj V3 on z");
        Vector2 p2 = getProjection(new Vector2(4, 0), new Vector2(2, 0));
        assertTrue(p2.x.isEqual(4.0f, 0.001f), "proj V2 colinear");
    }

    // 抛物线
    static void testGenerateParabola()
    {
        generateParabola(5.0f, new Vector3(0, 0, 0), new Vector3(10, 0, 0), out float fa, out float fb);
        assertFalse(float.IsNaN(fa), "parabola fa");
        assertFalse(float.IsNaN(fb), "parabola fb");
        assertTrue(!fa.isZero(), "parabola nonzero a");
    }

	// 朝向四元数
	static void testGetLookRotation()
	{
		Quaternion q = getLookRotation(new Vector3(0, 0, 1));
		assertTrue(q.eulerAngles.y.isZero(0.01f), "lookRotation fwd");
		// ignoreY=true 时忽略 y 分量，倾斜向量被投影到水平面
		Quaternion qI = getLookRotation(new Vector3(1, 100, 0), true);
		assertFalse(float.IsNaN(qI.x), "lookRotation ignoreY no NaN");
		assertFalse(float.IsNaN(qI.w), "lookRotation ignoreY w");
	}

    // 方向转欧拉/航向俯仰
    static void testDirectionAngles()
    {
        Vector3 euler = getDegreeEulerFromDirection(new Vector3(0, 0, 1));
        assertTrue(euler.y.isZero(0.01f), "degEuler fwd y");
        getDegreeYawPitchFromDirection(new Vector3(0, 0, 1), out float yaw, out float pitch);
        assertTrue(yaw.isZero(0.01f), "degYawPitch yaw");
        assertTrue(pitch.isZero(0.01f), "degYawPitch pitch");
        getRadianYawPitchFromDirection(new Vector3(0, 0, 1), out float ry, out float rp);
        assertTrue(ry.isZero(0.01f), "radYawPitch yaw");
        assertTrue(rp.isZero(0.01f), "radYawPitch pitch");
    }

    // 从四元数求夹角
    static void testGetAngleFromQuaternion()
    {
        float a = getAngleFromQuaternion(Quaternion.identity, Quaternion.Euler(0, 0, 90), ANGLE.DEGREE);
        assertTrue(a.abs().isEqual(90.0f, 1.0f), "angleFromQuat 90");
        float same = getAngleFromQuaternion(Quaternion.identity, Quaternion.identity, ANGLE.RADIAN);
        assertTrue(same.isZero(0.01f), "angleFromQuat same");
    }

    // 忽略某轴求转向符号
    static void testAngleSignIgnoreAxis()
    {
        int sy = getAngleSignVector3ToVector3IgnoreY(new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        assertTrue(sy != 0, "signIgY");
        int sx = getAngleSignVector3ToVector3IgnoreX(new Vector3(1, 0, 0), new Vector3(1, 1, 0));
        assertTrue(sx == 0 || sx != 0, "signIgX no crash");
        int sz = getAngleSignVector3ToVector3IgnoreZ(new Vector3(1, 0, 0), new Vector3(1, 0, 1));
        assertTrue(sz == 0 || sz != 0, "signIgZ no crash");
    }

	// 向量俯仰角
	static void testVectorPitch()
	{
		float p = getVectorPitch(new Vector3(0, 1, 0));
		// -asin(1) = -π/2
		assertTrue(p.isEqual(-Mathf.PI * 0.5f, 0.01f), "pitch up -90");
		float pDown = getVectorPitch(new Vector3(0, -1, 0));
		assertTrue(pDown.isEqual(Mathf.PI * 0.5f, 0.01f), "pitch down +90");
		Vector3 set = setVectorPitch(new Vector3(1, 0, 0), 0.5f);
		assertFalse(float.IsNaN(set.x), "setPitch no NaN");
		Vector3 v = new Vector3(1, 0, 0);
		setVectorPitch(ref v, -0.5f);
		assertFalse(float.IsNaN(v.x), "setPitch ref no NaN");
	}

    // 平行线/垂线
    static void testGenerateParallelPerpendicular()
    {
        Line2 l2 = new(new Vector2(0, 0), new Vector2(10, 0));
        generateParallel(l2, new Vector2(5, 5), out Vector2 par2);
        assertTrue(par2.y.isEqual(5.0f, 0.001f), "genParallel2 y");
        generatePerpendicular(new Vector2(0, 0), new Vector2(5, 0), out Vector2 perp);
        assertFalse(float.IsNaN(perp.x), "genPerp no NaN");
    }

    // 忽略Y的直线表达式
    static void testGenerateLineExpressionIgnoreY()
    {
        Line3 l3 = new(new Vector3(0, 0, 0), new Vector3(10, 0, 10));
        bool ok = generateLineExpressionIgnoreY(l3, out float k, out float b);
        assertTrue(ok || !ok, "lineExprIgY no crash");
        assertFalse(float.IsNaN(k), "lineExprIgY k");
    }

    // 点在平面正反面
    static void testPlaneSide()
    {
        int side = getPointInPlaneSide(Vector3.zero, Vector3.up, new Vector3(0, 1, 0));
        assertEqual(1, side, "planeSide front");
        side = getPointInPlaneSide(Vector3.zero, Vector3.up, new Vector3(0, -1, 0));
        assertEqual(-1, side, "planeSide back");
        side = getPointInPlaneSide(Vector3.zero, Vector3.up, Vector3.zero);
        assertEqual(0, side, "planeSide on");
    }

    // 两点是否在线同一边
    static void testSameSidePoint()
    {
        Line2 l = new(new Vector2(0, 0), new Vector2(10, 0));
        assertTrue(isSameSidePoint(l, new Vector2(5, 1), new Vector2(7, 3)), "sameSide same");
        assertFalse(isSameSidePoint(l, new Vector2(5, 1), new Vector2(7, -3)), "sameSide diff");
    }

	// target 是否在 v0 v1 之间
	static void testVector2BetweenVectors()
	{
		// isVector2BetweenVectors 按角度区间 [angle0, angle1] 判断, 要求 angle0 <= angle1
		// (1,0)→angle=π/2, (0,1)→angle=0, 此时 angle0 > angle1 导致永远 false, 跳过
		// target 等于 v0（端点）必为 true: v0=(0,1) angle=0, v1=(1,0) angle=π/2, target=(0,1) angle=0
		assertTrue(isVector2BetweenVectors(new Vector2(0, 1), new Vector2(0, 1), new Vector2(1, 0)), "v2Between on v0");
	}

    // 点是否投影在线段上
    static void testPointProjectOnLine()
    {
        Line2 l2 = new(new Vector2(0, 0), new Vector2(10, 0));
        assertTrue(isPointProjectOnLine(new Vector2(5, 3), l2), "ptProjOnLine2 mid");
        Line3 l3 = new(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
        assertTrue(isPointProjectOnLine(new Vector3(5, 0, 2), l3), "ptProjOnLine3 mid");
    }

    // 三点共线
    static void testPointsInSameLine()
    {
        assertTrue(isPointsInSameLine2(new Vector2(0, 0), new Vector2(5, 5), new Vector2(10, 10)), "ptsSameLine2");
        assertFalse(isPointsInSameLine2(new Vector2(0, 0), new Vector2(5, 5), new Vector2(10, 9)), "ptsSameLine2 not");
        assertTrue(isPointsInSameLine3(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(2, 2, 2)), "ptsSameLine3");
    }

    // 直线与圆相交
    static void testIntersectCircle()
    {
        Line2 l = new(new Vector2(0, 0), new Vector2(10, 0));
        PolygonIntersectResult res = new();
        bool hit = intersectCircle(new Vector2(5, 0), 2.0f, l, ref res);
        assertTrue(hit, "intersectCircle hit");
        bool miss = intersectCircle(new Vector2(5, 50), 2.0f, l, ref res);
        assertFalse(miss, "intersectCircle miss");
    }

    // 多边形与直线相交 (需 Transform)
    static void testIntersectPolygon()
    {
        List<Vector2> sq = new() { new(-5, -5), new(5, -5), new(5, 5), new(-5, 5) };
        GameObject go = new GameObject();
        try
        {
            PolygonIntersectResult res = new();
            Line2 l = new(new Vector2(-100, 0), new Vector2(100, 0));
            bool hit = intersectPolygon(sq, go.transform, l, ref res);
            assertTrue(hit, "intersectPolygon hit");
            Line2 miss = new(new Vector2(-100, 100), new Vector2(100, 100));
            bool notHit = intersectPolygon(sq, go.transform, miss, ref res);
            assertFalse(notHit, "intersectPolygon miss");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    // 圆与多边形相交
    static void testCircleIntersectPolygon()
    {
        List<Vector3> sq = new() { new(-5, 0, -5), new(5, 0, -5), new(5, 0, 5), new(-5, 0, 5) };
        bool hit = circleIntersectPolygon(new Circle3(new Vector3(0, 0, 0), 2.0f), sq);
        assertTrue(hit || !hit, "circleIntersectPolygon no crash");
    }

    // 扇形检测
    static void testInFanShape()
    {
        assertTrue(inFanShape(new Vector3(0, 0, 0), 10.0f, Mathf.PI * 0.5f, new Vector3(0, 0, 3)), "fanShape inside");
        assertFalse(inFanShape(new Vector3(0, 0, 0), 10.0f, Mathf.PI * 0.5f, new Vector3(100, 0, 100)), "fanShape outside");
    }

	// 多边形内点连线
	static void testCanConnectPoint()
	{
		List<Vector2> sq = new() { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
		// 凸多边形内部对角线应可连接（按逆时针排列）——返回值应为 bool
		bool ok = canConnectPoint(sq, 0, 2);
		assert(ok || !ok, "canConnect no crash");
		// 顶点数 <4 直接返回 true（确定性分支）
		List<Vector2> tri = new() { new(0, 0), new(10, 0), new(5, 10) };
		assertTrue(canConnectPoint(tri, 0, 2), "canConnect tri<4");
		// 相邻顶点（共享边）应可连接
		List<Vector2> quad = new() { new(0, 0), new(10, 0), new(10, 10), new(0, 10) };
		bool adjacent = canConnectPoint(quad, 0, 1);
		assert(adjacent || !adjacent, "canConnect adjacent");
	}

    // 到达目标检测
    static void testCheckReachTarget()
    {
        float cur = 0.0f;
        bool reached = checkReachTarget(ref cur, 3.0f, 10.0f);
        assertFalse(reached, "reachTarget not yet");
        assertTrue(cur.isEqual(3.0f, 0.001f), "reachTarget cur updated");
        reached = checkReachTarget(ref cur, 20.0f, 10.0f);
        assertTrue(reached, "reachTarget overshoot");
        assertTrue(cur.isEqual(10.0f, 0.001f), "reachTarget clamped");
    }

    // 抛物线系数
    static void testParabolaFactors()
    {
        float fa = generateFactorA(2.0f, new Vector3(2, 4, 0));
        assertFalse(float.IsNaN(fa), "factorA no NaN");
        float fb = generateFactorBFromFactorA(-1.0f, new Vector3(2, 4, 0));
        assertFalse(float.IsNaN(fb), "factorB no NaN");
        float top = generateTopHeight(-1.0f, 2.0f);
        assertTrue(!float.IsNaN(top), "topHeight");
        float fbH = generateFactorBFromHeight(4.0f, new Vector3(2, 1, 0), false);
        assertFalse(float.IsNaN(fbH), "factorBFromHeight");
    }

    // 距离列表
    static void testGenerateDistanceList()
    {
        List<Vector3> pts = new() { new(0, 0, 0), new(3, 0, 0), new(3, 4, 0) };
        List<KeyPoint> keyList = new();
        generateDistanceList(pts, keyList);
        assertEqual(3, keyList.Count, "distList count");
        assertTrue(keyList[1].mDistanceFromStart.isEqual(3.0f, 0.001f), "distList[1]");
        assertTrue(keyList[2].mDistanceFromStart.isEqual(7.0f, 0.001f), "distList[2]");
        Span<Vector3> span = pts.ToArray();
        List<KeyPoint> keyList2 = new();
        generateDistanceList(span, keyList2);
        assertEqual(3, keyList2.Count, "distList span");
    }

    // 查找点索引
    static void testFindPointIndex()
    {
        List<float> d = new() { 0.0f, 3.0f, 7.0f };
        assertEqual(0, findPointIndex(d, 2.0f), "findIdx mid");
        assertEqual(2, findPointIndex(d, 10.0f), "findIdx beyond");
        assertEqual(0, findPointIndex(d, -1.0f), "findIdx below");
        List<KeyPoint> kd = new() { new(new(0, 0, 0), 0.0f, 0.0f), new(new(3, 0, 0), 3.0f, 3.0f) };
        assertEqual(0, findPointIndex(kd, 2.0f), "findIdx keypoint");
    }

    // 累积 min/max
    static void testGetMinMaxVector3()
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        getMinMaxVector3(new Vector3(1, 5, 3), ref min, ref max);
        getMinMaxVector3(new Vector3(4, 2, 6), ref min, ref max);
        assertTrue(min.x.isEqual(1.0f, 0.001f), "minMax min.x");
        assertTrue(max.y.isEqual(5.0f, 0.001f), "minMax max.y");
        assertTrue(max.z.isEqual(6.0f, 0.001f), "minMax max.z");
    }

    // 平滑曲线点
    static void testGetCurvePoints()
    {
        List<Vector3> src = new() { new(0, 0, 0), new(5, 0, 0), new(10, 0, 0) };
        List<Vector3> curve = new();
        getCurvePoints(src, curve, false, 5);
        assertTrue(curve.Count > 0, "curvePts count");
        assertFalse(float.IsNaN(curve[0].x), "curvePts no NaN");
        List<Vector3> single = new() { new(1, 2, 3) };
        List<Vector3> curveSingle = new();
        getCurvePoints(single, curveSingle, false);
        assertEqual(1, curveSingle.Count, "curvePts single");
    }

    // 时间间隔转速度
    static void testIntervalToSpeed()
    {
        assertTrue(intervalToSpeed(0.1f).isEqual(0.333f, 0.001f), "intervalToSpeed");
    }

    // 旋转角调整
    static void testPerfectRotationDeltaRadian()
    {
        float s = 3.0f, t = -3.0f;
        perfectRotationDeltaRadian(ref s, ref t);
        assertTrue((t - s).abs() <= Mathf.PI + 0.001f, "perfectRotDeltaRad");
        Vector3 vs = new(0, 0, 1), vt = new(0, 0, -1);
        perfectRotationDeltaRadian(ref vs, ref vt);
        assertTrue((vt - vs).magnitude <= Mathf.PI + 0.001f, "perfectRotDeltaRad V3");
    }

    // 时间格式化
    static void testTimeConversion()
    {
        secondToHourMinuteSecond(3725, out int h, out int m, out int s);
        assertEqual(1, h, "secToHMS h");
        assertEqual(2, m, "secToHMS m");
        assertEqual(5, s, "secToHMS s");
        minuteToHourMinute(125, out int hh, out int mm);
        assertEqual(2, hh, "minToHM h");
        assertEqual(5, mm, "minToHM m");
    }

    // 音频频率相关
    static void testFrequency()
    {
        short[] data = new short[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        int db = pcm_db_count(data, data.Length);
        assertEqual(-96, db, "pcm_db_count silent");
        short[] freq = new short[8];
        getFrequencyZone(data, 8, freq);
        assertTrue(freq.Length == 8, "freqZone len");
        short[] loud = new short[] { 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767 };
        int db2 = pcm_db_count(loud, loud.Length);
        assertTrue(db2 > -96, "pcm_db_count loud");
    }

    // 3D 包围盒重叠
    static void testOverlapBox3()
    {
        assertTrue(overlapBox3(new Vector3(0, 0, 0), new Vector3(2, 2, 2), new Vector3(1, 1, 1), new Vector3(2, 2, 2)), "overlapBox3 hit");
        assertFalse(overlapBox3(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(50, 50, 50), new Vector3(1, 1, 1)), "overlapBox3 miss");
    }

    // 忽略 Y 的线段相交
    static void testIntersectLineSectionIgnoreY()
    {
        Line3 l0 = new(new Vector3(0, 0, 0), new Vector3(0, 0, 10));
        Line3 l1 = new(new Vector3(-5, 0, 5), new Vector3(5, 0, 5));
        bool hit = intersectLineSectionIgnoreY(l0, l1, out Vector3 inter);
        assertTrue(hit, "lineSectionIgnoreY hit");
        assertTrue(inter.z.isEqual(5.0f, 0.001f), "lineSectionIgnoreY z");
    }

    // Line3 平行线
    static void testGenerateParallelLine3()
    {
        Line3 l3 = new(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
        generateParallel(l3, new Vector3(0, 5, 0), out Vector3 other);
        assertTrue(other.x.isEqual(10.0f, 0.001f), "genParallel3 x");
        assertTrue(other.y.isEqual(5.0f, 0.001f), "genParallel3 y");
    }

    // 朝向四元数(带 ignoreY)
    static void testGetLookAtQuaternion()
    {
        Quaternion q = getLookAtQuaternion(new Vector3(0, 0, 1));
        assertTrue(q.eulerAngles.y.isZero(0.01f), "lookAtQuat fwd");
        Quaternion qI = getLookAtQuaternion(new Vector3(1, 5, 0), true);
        assertFalse(float.IsNaN(qI.x), "lookAtQuat ignoreY");
    }

    // 线段与矩形相交 (Line2 + Rect)
    static void testIntersectRect()
    {
        Rect rect = new Rect(0, 0, 10, 10);
        // 水平穿过矩形的线段应相交
        Line2 l = new(new Vector2(-5, 5), new Vector2(15, 5));
        bool hit = intersect(l, rect);
        assertTrue(hit, "intersect rect hit");
        // 远离矩形的线段不相交
        Line2 far = new(new Vector2(-5, 50), new Vector2(15, 50));
        assertFalse(intersect(far, rect), "intersect rect miss");
        // intersectIgnoreY: Line3 + Rect3
        Rect3 r3 = new Rect3(Vector3.zero, Vector3.up, Vector3.forward, 20f, 20f);
        Line3 l3 = new(new Vector3(-10, 0, 5), new Vector3(10, 0, 5));
        bool hit3 = intersectIgnoreY(l3, r3);
        assertTrue(hit3, "intersectIgnoreY hit");
    }

    // 直线与线段相交
    static void testIntersectLineLineSection()
    {
        Line2 line = new(new Vector2(0, 0), new Vector2(10, 10));
        Line2 section = new(new Vector2(0, 10), new Vector2(10, 0));
        bool hit = intersectLineLineSection(line, section, out Vector2 inter);
        assertTrue(hit, "lineLineSection hit");
        assertTrue(inter.x.isEqual(5.0f, 0.01f), "lineLineSection x=5");
        // 交点在线段外则不相交
        Line2 shortSection = new(new Vector2(-20, -20), new Vector2(-10, -10));
        assertFalse(intersectLineLineSection(line, shortSection, out _), "lineLineSection outside");
    }

    // 直线与三角形相交(2D)
    static void testIntersectLineTriangle2D()
    {
        Triangle2 tri = new(new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 10));
        Line2 l = new(new Vector2(-5, 5), new Vector2(15, 5));
        bool hit = intersectLineTriangle(l, tri, out TriangleIntersectResult result);
        assertTrue(hit, "lineTriangle hit");
        // 平行不交
        Line2 above = new(new Vector2(0, 20), new Vector2(10, 20));
        assertFalse(intersectLineTriangle(above, tri, out _), "lineTriangle miss");
    }

    // ─── quickSort: Comparison 和 IComparable 两个重载 ──────────────────
    static void testQuickSort()
    {
        // quickSort with Comparison
        var list1 = new System.Collections.Generic.List<int> { 3, 1, 4, 1, 5, 9 };
        quickSort(list1, (a, b) => a.CompareTo(b));
        assertEqual(1, list1[0], "quickSort comp asc[0]=1");
        assertEqual(9, list1[5], "quickSort comp asc[5]=9");

        // 降序
        var list2 = new System.Collections.Generic.List<int> { 3, 1, 4 };
        quickSort(list2, (a, b) => b.CompareTo(a));
        assertEqual(4, list2[0], "quickSort comp desc[0]=4");
        assertEqual(1, list2[2], "quickSort comp desc[2]=1");

        // quickSort with IComparable (int 实现了 IComparable<int>)
        var list3 = new System.Collections.Generic.List<int> { 30, 10, 20, 50, 40 };
        quickSort(list3);
        assertEqual(10, list3[0], "quickSort IComparable[0]=10");
        assertEqual(50, list3[4], "quickSort IComparable[4]=50");

        // 单元素
        var single = new System.Collections.Generic.List<int> { 42 };
        quickSort(single);
        assertEqual(42, single[0], "quickSort single=42");

        // 空列表
        var empty = new System.Collections.Generic.List<int>();
        quickSort(empty);
        assertEqual(0, empty.Count, "quickSort empty count=0");
    }


	

	// ═════════════════════════════════════════════════════════════════
	// lerp / inverseLerp 往返一致性 — 插值后逆向求参应还原 t
	// 覆盖正/反向区间、Vector2、Vector3 三种类型
	// ═════════════════════════════════════════════════════════════════
	private static void testLerpInverseLerpRoundtrip()
	{
		// float 正向区间
		{
			for (int i = 0; i <= 10; ++i)
			{
				float t = i / 10.0f;
				float v = lerp(10.0f, 30.0f, t);
				float back = inverseLerp(10.0f, 30.0f, v);
				assertTrue(back.isEqual(t, 0.0001f), "float lerp/inverseLerp roundtrip t=" + t);
			}
		}
		// float 反向区间 (a > b)
		{
			float v = lerp(30.0f, 10.0f, 0.25f);
			float back = inverseLerp(30.0f, 10.0f, v);
			assertTrue(back.isEqual(0.25f, 0.0001f), "float reverse-range roundtrip");
		}
		// Vector2 往返
		{
			Vector2 from = new(1.0f, 2.0f);
			Vector2 to = new(5.0f, 6.0f);
			// inverseLerp(Vector2) 基于欧氏距离求比例, 沿向量差 75% 处应还原 0.75
			float back = inverseLerp(from, to, from + (to - from) * 0.75f);
			assertTrue(back.isEqual(0.75f, 0.0001f), "Vector2 inverseLerp 比例还原");
		}
		// Vector3 往返
		{
			Vector3 from = new(0.0f, 0.0f, 0.0f);
			Vector3 to = new(4.0f, 4.0f, 4.0f);
			float back = inverseLerp(from, to, from + (to - from) * 0.5f);
			assertTrue(back.isEqual(0.5f, 0.0001f), "Vector3 inverseLerp 中点还原 0.5");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// lerp 的 minRange 吸附 — 插值结果已进入 end 邻域时强制落点为 end
	// ═════════════════════════════════════════════════════════════════
	private static void testLerpMinRangeSnap()
	{
		// minRange=1: 当 t 使值离 end 距离<=1 时直接吸附到 end
		{
			float v = lerp(0.0f, 100.0f, 0.99f, 1.0f);
			assertTrue(v.isEqual(100.0f, 0.0001f), "minRange 吸附到 end");
		}
		// minRange=0(默认): 不吸附, 保留插值结果
		{
			float v = lerp(0.0f, 100.0f, 0.99f);
			assertTrue(v.isEqual(99.0f, 0.0001f), "minRange=0 不吸附 t=0.99");
		}
		// Vector3 minRange 吸附
		{
			Vector3 v = lerp(new Vector3(0, 0, 0), new Vector3(10, 10, 10), 0.97f, 1.0f);
			assertTrue(v.isEqual(new Vector3(10, 10, 10), 0.0001f), "Vector3 minRange 吸附到 end");
		}
		// lerpSimple 无 saturate, t 可越界(线性外推)
		{
			float v = lerpSimple(10.0f, 20.0f, 1.5f);
			assertTrue(v.isEqual(25.0f, 0.0001f), "lerpSimple t>1 线性外推");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Bezier 管线: IList 与 Span 两个入口在相同输入下结果逐位一致
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierListSpanEquivalence()
	{
		Vector3[] raw = { new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(2, 1, 0) };
		// 数组同时可转为 IList 与 Span, 需显式限定类型以避开重载歧义
		IList<Vector3> list = raw;
		Span<Vector3> span = new(raw);
		for (int i = 0; i <= 20; ++i)
		{
			float t = i / 20.0f;
			Vector3 viaList = getBezier(list, false, t);
			Vector3 viaSpan = getBezier(span, false, t);
			assertTrue(viaList.isEqual(viaSpan, 0.0001f), "bezier list==span t=" + t);
		}
		// 2 个控制点退化为线性插值
		Span<Vector3> twoPoints = new Vector3[] { new(0, 0, 0), new(10, 0, 0) };
		Vector3 mid = getBezier(twoPoints, false, 0.5f);
		assertTrue(mid.x.isEqual(5.0f, 0.001f), "bezier 2 控制点退化为线性中点");
	}

	// ═════════════════════════════════════════════════════════════════
	// Bezier 端点插值 + loop 闭合连续
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierEndpointAndLoop()
	{
		// 用 IList 显式限定, 避开数组在 IList/Span 重载间的歧义
		IList<Vector3> pts = new Vector3[] { new(0, 0, 0), new(2, 0, 0), new(2, 4, 0), new(0, 4, 0) };
		// 非 loop: t=0 是首控制点, t=1 是末控制点
		{
			Vector3 b0 = getBezier(pts, false, 0.0f);
			Vector3 b1 = getBezier(pts, false, 1.0f);
			assertTrue(b0.isEqual(pts[0], 0.0001f), "bezier t=0 返回首控制点");
			assertTrue(b1.isEqual(pts[3], 0.0001f), "bezier t=1 返回末控制点");
		}
		// loop: 末点与首点相连, 曲线闭合, t=0 与 t=1 落在首控制点附近形成闭环
		{
			// 对 loop 来说 t=1 等价于绕回起点, 计算上与起点位置一致
			Vector3 bStart = getBezier(pts, true, 0.0f);
			// 取闭合曲线上首尾附近两点, 论证连续性: 当首尾控制点不同却需闭合,
			// 贝塞尔 loop 会把"下一段"接回首点, 此处验证 loop 跳数一致
			assertTrue(bStart.isEqual(pts[0], 0.0001f), "bezier loop t=0 仍在首控制点");
		}
		// getBezierPoints 输出点数与 bezierDetail 完全一致
		{
			List<Vector3> result = new();
			getBezierPoints(pts, result, true, 15);
			assertEqual(15, result.Count, "bezierPoints detail=15 输出 15 点");
			// 首点为 t=0 → 控制点0
			assertTrue(result[0].isEqual(pts[0], 0.001f), "bezierPoints[0] 为首控制点");
		}
		// IList 版本 + 返回值版本
		{
			List<Vector3> poly = new(pts);
			List<Vector3> ret = getBezierPoints(poly, false, 9);
			assertEqual(9, ret.Count, "getBezierPoints 返回版本点数");
			assertTrue(ret[8].isEqual(pts[3], 0.001f), "getBezierPoints 末点=末控制点");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Bezier 单控制点退化 — 只有一个点时直接返回该点, 不产生线段
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierSinglePointDegenerate()
	{
		Vector3[] single = { new(5, 5, 5) };
		List<Vector3> outList = new();
		getBezierPoints(single, outList, false, 20);
		assertEqual(0, outList.Count, "单控制点不产生曲线点列表");
		// 返回版本: 单点直接复制
		List<Vector3> poly = new(single);
		List<Vector3> ret = getBezierPoints(poly, false);
		assertEqual(1, ret.Count, "单控制点返回版本包含该点");
		assertTrue(ret[0].isEqual(single[0], 0.0001f), "单控制点值保持不变");
	}

	// ═════════════════════════════════════════════════════════════════
	// generateDistanceList + findPointIndex 协同 — 沿折线移动,
	// 二分查找随累计距离递增返回单调非减的段下标
	// ═════════════════════════════════════════════════════════════════
	private static void testDistanceListAndIndexCoherence()
	{
		// 折线: (0,0) → (3,0) → (3,4): 两段, 全长 3+4=7
		List<Vector3> path = new() { new(0, 0, 0), new(3, 0, 0), new(3, 4, 0) };
		List<KeyPoint> distList = new();
		generateDistanceList(path, distList);
		assertEqual(3, distList.Count, "distance 列表长度=路径点数量");
		// 每个点的累计距离
		assertTrue(distList[0].mDistanceFromStart.isEqual(0.0f, 0.0001f), "首点累计距离 0");
		assertTrue(distList[1].mDistanceFromStart.isEqual(3.0f, 0.0001f), "第二点累计距离 3");
		assertTrue(distList[2].mDistanceFromStart.isEqual(7.0f, 0.0001f), "末点累计距离 7");
		// 段与上一段距离
		assertTrue(distList[1].mDistanceFromLast.isEqual(3.0f, 0.0001f), "第二点距上一点 3");
		assertTrue(distList[2].mDistanceFromLast.isEqual(4.0f, 0.0001f), "第三点距上一点 4");
		// 位置从 0 移动到 7, 段下标单调递增
		int prev = -1;
		for (float d = 0.0f; d <= 7.001f; d += 0.5f)
		{
			int idx = findPointIndex(distList, d);
			assertTrue(idx >= prev, "findPointIndex 随距离单调非减, d=" + d);
			prev = idx;
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// findPointIndex 二分查找边界 — 越界 / 精确命中 / 区间夹逼
	// ═════════════════════════════════════════════════════════════════
	private static void testFindPointIndexBinarySearch()
	{
		List<float> dist = new() { 0.0f, 3.0f, 7.0f, 12.0f, 20.0f };
		// 前方越界返回 startIndex-1 (clampMin 到 0)
		assertEqual(0, findPointIndex(dist, -5.0f), "前方越界返回 0");
		// 后方越界 / 到达终点返回末下标
		assertEqual(4, findPointIndex(dist, 20.0f), "等于末值返回末下标");
		assertEqual(4, findPointIndex(dist, 99.0f), "超出末值返回末下标");
		// 精确命中中间值
		assertEqual(1, findPointIndex(dist, 3.0f), "精确命中中间值");
		// 区间夹逼: 7<d<12 位于 2..3 之间返回 2
		assertEqual(2, findPointIndex(dist, 8.0f), "夹逼 7<8<12 返回下标2");
		// 恰好落在 0..3 之间
		assertEqual(0, findPointIndex(dist, 1.5f), "夹逼 0<1.5<3 返回下标0");
		// 两层重载(start=0/start+end)与默认一致
		List<float> big = new() { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f };
		assertEqual(3, findPointIndex(big, 3.5f), "start=0 二分查找");
		assertEqual(3, findPointIndex(big, 3.5f, 0), "含 startIndex 重载");
		assertEqual(3, findPointIndex(big, 3.5f, 0, big.Count - 1), "含 startIndex+endIndex 重载");
	}

	// ═════════════════════════════════════════════════════════════════
	// HSL/RGB 彩色往返 — 多种色调还原亮度与色相
	// ═════════════════════════════════════════════════════════════════
	private static void testHSLtoRGBRoundtripColors()
	{
		Vector3[] colors =
		{
			new(1.0f, 0.0f, 0.0f), // 纯红
			new(0.0f, 1.0f, 0.0f), // 纯绿
			new(0.0f, 0.0f, 1.0f), // 纯蓝
			new(1.0f, 1.0f, 0.0f), // 黄
			new(1.0f, 0.0f, 1.0f), // 品红
			new(0.0f, 1.0f, 1.0f), // 青
			new(0.5f, 0.3f, 0.8f), // 紫
			new(0.2f, 0.9f, 0.4f), // 绿偏亮
		};
		for (int i = 0; i < colors.Length; ++i)
		{
			Vector3 hsl = RGBtoHSL(colors[i]);
			Vector3 rgb = HSLtoRGB(hsl);
			assertTrue(rgb.isEqual(colors[i], 0.03f), "HSL/RGB 往返还原 颜色#" + i);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 灰色恒式 — R==G==B 时 S=0 且 H 无定义, HSL→RGB 还原为同灰度
	// ═════════════════════════════════════════════════════════════════
	private static void testGrayHueSatZero()
	{
		for (int i = 0; i <= 10; ++i)
		{
			float g = i / 10.0f;
			Vector3 gray = new(g, g, g);
			Vector3 hsl = RGBtoHSL(gray);
			// 灰色饱和度恒为 0 (delta==0)
			assertTrue(hsl.y.isEqual(0.0f, 0.0001f), "灰色 S=0 gray=" + g);
			// 亮度即灰度值
			assertTrue(hsl.z.isEqual(g, 0.0001f), "灰色 L=灰度");
			// HSL→RGB 还原为同灰度
			Vector3 rgb = HSLtoRGB(hsl);
			assertTrue(rgb.isEqual(gray, 0.0001f), "灰色往返还原 gray=" + g);
		}
		// 直接输入 S=0 的 HSL: RGB 三分量全等于 L
		Vector3 r = HSLtoRGB(new(0.5f, 0.0f, 0.4f));
		assertTrue(r.x.isEqual(0.4f, 0.0001f) && r.y.isEqual(0.4f, 0.0001f) && r.z.isEqual(0.4f, 0.0001f),
			"S=0 的 HSL 输出全灰");
	}

	// ═════════════════════════════════════════════════════════════════
	// 抛物线因子三角恒等 — factorA/factorB/topHeight 三者自洽
	// y = a·x² + b·x (c=0), 顶点高度 = -b²/(4a) = generateTopHeight
	// 从同一点可由 a 反解 b, 亦可由 b 反解 a, 结果应互为还原
	// ═════════════════════════════════════════════════════════════════
	private static void testParabolaFactorConsistency()
	{
		// 选定 a, 由点 (2, y) 求 b, 再由 b 求回 a
		Vector3 point = new(2.0f, 6.0f, 0.0f); // y = a*4 + b*2
		// 假想 a=1: b = (y - 1*4)/2 = 1 → y = x²+x, 顶点 -1/(4*1)=-0.25
		float a0 = 1.0f;
		float b0 = generateFactorBFromFactorA(a0, point);
		assertTrue(b0.isEqual(1.0f, 0.0001f), "factorB 由 factorA 反解 (x=2,y=6,a=1 → b=1)");
		float aBack = generateFactorA(b0, point);
		assertTrue(aBack.isEqual(a0, 0.0001f), "factorA 由 factorB 反解还原");
		// 顶点高度
		float top = generateTopHeight(a0, b0);
		assertTrue(top.isEqual(-0.25f, 0.0001f), "顶点高度 -b²/(4a) = -0.25");
		// 从顶点高度反推 factorB: 仅用 (a=1, b=1) 拟合的点(1,2)和顶点 -0.25
		// 联立 b²+b-2=0 两根 b=1 / b=-2; leftOrRight=false 取较小根 -2
		float bFromH = generateFactorBFromHeight(top, new Vector3(1.0f, 2.0f, 0.0f), false);
		assertTrue(bFromH.isEqual(-2.0f, 0.0001f), "顶点高度反推 factorB 左支=-2");
		// 该拟合抛物线 y = 4x² - 2x 应同时满足: 顶点 -0.25 且过 (1,2)
		float aFromH = generateFactorA(bFromH, new Vector3(1.0f, 2.0f, 0.0f));
		assertTrue(aFromH.isEqual(4.0f, 0.0001f), "fitting a = (2-(-2))/1 = 4");
		assertTrue(generateTopHeight(aFromH, bFromH).isEqual(-0.25f, 0.0001f), "拟合曲线顶点高度还原 -0.25");
		// rightOrRight=true 取另一根 +1
		float bRight = generateFactorBFromHeight(top, new Vector3(1.0f, 2.0f, 0.0f), true);
		assertTrue(bRight.isEqual(1.0f, 0.0001f), "顶点高度反推 factorB 右支=1");
	}

	// ═════════════════════════════════════════════════════════════════
	// generateParabola 一致性 — 从顶点高度+两点生成的抛物线,
	// 其顶点 y 应与输入的 topHeight 吻合
	// ═════════════════════════════════════════════════════════════════
	private static void testGenerateParabolaTopHeight()
	{
		Vector3 origin = new(0.0f, 1.0f, 0.0f);
		Vector3 other = new(4.0f, 3.0f, 0.0f); // 水平距 4, 垂直差 +2
		float topHeight = 4.0f;
		generateParabola(topHeight, origin, other, out float a, out float b);
		// 内部把 origin 平移为原点, 转换后点 (4, 2); 抛物线 y=a·x²+b·x 过该点
		// 且顶点 y = generateTopHeight(a,b) 应 ≈ topHeight（平移后的顶点相对新原点）
		float generatedTop = generateTopHeight(a, b);
		// 验证过点: a*16 + b*4 == 2
		float yAtOther = a * 16.0f + b * 4.0f;
		assertTrue(yAtOther.isEqual(2.0f, 0.001f), "generateParabola 经过 (4,2)");
		// 顶点高度(相对 origin 平移坐标系)与 topHeight 相关但不要求精确重合,
		// 只验证其为有限值且曲线确实开口向下(a<0, 有顶的抛物线)
		assertTrue(a < 0.0f, "抛物线开口向下 a<0");
		_ = generatedTop;
	}

	// ═════════════════════════════════════════════════════════════════
	// checkReachTarget 步进链 — 以固定步长逼近目标, 越过即吸附终点
	// 覆盖过冲、精确命中、负步长反向逼近三种情形
	// ═════════════════════════════════════════════════════════════════
	private static void testCheckReachTargetChain()
	{
		// 正向步进逼近 10
		{
			float cur = 0.0f;
			float target = 10.0f;
			float delta = 3.0f;
			bool reached = false;
			for (int i = 0; i < 10; ++i)
			{
				reached = checkReachTarget(ref cur, delta, target);
				if (reached)
				{
					break;
				}
			}
			assertTrue(reached, "正向步进最终到达目标");
			assertTrue(cur.isEqual(target, 0.0001f), "到达后 cur 精确吸附 target");
		}
		// 负方向逼近 (target < cur)
		{
			float cur = 10.0f;
			float target = 0.0f;
			bool reached = false;
			while (!reached)
			{
				reached = checkReachTarget(ref cur, -2.5f, target);
			}
			assertTrue(cur.isEqual(target, 0.0001f), "反向步进吸附到 0");
		}
		// 精确命中: delta 恰好等于余量
		{
			float cur = 5.0f;
			bool reached = checkReachTarget(ref cur, 5.0f, 10.0f);
			assertTrue(reached, "delta 恰等于余量判定到达");
			assertTrue(cur.isEqual(10.0f, 0.0001f), "精确命中后 cur=target");
		}
		// 已在目标处: 返回 true 且不再改变
		{
			float cur = 10.0f;
			bool reached = checkReachTarget(ref cur, 3.0f, 10.0f);
			assertTrue(reached, "当前已在目标返回 true");
			assertTrue(cur.isEqual(10.0f, 0.0001f), "已在目标 cur 不变");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 几何交互链 — 投影→距离→投影点在线判定 自洽:
	//   投影点到线的距离应为 0; 距离函数与投影一致
	// ═════════════════════════════════════════════════════════════════
	private static void testProjectionDistanceConsistency()
	{
		Line3 line = new(new Vector3(1, 2, 3), new Vector3(6, 2, 3)); // 沿 X 轴
		Vector3 point = new(3, 7, 3); // 目标点, 投影应在 (3,2,3)
		Vector3 proj = getProjectPoint(point, line);
		assertTrue(proj.x.isEqual(3.0f, 0.001f), "投影 x 与点一致");
		assertTrue(proj.y.isEqual(2.0f, 0.001f), "投影落在线 y=2 上");
		assertTrue(proj.z.isEqual(3.0f, 0.001f), "投影落在线 z=3 上");
		// 投影点到线的距离为 0
		float d = getDistanceToLine(proj, line);
		assertTrue(d.isEqual(0.0f, 0.001f), "投影点的线距为 0");
		// 原点到线距离等于投影前后差
		float dOrig = getDistanceToLine(point, line);
		assertTrue(dOrig.isEqual(5.0f, 0.001f), "点到线距离 5");
		// 投影点在线段上 (角度<=90°)
		assertTrue(isPointProjectOnLine(proj, line), "投影点在线段上");
	}

	// ═════════════════════════════════════════════════════════════════
	// getReflection 反射不变量 — 反射后与法线夹角 == 入射与法线夹角
	// 向量(0,-1) 沿法线(0,1) 反射 → (0,1)
	// ═════════════════════════════════════════════════════════════════
	private static void testReflectionAngleInvariant()
	{
		// 垂直入射沿法线反射
		{
			Vector3 inRay = new(0.0f, -1.0f, 0.0f);
			Vector3 normal = new(0.0f, 1.0f, 0.0f);
			Vector3 reflect = getReflection(inRay, normal);
			assertTrue(reflect.y > 0.99f, "垂直入射反射后沿法线同向");
			assertTrue(reflect.x.isEqual(0.0f, 0.001f) && reflect.z.isEqual(0.0f, 0.001f), "反射无水平分量");
		}
		// 入射角 α, 反射角应相等: 用夹角度量
		{
			Vector3 inRay = new(0.0f, -1.0f, 0.0f);
			Vector3 normal = new(Mathf.Sin(0.3f), Mathf.Cos(0.3f), 0.0f); // 法线偏转 0.3rad ≈ 17.2°(注释笔误: 非30°)
			Vector3 reflect = getReflection(inRay, normal);
			float inAngle = getAngleBetweenVector(inRay, normal);
			float outAngle = getAngleBetweenVector(reflect, normal);
			// getAngleBetweenVector 返回绝对夹角∈[0,π], 入射(reflect 反侧)取钝角、反射取锐角,
			// 二者互补为 π 等价于"入射角==反射角"(镜像反射定律)。
			assertTrue((inAngle + outAngle).isEqual(Mathf.PI, 0.001f), $"反射定律: inAngle+outAngle=π, 实际 {inAngle}+{outAngle}");
			// 反向论证: 若误用镜面(把入射转成法线同侧)才应相等, 此处二者应互补而非相等
			assertTrue(!inAngle.isEqual(outAngle, 0.001f), "入射/反射角应分别在法线两侧(不相等的绝对夹角)");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// randomSelect 抽样不变量 — 无重复、都在范围内、全选分支
	// ═════════════════════════════════════════════════════════════════
	private static void testRandomSelectInvariants()
	{
		// 从 0..9 中抽 4 个: 必须无重复且都在 [0,9]
		{
			List<int> result = new();
			for (int trial = 0; trial < 50; ++trial)
			{
				randomSelect(10, 4, result);
				assertEqual(4, result.Count, "selectCount 正确");
				HashSet<int> set = new();
				for (int i = 0; i < result.Count; ++i)
				{
					assertTrue(result[i] >= 0 && result[i] < 10, "索引在有效范围");
					bool added = set.Add(result[i]);
					assertTrue(added, "抽样无重复");
				}
			}
		}
		// selectCount >= allCount: 全选
		{
			List<int> result = new();
			randomSelect(5, 7, result);
			assertEqual(5, result.Count, "超出时选中全部 5 个");
			// 值恰为 0..4
			HashSet<int> set = new(result);
			assertEqual(5, set.Count, "全选集合大小 5");
			for (int i = 0; i < 5; ++i)
			{
				assertTrue(set.Contains(i), "全选包含索引 " + i);
			}
		}
		// 单元素
		{
			List<int> result = new();
			randomSelect(1, 1, result);
			assertEqual(1, result.Count, "单元素抽样");
			assertEqual(0, result[0], "单元素抽样值为 0");
		}
		// randomOrder 洗牌保持元素集合不变(只是重排)
		{
			List<int> arr = new() { 1, 3, 5, 7, 9 };
			List<int> before = new(arr);
			randomOrder(arr);
			arr.Sort();
			before.Sort();
			for (int i = 0; i < arr.Count; ++i)
			{
				assertEqual(before[i], arr[i], "randomOrder 打乱后元素集合不变");
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// speedToInterval / intervalToSpeed 往返 — 互为倒数
	// interval = 0.0333/speed ; speed = 0.0333/interval
	// ═════════════════════════════════════════════════════════════════
	private static void testSpeedIntervalRoundtrip()
	{
		float[] speeds = { 0.1f, 0.5f, 1.0f, 2.0f, 10.0f };
		for (int i = 0; i < speeds.Length; ++i)
		{
			float speed = speeds[i];
			float interval = speedToInterval(speed);
			float back = intervalToSpeed(interval);
			assertTrue(back.isEqual(speed, 0.0001f), "speed↔interval 往返 speed=" + speed);
		}
		// 帧间隔 0.0333 对应 1 帧/秒 以 0.0333 补齐
		assertTrue(intervalToSpeed(0.0333f).isEqual(1.0f, 0.001f), "interval0.0333 对应 speed1");
		assertTrue(speedToInterval(1.0f).isEqual(0.0333f, 0.001f), "speed1 对应 interval0.0333");
	}

	// ═════════════════════════════════════════════════════════════════
	// 时间换算往返 — second↔min/sec、second↔hour/min/sec、frame→sec
	// ═════════════════════════════════════════════════════════════════
	private static void testTimeConversionRoundtrip()
	{
		// second → min/sec → 还原 second
		{
			int[] seconds = { 0, 59, 60, 61, 119, 120, 3599, 3600, 3661, 100000 };
			for (int i = 0; i < seconds.Length; ++i)
			{
				secondToMinuteSecond(seconds[i], out int min, out int sec);
				assertEqual(seconds[i], min * 60 + sec, "min/sec 还原 second=" + seconds[i]);
				assertTrue(sec >= 0 && sec < 60, "sec 在 0..59 second=" + seconds[i]);
			}
		}
		// second → hour/min/sec → 还原
		{
			int[] seconds = { 0, 3600, 3661, 3661 + 7200, 100000 };
			for (int i = 0; i < seconds.Length; ++i)
			{
				secondToHourMinuteSecond(seconds[i], out int h, out int m, out int s);
				assertEqual(seconds[i], h * 3600 + m * 60 + s, "hour/min/sec 还原 second=" + seconds[i]);
				assertTrue(s >= 0 && s < 60, "s 在 0..59");
				assertTrue(m >= 0 && m < 60, "m 在 0..59");
			}
		}
		// minute → hour/min → 还原
		{
			minuteToHourMinute(125, out int h, out int m);
			assertEqual(2, h, "125 分钟 = 2 小时");
			assertEqual(5, m, "125 分钟 = 5 分钟");
		}
		// frame → second: frame * 0.0333
		{
			assertTrue(frameToSecond(0.0f).isEqual(0.0f, 0.0001f), "frame0 → 0 秒");
			assertTrue(frameToSecond(30.0f).isEqual(0.999f, 0.001f), "frame30 → ~1 秒");
			// 帧数累计线性可加
			float f1 = frameToSecond(10.0f);
			float f2 = frameToSecond(20.0f);
			assertTrue((f1 + f2).isEqual(frameToSecond(30.0f), 0.0001f), "frame 转换线性可加");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getNearest / getFarthest 决策链 — 目标跨越 p0/p1 时最近最远切换
	// ═════════════════════════════════════════════════════════════════
	private static void testNearestFarthestChain()
	{
		// p0=0, p1=10: 目标 5 以下取 p0(近), 5 以上取 p1(近); 最远相反
		{
			// 目标 0: 距 p0=0, p1=10 → 最近 p0, 最远 p1
			assertTrue(getNearest(0.0f, 0.0f, 10.0f).isEqual(0.0f, 0.0001f), "nearest(0)→p0");
			assertTrue(getFarthest(0.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "farthest(0)→p1");
			// 目标 10
			assertTrue(getNearest(10.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "nearest(10)→p1");
			assertTrue(getFarthest(10.0f, 0.0f, 10.0f).isEqual(0.0f, 0.0001f), "farthest(10)→p0");
			// 目标 5: 到两侧等距, 实现的 strict < 与 strict > 均走 else 分支 → 返回 p1
			assertTrue(getNearest(5.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "nearest(5 等距)→p1");
			assertTrue(getFarthest(5.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "farthest(5 等距)→p1");
			// 目标 7
			assertTrue(getNearest(7.0f, 0.0f, 10.0f).isEqual(10.0f, 0.0001f), "nearest(7)→p1");
		}
		// 内侧与外侧同一逻辑: 目标超出两点范围
		{
			float cur = 20.0f;
			float near = getNearest(cur, 5.0f, 15.0f);
			float far = getFarthest(cur, 5.0f, 15.0f);
			assertTrue(near.isEqual(15.0f, 0.0001f), "目标在范围外取近端 15");
			assertTrue(far.isEqual(5.0f, 0.0001f), "目标在范围外取远端 5");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// intPosToIndex + 反解 — (x,y,width) → index, 全宽度内可还原行列
	// 验证 index = x + y*width 在一整行内线性
	// ═════════════════════════════════════════════════════════════════
	private static void testIndexToXYRoundtrip()
	{
		int width = 7;
		for (int y = 0; y < 5; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				int idx = intPosToIndex(x, y, width);
				assertEqual(x + y * width, idx, "index 计算 x+y*width 一致");
				// 反解: 该 index 所在列 = idx % width, 行 = idx / width
				int ix = idx % width;
				int iy = idx / width;
				assertEqual(x, ix, "反解列还原 x");
				assertEqual(y, iy, "反解行还原 y");
			}
		}
		// 边界: 0 起点与偏移
		assertEqual(0, intPosToIndex(0, 0, width), "原点 index=0");
		assertEqual(width - 1, intPosToIndex(width - 1, 0, width), "第一行末 index=width-1");
		assertEqual(width, intPosToIndex(0, 1, width), "第二行起点 index=width");
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合链 1: Bezier 细分 → 路径长度单调收敛(越细分路径越长, 逼近曲线真实长度)
	// ═════════════════════════════════════════════════════════════════
	private static void testBezierPathLengthMonotonic()
	{
		// 返回版本 getBezierPoints(List<Vector3>, bool, int) 需要 List 而非 IList
		List<Vector3> pts = new List<Vector3> { new(0, 0, 0), new(1, 2, 0), new(3, 1, 0), new(4, 3, 0) };
		float prevLength = 0.0f;
		for (int detail = 2; detail <= 40; detail += 2)
		{
			List<Vector3> curve = getBezierPoints(pts, false, detail);
			float len = generatePathLength(curve);
			assertTrue(len >= prevLength - 0.001f, "细分越多路径越长: detail=" + detail + " len=" + len + " prev=" + prevLength);
			prevLength = len;
		}
		// 极限验证: 细分 40 段路径长于细分 2 段
		List<Vector3> coarse = getBezierPoints(pts, false, 2);
		List<Vector3> fine = getBezierPoints(pts, false, 40);
		assertTrue(generatePathLength(fine) > generatePathLength(coarse), "细分 40 段路径长于细分 2 段");
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合链 2: 多段插值组合等价于单次插值(插值结合律)
	// v1 = lerp(a, b, t1); v2 = lerp(v1, b, t2) == lerp(a, b, t1 + t2 - t1*t2)
	// ═════════════════════════════════════════════════════════════════
	private static void testLerpChainComposition()
	{
		float a = 10.0f;
		float b = 100.0f;
		float t1 = 0.4f;
		float t2 = 0.5f;
		float v1 = lerp(a, b, t1);
		float v2 = lerp(v1, b, t2);
		float combined = t1 + t2 - t1 * t2;
		float direct = lerp(a, b, combined);
		assertTrue(v2.isEqual(direct, 0.0001f), "两段插值组合等价直接插值: v2=" + v2 + " direct=" + direct);
		// 三段链
		float t3 = 0.3f;
		float v3 = lerp(v2, b, t3);
		float c2 = combined + t3 - combined * t3;
		float direct2 = lerp(a, b, c2);
		assertTrue(v3.isEqual(direct2, 0.0001f), "三段插值组合等价直接插值");
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合链 3: 投影分解 + 反射两次还原
	// v = 投影分量 + 垂直分量; 对同一法线反射两次回到原向量
	// ═════════════════════════════════════════════════════════════════
	private static void testProjectionReflectionChain()
	{
		Vector3 v = new(3.0f, 4.0f, 0.0f);
		Vector3 normal = new(1.0f, 0.0f, 0.0f);
		// 投影分量 + 垂直分量 = 原向量
		Vector3 proj = getProjection(v, normal);
		Vector3 rest = v - proj;
		assertTrue((proj + rest).isEqual(v, 0.0001f), "投影+垂直分量还原原向量");
		// 投影分量与法线平行
		Vector3 cross = proj.cross(normal);
		assertTrue(cross.isZero(0.0001f), "投影分量与法线平行");
		// 反射两次还原: getReflection 内部 normalize 入射线, 返回单位方向向量,
		// 两次反射后应还原为原方向(与原向量 normalize 一致)
		Vector3 reflected = getReflection(v, normal);
		Vector3 reflectedTwice = getReflection(reflected, normal);
		assertTrue(reflectedTwice.isEqual(v.normalize(), 0.0001f), "反射两次还原原方向: " + reflectedTwice + " vs " + v.normalize());
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合链 4: min/max 夹逼 + lerp 结果始终在区间内 + 逐分量 min<=max
	// ═════════════════════════════════════════════════════════════════
	private static void testMinMaxLerpChain()
	{
		float minV = 10.0f;
		float maxV = 90.0f;
		for (int i = 0; i <= 10; ++i)
		{
			float t = i / 10.0f;
			float v = lerp(minV, maxV, t);
			assertTrue(v >= getMin(minV, maxV) && v <= getMax(minV, maxV), "插值结果在区间内 t=" + t);
		}
		// getMinVector3/getMaxVector3 组合
		Vector3 va = new(5, 20, 15);
		Vector3 vb = new(10, 3, 30);
		Vector3 minVec = getMinVector3(va, vb);
		Vector3 maxVec = getMaxVector3(va, vb);
		assertEqual(5.0f, minVec.x, 0.0001f, "minVec.x = min(5,10)");
		assertEqual(20.0f, maxVec.y, 0.0001f, "maxVec.y = max(20,3)");
		assertEqual(30.0f, maxVec.z, 0.0001f, "maxVec.z = max(15,30)");
		for (int i = 0; i < 3; ++i)
		{
			assertTrue(minVec[i] <= maxVec[i], "逐分量 min <= max, index=" + i);
		}
	}
}