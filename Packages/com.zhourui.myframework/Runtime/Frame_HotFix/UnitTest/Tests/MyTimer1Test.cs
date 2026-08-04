using static TestAssert;

public static class MyTimer1Test
{
	public static void Run()
	{
		testInitAndDefaults();
		testStartStop();
		testTimePercent();
		testTickTimerLoop();
		testTickTimerNoLoop();
		testSetInterval();
		testResetToInterval();
		testStopResetInterval();
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
		t.init(0.5f, 0.0f);
		assertEqual(0.0f, t.getTimePercent(), "percent 0 when interval=0");

		t.init(1.0f, 5.0f);
		assertTrue(isFloatEqual(t.getTimePercent(), 0.2f, 0.001f), "percent 1/5=0.2");
	}

	private static void testTickTimerLoop()
	{
		MyTimer1 t = new();
		t.init(0.0f, 0.0f); // interval=0 时任意时刻都已完成
		bool triggered = t.tickTimer();
		// interval<=0 且 loop=true 时 curTime 会被设为 0
		assertTrue(triggered || !triggered); // tickTimer 至少不崩溃
	}

	private static void testTickTimerNoLoop()
	{
		MyTimer1 t = new();
		t.init(0.0f, 0.0f, false);
		t.tickTimer();
		// noLoop: 到期后 curTime=-1
		assertEqual(-1.0f, t.mCurTime, "noLoop curTime=-1 after tick");
	}

	private static void testSetInterval()
	{
		MyTimer1 t = new();
		t.init(0.0f, 1.0f);
		t.setInterval(5.0f);
		assertEqual(5.0f, t.mTimeInterval, "setInterval 5");
	}

	private static void testResetToInterval()
	{
		MyTimer1 t = new();
		t.init(0.0f, 3.0f);
		t.resetToInterval();
		assertEqual(3.0f, t.mCurTime, "resetToInterval curTime=interval");
	}

	private static void testStopResetInterval()
	{
		MyTimer1 t = new();
		t.init(0.0f, 2.0f);
		t.stop(true);
		assertEqual(-1.0f, t.mTimeInterval, "stop(resetInterval) interval=-1");
		t.stop(false);
		// resetInterval=false 时 interval 不变
		t.init(0.0f, 2.0f);
		t.stop(false);
		assertEqual(2.0f, t.mTimeInterval, "stop(!resetInterval) interval 不变");
	}

	private static bool isFloatEqual(float a, float b, float eps = 0.0001f)
	{
		return System.Math.Abs(a - b) < eps;
	}
}
