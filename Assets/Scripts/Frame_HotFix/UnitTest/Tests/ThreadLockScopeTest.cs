#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;

// ThreadLockScope 自动加锁解锁作用域测试
public static class ThreadLockScopeTest
{
	public static void Run()
	{
		testScopeLocksAndUnlocks();
		testScopeWithNullLock();
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
}
#endif
