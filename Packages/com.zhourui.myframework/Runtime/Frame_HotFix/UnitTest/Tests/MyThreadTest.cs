using static TestAssert;

// MyThread 线程封装单测(未启动线程的状态/纯逻辑)
//
// 设计要点:
//   - MyThread 构造 new ThreadTimeLock(0) 无 registerLock 副作用, 安全局部实例化。
//   - 关键: 测试只调用 isFinished/setBackground/stop 等方法, **不调用 start()**,
//     因此不会启动真实线程, 无环境/平台依赖, 无 logError。
//   - isFinished() 返回 mFinish, 构造后 mFinish 默认为 true(未 start 即视为已完成)。
//   - setBackground(bool) 在 mThread==null(未 start)时仅改 mIsBackground 字段, 无副作用。
//   - stop() 在 mThread==null 时直接 return, 空安全。
public static class MyThreadTest
{
	public static void Run()
	{
		testIsFinishedDefault();
		testSetBackground();
		testStopWhenNotStarted();
	}

	// ─── isFinished: 未 start 时 mFinish 默认为 true ─────────────
	private static void testIsFinishedDefault()
	{
		MyThread thread = new MyThread("test-thread");
		assertTrue(thread.isFinished(), "未 start 的线程 isFinished 应为 true(mFinish 默认 true)");
	}

	// ─── setBackground: mThread==null 时仅改字段, 无副作用 ──────
	private static void testSetBackground()
	{
		MyThread thread = new MyThread("test-thread");
		// mThread==null, 只改 mIsBackground 字段, 不会访问真实线程, 无异常即通过
		thread.setBackground(false);
		thread.setBackground(true);
	}

	// ─── stop: mThread==null 时直接 return, 空安全 ──────────────
	private static void testStopWhenNotStarted()
	{
		MyThread thread = new MyThread("test-thread");
		thread.stop();   // mThread==null, 直接 return, 不触发 logError
		thread.destroy(); // destroy 内部调 stop(), 同样空安全
	}
}
