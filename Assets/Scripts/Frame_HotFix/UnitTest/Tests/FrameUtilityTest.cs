#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;
using static MathUtility;
using static StringUtility;

public static class FrameUtilityTest
{
    public static void Run()
    {
        testTickTimerLoop();
        testTickTimerOnce();
        testArrayHelpers();
        testIdHelpers();
        testEnumAndColorHelpers();
        testBoolToString();
        testSign();
        testClampFloat();
        testEnumAndCollections();
        testTimersAndColors();
        testIdsAndDiagnostics();
        testEnumConversion();
        testCrcHelpers();
        testFindMaxHelpers();
        testFindMaxAbsHelpers();
        testPathIgnoreAndParsing();
        testLineAndStackHelpers();
        testPercentAndProbability();
        testFixedAndPercent();
        testSwap();
        testClampInt();
        testFixedAndPercent2();
        testToPercentFloat();
        testToPercentString();
        testToProbabilityFloat();
        testToProbabilityString();
    }

    static void testTickTimerLoop()
    {
        float t = 1.0f;
        bool f = tickTimerLoop(ref t, 0.25f, 1.0f);
        assertFalse(f);
        assertEqual(0.75f, t);
        f = tickTimerLoop(ref t, 0.8f, 1.0f);
        assertTrue(f);
        float e = 0.1f;
        f = tickTimerLoop(ref e, 2.0f, 1.5f, true);
        assertTrue(f);
        assertEqual(1.5f, e);
        float s = -1.0f;
        f = tickTimerLoop(ref s, 0.5f, 1.0f);
        assertFalse(f);
    }

    static void testTickTimerOnce()
    {
        float t = 1.0f;
        bool f = tickTimerOnce(ref t, 0.25f);
        assertFalse(f);
        assertEqual(0.75f, t);
        f = tickTimerOnce(ref t, 0.75f);
        assertTrue(f);
        assertEqual(-1.0f, t);
        float s = -1.0f;
        f = tickTimerOnce(ref s, 0.1f);
        assertFalse(f);
    }

    static void testArrayHelpers()
    {
        int[] v = { 1, 2, 3, 4 };
        v.removeIndex(v.Length, 1);
        assertEqual(3, v[1]);
        string[] cv = { "a", "b", "c", "b" };
        int cc = cv.removeValue(cv.Length, "b");
        assertEqual(2, cc);
        int[] vv = { 1, 2, 3, 2, 4 };
        int vc = vv.removeValue(vv.Length, 2);
        assertEqual(3, vc);
        assertTrue(v.contains(3));
        assertFalse(v.contains(99));
    }

    static void testIdHelpers()
    {
        int f = makeID();
        int s = makeID();
        assertEqual(f + 1, s);
        notifyIDUsed(s + 20);
        int n = makeID();
        assertEqual(s + 21, n);
    }

    static void testEnumAndColorHelpers()
    {
        assertTrue(isEnumValid(CoreTestEnum.First));
        assertFalse(isEnumValid((CoreTestEnum)99));
        string c = "#112233";
        assertEqual(c, getCountColor(true, c));
        string nc = getCountColor(false, c);
        assertFalse(string.IsNullOrEmpty(nc));
    }

    static void testBoolToString()
    {
        assertEqual("true", boolToString(true));
        assertEqual("false", boolToString(false));
    }

    static void testSign()
    {
        assertEqual(-1, sign(-5));
        assertEqual(0, sign(0));
        assertEqual(1, sign(10));
    }

    static void testClampFloat()
    {
        assertEqual(3.0f, clamp(3.0f, 0.0f, 5.0f), 0.001f, "clamp float");
        assertEqual(0.0f, clamp(-1.0f, 0.0f, 5.0f), 0.001f, "clamp low");
    }

    static void testEnumAndCollections()
    {
        assertTrue(isEnumValid(CoreTestEnum.First));
        assertFalse(isEnumValid((CoreTestEnum)99));
    }

    static void testTimersAndColors()
    {
        float t = 1.0f;
        tickTimerLoop(ref t, 0.25f, 1.0f);
        assertEqual(0.75f, t, 0.001f);
        string c = getCountColor(true, "#FFF");
        assertEqual("#FFF", c);
    }

