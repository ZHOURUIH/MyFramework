using System.Threading;
using static TestAssert;

// MyThread线程封装单测
//
// 覆盖范围:
//   - 未启动状态与空安全。
//   - Event模式没有wake时必须保持休眠。
//   - wake后立即执行一轮回调。
//   - stop必须先停止再唤醒,无限等待中的Event线程也能正常退出,且不能额外执行业务回调。
//   - Event超时等待能够自动执行。
//   - 同一个MyThread停止后重新启动Event模式时,不能消费上一次stop残留的信号。
//   - 原有普通周期线程仍可正常执行并自然结束。
public static class MyThreadTest
{
	private const int WAIT_TIMEOUT_MS = 1000;

	public static void Run()
	{
		testIsFinishedDefault();
		testSetBackground();
		testStopWhenNotStarted();
		testEventThreadWaitsForWake();
		testEventThreadStopWakesAndDoesNotExecuteCallback();
		testEventThreadTimeout();
		testEventThreadRestartHasNoStaleWake();
		testNormalThreadStillWorks();
	}

	// ─── isFinished: 未start时mFinish默认为true ───────────────────────────────────────────────
	private static void testIsFinishedDefault()
	{
		MyThread thread = new("test-thread-default");
		assertTrue(thread.isFinished(), "未start的线程isFinished应为true");
		assertFalse(thread.isEventMode(), "未start的线程不应处于Event模式");
		assertEqual(-1, thread.getEventWaitTime(), "Event默认等待时间应为-1");
		thread.destroy();
	}

	// ─── setBackground: mThread==null时只修改配置 ─────────────────────────────────────────────
	private static void testSetBackground()
	{
		MyThread thread = new("test-thread-background");
		thread.setBackground(false);
		thread.setBackground(true);
		thread.destroy();
	}

	// ─── stop: 未启动线程直接返回 ─────────────────────────────────────────────────────────────
	private static void testStopWhenNotStarted()
	{
		MyThread thread = new("test-thread-not-started");
		thread.stop();
		thread.destroy();
	}

	// ─── Event线程: 没有wake绝不能执行; wake后执行一轮 ───────────────────────────────────────
	private static void testEventThreadWaitsForWake()
	{
		MyThread thread = new("test-thread-event-wake");
		using ManualResetEventSlim callbackEvent = new(false);
		int callbackCount = 0;

		thread.startEvent((ref bool run) =>
		{
			Interlocked.Increment(ref callbackCount);
			callbackEvent.Set();
		});

		// Event线程启动以后应阻塞在WaitOne,没有wake时不能自己执行。
		assertFalse(callbackEvent.Wait(50), "Event线程没有wake时不应执行回调");
		assertEqual(0, Volatile.Read(ref callbackCount), "Event线程没有wake时回调次数应为0");

		thread.wake();
		assertTrue(callbackEvent.Wait(WAIT_TIMEOUT_MS), "wake后Event线程应及时执行回调");
		assertEqual(1, Volatile.Read(ref callbackCount), "单次wake应至少完成一轮Event线程回调");

		thread.destroy();
		assertTrue(thread.isFinished(), "Event线程destroy后应正常结束");
	}

	// ─── stop: 必须唤醒无限等待线程; stop的内部wake不能再执行业务回调 ────────────────────────
	private static void testEventThreadStopWakesAndDoesNotExecuteCallback()
	{
		MyThread thread = new("test-thread-event-stop");
		int callbackCount = 0;
		thread.startEvent((ref bool run) =>
		{
			Interlocked.Increment(ref callbackCount);
		});

		// 用辅助后台线程执行stop,这样即使未来退出逻辑回归为死锁,单测本身也能超时失败而不是永久卡死。
		using ManualResetEventSlim stopFinished = new(false);
		Thread stopThread = new(() =>
		{
			thread.stop();
			stopFinished.Set();
		});
		stopThread.IsBackground = true;
		stopThread.Start();

		bool finishedNormally = stopFinished.Wait(WAIT_TIMEOUT_MS);
		if (!finishedNormally)
		{
			// 测试失败时尝试外部wake进行清理,避免遗留测试线程。
			thread.wake();
			stopFinished.Wait(WAIT_TIMEOUT_MS);
		}

		assertTrue(finishedNormally, "Event线程stop应主动唤醒WaitOne并正常结束,不能永久等待");
		assertTrue(thread.isFinished(), "stop完成后Event线程应处于完成状态");
		assertEqual(0, Volatile.Read(ref callbackCount), "stop用于退出的wake不能额外执行一次业务回调");

		thread.destroy();
	}

