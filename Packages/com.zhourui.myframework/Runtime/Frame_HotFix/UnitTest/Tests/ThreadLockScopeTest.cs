using static TestAssert;

// ThreadLockScope 自动加锁解锁作用域测试
public static class ThreadLockScopeTest
{
	public static void Run()
	{
		testScopeLocksAndUnlocks();
		testScopeWithNullLock();
		testDisposeExplicitly();
		testDisposeTwice();
	}

	static void testScopeLocksAndUnlocks()
	{
		ThreadLock lockObj = new ThreadLock();
		assertFalse(lockObj.isLocked(), "Lock should be unlocked before scope");

		using (new ThreadLockScope(lockObj))
		{
			assertTrue(lockObj.isLocked(), "Lock should be held inside scope");
		}

		assertFalse(lockObj.isLocked(), "Lock should be released after scope exits");
		lockObj.destroy();
	}

	static void testScopeWithNullLock()
	{
		// Should not throw when ThreadLock is null
		using (new ThreadLockScope(null))
		{
			// No assertion needed; the test passes if we reach here without crash
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// 显式 Dispose(非 using)
	static void testDisposeExplicitly()
	{
		ThreadLock lockObj = new ThreadLock();
		try
		{
			ThreadLockScope scope = new ThreadLockScope(lockObj);
			assertTrue(lockObj.isLocked(), "构造后已加锁");
			scope.Dispose();
			assertFalse(lockObj.isLocked(), "Dispose 后已解锁");
		}
		finally
		{
			lockObj.destroy();
		}
	}

	// ⚠️ 不能嵌套同锁: ThreadLock.waitForUnlock 同线程重复加锁会 logError + 阻塞死锁(框架限制, 合法不测)
	// Dispose 后重复使用 scope 不再解锁(null 锁安全)
	static void testDisposeTwice()
	{
		ThreadLock lockObj = new ThreadLock();
		try
		{
			ThreadLockScope scope = new ThreadLockScope(lockObj);
			scope.Dispose();
			scope.Dispose();
			assertFalse(lockObj.isLocked(), "重复 Dispose 安全");
		}
		finally
		{
			lockObj.destroy();
		}
	}
}