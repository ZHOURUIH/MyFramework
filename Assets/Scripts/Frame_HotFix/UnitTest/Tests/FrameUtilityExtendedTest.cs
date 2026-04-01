#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameUtility;
using static TestAssert;

public static class FrameUtilityExtendedTest
{
    private enum SampleEnum { Zero = 0, One = 1, Two = 2, Four = 4 }

    public static void Run()
    {
        testEnumAndCollections();
        testTimersAndColors();
        testIdsAndDiagnostics();
    }

    private static void testEnumAndCollections()
    {
        assertEqual(1, enumToInt(SampleEnum.One), "enum to int");
        assertEqual(SampleEnum.Two, intToEnum<SampleEnum, int>(2), "int to enum");
        assertTrue(isEnumValid(SampleEnum.Four), "enum valid");
        assertEqual(4, findMax(new List<int> { 1, 4, 2 }), "find max");
        assertEqual(7, findMaxAbs(new List<int> { -3, 7, -5 }), "find max abs");
        int[] values = { 1, 2, 3, 4 };
        assertTrue(arrayContains(values, 3), "array contains");
        assertEqual(3, removeValueElement(values, 4, 2), "remove value element");
    }

    private static void testTimersAndColors()
    {
        float timer = 1.0f;
        assertFalse(tickTimerOnce(ref timer, 0.5f), "tick once false");
        assertTrue(tickTimerOnce(ref timer, 0.6f), "tick once true");

        timer = 1.0f;
        assertFalse(tickTimerLoop(ref timer, 0.2f, 1.0f, true), "loop not triggered");
        timer = 0.3f;
        assertTrue(tickTimerLoop(ref timer, 0.5f, 1.0f, true), "loop triggered");

        assertEqual("FFFFFF", getCountColor(true), "count color enough");
        assertEqual("FF0000", getCountColor(false), "count color not enough");
        assertEqual("ABCDEF", getCountColor(true, "ABCDEF"), "count color custom");
        assertTrue(generateCRC16(new byte[] { 1, 2, 3 }, 3) != 0, "frame crc");
    }

    private static void testIdsAndDiagnostics()
    {
        int id = makeID();
        notifyIDUsed(id);
        assertTrue(id > 0, "make id");
        assertTrue(isIgnorePath("a/b/c.txt", new List<string> { "/b/" }), "ignore path");
        assertTrue(getLineNum() > 0, "line num");
        assertTrue(getCurSourceFileName().Length > 0, "source file");
        assertTrue(getStackTrace(2).Length > 0, "stack trace");
    }
}
#endif