	// ─── Event超时模式: 没有wake也应在超时后执行 ────────────────────────────────────────────
	private static void testEventThreadTimeout()
	{
		MyThread thread = new("test-thread-event-timeout");
		using ManualResetEventSlim callbackEvent = new(false);
		int callbackCount = 0;

		thread.startEvent((ref bool run) =>
		{
			Interlocked.Increment(ref callbackCount);
			run = false;
			callbackEvent.Set();
		}, 20);

		assertTrue(callbackEvent.Wait(WAIT_TIMEOUT_MS), "Event线程设置超时时间后应在无wake时自动执行");
		assertTrue(waitUntil(() => thread.isFinished(), WAIT_TIMEOUT_MS), "Event超时回调将run置false后线程应自然结束");
		assertEqual(1, Volatile.Read(ref callbackCount), "Event超时模式本测试应只执行一次回调");

		thread.destroy();
	}

	// ─── Event线程重启: stop产生的Set不能污染下一次启动 ─────────────────────────────────────
	private static void testEventThreadRestartHasNoStaleWake()
	{
		MyThread thread = new("test-thread-event-restart");
		int callbackCount = 0;

		thread.startEvent((ref bool run) =>
		{
			Interlocked.Increment(ref callbackCount);
		});
		thread.stop();
		assertTrue(thread.isFinished(), "第一次Event线程stop后应正常结束");

		using ManualResetEventSlim secondCallbackEvent = new(false);
		thread.startEvent((ref bool run) =>
		{
			Interlocked.Increment(ref callbackCount);
			secondCallbackEvent.Set();
		});

		// startEvent内部必须Reset旧信号,否则第二次启动会在没有wake时立即执行。
		assertFalse(secondCallbackEvent.Wait(50), "Event线程重新启动后不能消费上一次stop残留的唤醒信号");
		assertEqual(0, Volatile.Read(ref callbackCount), "重新启动但未wake时回调次数应保持0");

		thread.wake();
		assertTrue(secondCallbackEvent.Wait(WAIT_TIMEOUT_MS), "重新启动后的Event线程仍应能被wake正常唤醒");
		assertEqual(1, Volatile.Read(ref callbackCount), "重新启动后的首次wake应执行一次回调");

		thread.destroy();
	}

	// ─── 普通线程: 新增Event模式不能破坏原来的周期线程 ──────────────────────────────────────
	private static void testNormalThreadStillWorks()
	{
		MyThread thread = new("test-thread-normal");
		using ManualResetEventSlim callbackEvent = new(false);
		int callbackCount = 0;

		thread.start((ref bool run) =>
		{
			Interlocked.Increment(ref callbackCount);
			run = false;
			callbackEvent.Set();
		}, 1, 1);

		assertTrue(callbackEvent.Wait(WAIT_TIMEOUT_MS), "普通MyThread仍应能够按原有周期模式执行回调");
		assertTrue(waitUntil(() => thread.isFinished(), WAIT_TIMEOUT_MS), "普通线程回调将run置false后应自然结束");
		assertEqual(1, Volatile.Read(ref callbackCount), "普通线程本测试应只执行一次回调");

		thread.destroy();
	}

	private static bool waitUntil(System.Func<bool> predicate, int timeoutMS)
	{
		int elapsed = 0;
		while (elapsed < timeoutMS)
		{
			if (predicate())
			{
				return true;
			}
			Thread.Sleep(1);
			++elapsed;
		}
		return predicate();
	}
}
