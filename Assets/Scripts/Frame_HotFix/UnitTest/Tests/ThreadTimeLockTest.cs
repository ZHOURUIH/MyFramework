using System;
using static TestAssert;

// ThreadTimeLock 线程锁帧测试
public static class ThreadTimeLockTest
{
	public static void Run()
	{
		testCreateAndUpdate();
		testSetForceSleep();
		testSetFrameTime();
		testGetFrameStartTime();
	}

	static void testCreateAndUpdate()
	{
		ThreadTimeLock timeLock = new ThreadTimeLock(50);
		assertNotNull(timeLock, "ThreadTimeLock should be created");

		double frameTime = timeLock.update();
		assertTrue(frameTime > 0, "update() should return a positive frame time");
	}

	static void testSetForceSleep()
	{
		ThreadTimeLock timeLock = new ThreadTimeLock(50);
		timeLock.setForceSleep(5);
		// No crash means setForceSleep works
		double frameTime = timeLock.update();
		assertTrue(frameTime > 0, "update() should still return a positive frame time after setForceSleep");
	}

	static void testSetFrameTime()
	{
		ThreadTimeLock timeLock = new ThreadTimeLock(50);
		timeLock.setFrameTime(100);
		double frameTime = timeLock.update();
		assertTrue(frameTime > 0, "update() should return a positive frame time after setFrameTime");
	}

	static void testGetFrameStartTime()
	{
		ThreadTimeLock timeLock = new ThreadTimeLock(50);
		DateTime startTime = timeLock.getFrameStartTime();
		assertTrue(startTime <= DateTime.Now, "Frame start time should not be in the future");
	}
}