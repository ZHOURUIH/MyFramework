#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static TestAssert;

public static class MathUtilityAdvancedCoverageTest
{
	public static void Run()
	{
		testAngleHelpers();
		testVectorRounding();
		testComparisonHelpers();
		testTrigonometryAndProducts();
		testCalculateFloat();
		testCheckFloatAndInt();
		testSpatialOverlap();
		testPowerAndSplitHelpers();
	}

	private static void testAngleHelpers()
	{
		assertTrue(isFloatEqual(toAcuteAngleRadian(Mathf.PI * 0.75f), Mathf.PI * 0.25f, 0.0001f), "toAcuteAngleRadian should fold obtuse angles");
		assertTrue(isFloatEqual(toAcuteAngleRadian(Mathf.PI * 0.25f), Mathf.PI * 0.25f, 0.0001f), "toAcuteAngleRadian should keep acute angles");

		assertTrue(isFloatEqual(toAcuteAngleDegree(120.0f), 60.0f, 0.0001f), "toAcuteAngleDegree should fold obtuse degrees");
		assertTrue(isFloatEqual(toAcuteAngleDegree(45.0f), 45.0f, 0.0001f), "toAcuteAngleDegree should keep acute degrees");
	}

	private static void testVectorRounding()
	{
		Vector2 v2 = new(-1.2f, 2.2f);
		ceil(ref v2);
		assertEqual(new Vector2(-1.0f, 3.0f), v2, "ceil(Vector2) should round each component up");

		Vector3 v3 = new(1.9f, -1.1f, 0.0f);
		v3 = floor(v3);
		assertEqual(new Vector3(1.0f, -2.0f, 0.0f), v3, "floor(Vector3) should round each component down");

		Vector3 r3 = new(1.4f, 1.5f, -1.5f);
		r3 = round(r3);
		assertEqual(new Vector3(1.0f, 2.0f, -2.0f), r3, "round(Vector3) should round to nearest integer");

		Vector3 s3 = new(-2.0f, 0.5f, 3.0f);
		saturate(ref s3);
		assertEqual(new Vector3(0.0f, 0.5f, 1.0f), s3, "saturate(Vector3) should clamp each component");
	}

	private static void testComparisonHelpers()
	{
		assertEqual(3.0f, getNearest(4.2f, 3.0f, 9.0f), "getNearest should choose the closer point");
		assertEqual(9.0f, getFarthest(4.2f, 3.0f, 9.0f), "getFarthest should choose the farther point");
		assertEqual(1, sign((sbyte)7), "sign(sbyte) should report positive");
		assertEqual(-1, sign((long)-99), "sign(long) should report negative");
		assertEqual(0, sign((uint)5, (uint)5), "sign(uint,uint) should report equality");
		assertEqual(-1, sign((ulong)1, (ulong)3), "sign(ulong,ulong) should report ordering");
	}

	private static void testTrigonometryAndProducts()
	{
		assertTrue(isFloatEqual(sin(Mathf.PI * 0.5f), 1.0f, 0.0001f), "sin(pi/2) should be 1");
		assertTrue(isFloatEqual(cos(0.0f), 1.0f, 0.0001f), "cos(0) should be 1");
		assertTrue(isFloatEqual(tan(0.0f), 0.0f, 0.0001f), "tan(0) should be 0");
		assertTrue(isFloatEqual(atan(1.0f), Mathf.PI * 0.25f, 0.0001f), "atan(1) should be pi/4");
		assertTrue(isFloatEqual(atan2(1.0f, 0.0f), Mathf.PI * 0.5f, 0.0001f), "atan2 should match axis angle");
		assertTrue(isFloatEqual(sqrt(9.0f), 3.0f, 0.0001f), "sqrt should return the positive root");

		Vector3 a = new(1, 2, 3);
		Vector3 b = new(4, 5, 6);
		assertTrue(isFloatEqual(dot(a, b), 32.0f, 0.0001f), "dot(Vector3) should match manual multiplication");
		assertEqual(new Vector3(-3, 6, -3), cross(a, b), "cross(Vector3) should match the right-handed result");

		Vector2 p0 = new(0, 0);
		Vector2 p1 = new(4, 0);
		Vector2 p2 = new(4, 4);
		assertTrue(crossProduct(p0, p1, p2) > 0.0f, "crossProduct should report a left turn");
	}

	private static void testCalculateFloat()
	{
		assertTrue(isFloatEqual(calculateFloat("1"), 1.0f, 0.0001f), "calculateFloat should handle a single literal");
		assertTrue(isFloatEqual(calculateFloat("(1+2)*3"), 9.0f, 0.0001f), "calculateFloat should evaluate parentheses first");
		assertTrue(isFloatEqual(calculateFloat("-3+5"), 2.0f, 0.0001f), "calculateFloat should handle leading negative values");
		assertTrue(isFloatEqual(calculateFloat("2*(3+4)"), 14.0f, 0.0001f), "calculateFloat should handle nested parentheses");
	}

	private static void testCheckFloatAndInt()
	{
		float floatValue = 1.23456f;
		checkFloat(ref floatValue, 2);
		assertTrue(isFloatEqual(floatValue, 1.23f, 0.0001f), "checkFloat should trim to the requested precision");

		float nearInt = 9.99999f;
		checkInt(ref nearInt, 0.001f);
		assertTrue(isFloatEqual(nearInt, 10.0f, 0.0001f), "checkInt should snap to the nearest integer");

		Vector3 nearVector = new(2.9999f, -3.0001f, 4.0f);
		checkInt(ref nearVector, 0.001f);
		assertEqual(new Vector3(3.0f, -3.0f, 4.0f), nearVector, "checkInt(Vector3) should snap each component");
	}

	private static void testSpatialOverlap()
	{
		assertTrue(overlapBox2(new Vector2(0, 0), new Vector2(2, 2), new Vector2(1, 1), new Vector2(2, 2)), "overlapBox2 should detect intersection");
		assertFalse(overlapBox2(new Vector2(0, 0), new Vector2(1, 1), new Vector2(5, 5), new Vector2(1, 1)), "overlapBox2 should reject separated boxes");

		assertTrue(overlapBox3(new Vector3(0, 0, 0), new Vector3(2, 2, 2), new Vector3(1, 1, 1), new Vector3(2, 2, 2)), "overlapBox3 should detect intersection");
		assertFalse(overlapBox3(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(5, 5, 5), new Vector3(1, 1, 1)), "overlapBox3 should reject separated boxes");
	}

	private static void testPowerAndSplitHelpers()
	{
		assertEqual(1000, pow10(3), "pow10 should match table lookup");
		assertEqual(10000000000L, pow10Long(10), "pow10Long should match table lookup");
		assertTrue(isFloatEqual(pow2(5), 32.0f, 0.0001f), "pow2 should produce the corresponding power");
		assertTrue(isFloatEqual(inversePow10(2), 0.01f, 0.0001f), "inversePow10 should produce decimal inverse");

		List<byte> digits = new();
		splitNumber(10203L, digits);
		assertEqual(5, digits.Count, "splitNumber should emit one digit per number");
		assertEqual((byte)1, digits[0], "splitNumber first digit");
		assertEqual((byte)0, digits[1], "splitNumber second digit");
		assertEqual((byte)2, digits[2], "splitNumber third digit");
		assertEqual((byte)0, digits[3], "splitNumber fourth digit");
		assertEqual((byte)3, digits[4], "splitNumber fifth digit");
	}
}
#endif