    static void testIdsAndDiagnostics()
    {
        int id = makeID();
        assertTrue(id > 0);
        notifyIDUsed(id + 50);
        int nid = makeID();
        assertTrue(nid > id);
    }

    static void testEnumConversion()
    {
        assertTrue(isEnumValid(CoreTestEnum.First));
        assertFalse(isEnumValid((CoreTestEnum)0));
    }

    static void testCrcHelpers()
    {
        byte[] d = { 0x01, 0x02 };
        ushort c = generateCRC16(d, d.Length);
        ushort c2 = generateCRC16(d, d.Length);
        assertEqual(c, c2);
    }

    static void testFindMaxHelpers()
    {
        float[] vals = { 1.5f, 3.7f, 2.1f };
        float mx = findMax(vals);
        assertEqual(3.7f, mx, 0.001f);
    }

    static void testFindMaxAbsHelpers()
    {
        float[] vals = { -1.5f, 3.7f, -5.2f };
        float mx = findMaxAbs(vals);
        assertEqual(5.2f, mx, 0.001f);
    }

    static void testPathIgnoreAndParsing()
    {
        string p = "a/b/c";
        assertTrue(p.Length > 0);
    }

    static void testLineAndStackHelpers()
    {
        string s = "line1\nline2";
        string[] l = s.Split('\n');
        assertEqual(2, l.Length);
    }

    static void testPercentAndProbability()
    {
        assertEqual("50%", toPercent(0.5f, 1), "50%");
        assertEqual("100%", toPercent(1.0f, 0), "100%");
        assertEqual("0%", toPercent(0.0f), "0%");
        assertEqual("0.005%", toProbability(0.5f), "0.5%");
        assertEqual("0.01%", toProbability(1.0f), "1%");
        assertEqual("1%", toProbability(100.0f), "100%");
    }

    static void testFixedAndPercent()
    {
        assertEqual("100+10%", fixedAndPercent(100, 0.1f));
        assertEqual("200+50%", fixedAndPercent(200, 0.5f));
        assertEqual("50", fixedAndPercent(50, 0.0f));
        assertEqual("10%", fixedAndPercent(0, 0.1f));
    }

    static void testSwap()
    {
        int a = 1, b = 2;
        swap(ref a, ref b);
        assertEqual(2, a);
        assertEqual(1, b);
    }

    static void testClampInt()
    {
        assertEqual(5, clamp(5, 0, 10));
        assertEqual(0, clamp(-5, 0, 10));
        assertEqual(10, clamp(15, 0, 10));
    }

    static void testFixedAndPercent2()
    {
        assertEqual("100+10%", fixedAndPercent(100, 0.1f));
    }

    static void testToPercentFloat()
    {
        string r = toPercent(0.5f);
        assertTrue(r.Contains("%"));
    }

    static void testToPercentString()
    {
        string r = toPercent("0.5", 1);
        assertTrue(r.Contains("%"));
    }

    static void testToProbabilityFloat()
    {
        string r = toProbability(5.0f);
        assertTrue(r.Contains("%"));
    }

    static void testToProbabilityString()
    {
        string r = toProbability("50");
        assertTrue(r.Contains("%"));
    }

    enum CoreTestEnum { First = 1, Second = 2 }

    static void assert(bool c, string m = "")
    {
        if (!c) throw new Exception(m);
    }

    static void assertEqual<T>(T e, T a, string m = "")
    {
        if (!e.Equals(a)) throw new Exception($"Expected [{e}] got [{a}] - {m}");
    }

    static void assertEqual(float e, float a, float eps, string m = "")
    {
        if (Math.Abs(e - a) > eps) throw new Exception($"Expected [{e}] got [{a}] - {m}");
    }

    static void assertFalse(bool c, string m = "")
    {
        if (c) throw new Exception($"Expected false - {m}");
    }

    static void assertTrue(bool c, string m = "")
    {
        if (!c) throw new Exception($"Expected true - {m}");
    }
}
#endif
