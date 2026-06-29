using static TestAssert;

public static class MyTimer1Test
{
    public static void Run()
    {
        testInitAndDefaults();
        testStartStop();
        testTimePercent();
    }

    private static void testInitAndDefaults()
    {
        MyTimer1 t = new();
        assertFalse(t.isCounting(), "default not counting");
        assertEqual(-1.0f, t.mCurTime, "default curTime=-1");

        t.init(0.5f, 1.0f);
        assertTrue(t.isCounting(), "after init counting");
        assertEqual(0.5f, t.mCurTime, "init curTime");
        assertEqual(1.0f, t.mTimeInterval, "init interval");
        assertTrue(t.mLoop, "init loop=true");
    }

    private static void testStartStop()
    {
        MyTimer1 t = new();
        t.init(0.0f, 1.0f);
        assertTrue(t.isCounting(), "init counting");

        t.stop();
        assertFalse(t.isCounting(), "stop not counting");

        t.start();
        assertTrue(t.isCounting(), "start counting");
        assertTrue(t.mCurTime >= 0.0f, "start curTime>=0");
    }

    private static void testTimePercent()
    {
        MyTimer1 t = new();
        // interval <= 0 returns 0
        t.init(0.5f, 0.0f);
        assertEqual(0.0f, t.getTimePercent(), "percent 0 when interval=0");

        t.init(1.0f, 5.0f);
        assertTrue(isFloatEqual(t.getTimePercent(), 0.2f, 0.001f), "percent 1/5=0.2");
    }

    private static bool isFloatEqual(float a, float b, float eps = 0.0001f)
    {
        return System.Math.Abs(a - b) < eps;
    }
}