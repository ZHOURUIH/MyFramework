#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static TestAssert;

public static class MathUtilityExtendedTest
{
    public static void Run()
    {
        testClampAndCycles();
        testPowersAndAngles();
        testVectorMath();
    }

    private static void testClampAndCycles()
    {
        assertEqual(3, clamp(3, 1, 5), "clamp inside");
        assertEqual(1, clamp(-1, 1, 5), "clamp low");
        assertEqual(5, clamp(8, 1, 5), "clamp high");
        assertEqual(0, clampCycle(10, 0, 9, 10), "clamp cycle wrap");
        assertEqual(9, clampCycle(-1, 0, 9, 10), "clamp cycle negative");
        assertTrue(isFloatEqual(clamp(2.5f, 0.5f, 3.5f), 2.5f, 0.0001f), "float clamp");
        assertEqual(4, divideInt(9, 2), "divideInt");
        assertEqual(3L, divideLong(7L, 2L), "divideLong");
        assertTrue(isFloatEqual(divide(7.5f, 2.5f), 3.0f, 0.0001f), "divide float");
    }

    private static void testPowersAndAngles()
    {
        assertEqual(1, pow2(0), "pow2 zero");
        assertEqual(8, pow2(3), "pow2 three");
        assertEqual(1000, pow10(3), "pow10 three");
        assertEqual(1024, getGreaterPow2(513), "greater pow2");
        assertEqual(27, getGreaterPowValue(20, 3), "greater pow value");
        assertTrue(isFloatEqual(toDegree(toRadian(90.0f)), 90.0f, 0.0001f), "degree/radian");
        assertTrue(isFloatEqual(adjustAngle180(190.0f), -170.0f, 0.0001f), "adjust angle 180");
        assertTrue(isFloatEqual(adjustAngle360(-10.0f), 350.0f, 0.0001f), "adjust angle 360");
        float start = 10.0f;
        float target = 350.0f;
        perfectRotationDeltaDegree(ref start, ref target);
        assertTrue(Mathf.Abs(target - start) <= 180.0f, "perfect rotation delta");
    }

    private static void testVectorMath()
    {
        Vector2 v2 = new(3, 4);
        Vector3 v3 = new(1, 2, 2);
        assertTrue(isFloatEqual(getLength(v2), 5.0f, 0.0001f), "length v2");
        assertTrue(isFloatEqual(getLength(v3), 3.0f, 0.0001f), "length v3");
        assertTrue(isFloatEqual(dot(new Vector2(1, 2), new Vector2(3, 4)), 11.0f, 0.0001f), "dot v2");
        assertTrue(isFloatEqual(dot(new Vector3(1, 0, 0), new Vector3(0, 1, 0)), 0.0f, 0.0001f), "dot v3");
        assertEqual(new Vector3(0, 0, 1), cross(new Vector3(1, 0, 0), new Vector3(0, 1, 0)), "cross");
        assertTrue(isFloatEqual(getLength(normalize(new Vector2(1, 1))), 1.0f, 0.0001f), "normalize v2");
        assertTrue(isFloatEqual(inverseLerp(0.0f, 10.0f, 5.0f), 0.5f, 0.0001f), "inverse lerp float");
        assertTrue(isFloatEqual(lerpSimple(2.0f, 6.0f, 0.25f), 3.0f, 0.0001f), "lerp simple");
        assertTrue(isFloatEqual(getAngleBetweenVector(new Vector2(1, 0), new Vector2(0, 1)), Mathf.PI * 0.5f, 0.0001f), "angle between v2");
        assertEqual(new Vector3(0, 0, 1), generateNormal(new Vector3(1, 0, 0), new Vector3(0, 1, 0)), "generate normal");
    }
}
#endif
