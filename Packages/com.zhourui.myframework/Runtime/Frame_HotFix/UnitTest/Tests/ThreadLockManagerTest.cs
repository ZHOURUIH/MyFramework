using System.Threading;
using static TestAssert;

// ThreadLockManager 单元测试 — 线程锁管理器的注册/反注册与批量解锁
//
// 设计要点:
//   - ThreadLock 构造时会自动 registerLock 到 ThreadLockManager 的静态集合,
//     destroy() 时 unregisterLock。mLockList 是全局静态状态(测试间不重置),
//     因此本测试每个用例都要在 finally 里 destroy 自己 new 的锁, 避免残留。
//   - waitForUnlock() 在单线程 EditMode 测试中会立即获得锁(锁空闲时
//     CompareExchange 立刻成功), 并把 mLockThreadID 置为当前线程的
//     ManagedThreadId。因此"锁定于当前线程"的锁, 其线程 ID == 当前线程 ID。
//   - 为降低编辑器堆栈追踪(setTrackStack 默认 true)的额外开销与噪音,
//     统一用 setTrackStack(false) 关闭堆栈记录。
//   - tryUnlockThreadLock(threadID) 只解锁"已注册 && isLocked() &&
//     线程ID匹配"的锁。
public static class ThreadLockManagerTest
{
	public static void Run()
	{
		testRegisterAndTryUnlock();
		testUnregisterExcludesLock();
		testTryUnlockSkipsUnlocked();
		testTryUnlockWrongThreadID();
		testTryUnlockMultipleLocks();
	}

	// ─── 注册后可用当前线程 ID 批量解锁 ─────────────────────────────
	// 验证: new ThreadLock() 自动 registerLock; 锁已获得(线程ID=当前线程)时,
	//   tryUnlockThreadLock(当前线程ID) 能解锁所有匹配线程的已注册锁。
	private static void testRegisterAndTryUnlock()
	{
		ThreadLock a = null;
		ThreadLock b = null;
		try
		{
			a = newLock();
			b = newLock();
			a.waitForUnlock();
			b.waitForUnlock();
			assertTrue(a.isLocked(), "锁A应已锁定");
			assertTrue(b.isLocked(), "锁B应已锁定");

			int currentId = Thread.CurrentThread.ManagedThreadId;
			ThreadLockManager.tryUnlockThreadLock(currentId);
			assertFalse(a.isLocked(), "A 应被 tryUnlockThreadLock 解锁(线程ID匹配)");
			assertFalse(b.isLocked(), "B 应被 tryUnlockThreadLock 解锁(线程ID匹配)");
		}
		finally
		{
			safeDestroy(a);
			safeDestroy(b);
		}
	}

	// ─── destroy() 反注册后不再被批量解锁 ───────────────────────────
	// 验证: unregisterLock 生效。锁获得后先 destroy() 反注册,
	//   使其脱离管理器的静态集合, 此后 tryUnlockThreadLock 不再作用到它,
	//   即便它仍处于 isLocked()==true 状态也保持锁定。
	private static void testUnregisterExcludesLock()
	{
		ThreadLock a = null;
		try
		{
			a = newLock();
			a.waitForUnlock();
			assertTrue(a.isLocked(), "锁A应已锁定");

			// 反注册: destroy() 会调用 ThreadLockManager.unregisterLock
			a.destroy();
			assertTrue(a.isLocked(), "destroy() 只反注册, 不改变锁的持有状态(仍锁定)");

			// 此时 a 已不在 mLockList, 批量解锁不应再作用于它
			int currentId = Thread.CurrentThread.ManagedThreadId;
			ThreadLockManager.tryUnlockThreadLock(currentId);
			assertTrue(a.isLocked(), "已反注册的锁不应被 tryUnlockThreadLock 解锁");
		}
		finally
		{
			// a 已 destroy 反注册, 直接置空避免二次 destroy
			a = null;
		}
	}

	// ─── tryUnlock 跳过未锁定(但已注册)的锁 ─────────────────────────
	private static void testTryUnlockSkipsUnlocked()
	{
		ThreadLock locked = null;
		ThreadLock idle = null;
		try
		{
			locked = newLock();
			idle = newLock();       // 从未获取, isLocked()==false, 但已注册
			locked.waitForUnlock();
			assertTrue(locked.isLocked(), "锁定锁应已锁定");
			assertFalse(idle.isLocked(), "空闲锁应未锁定");

			int currentId = Thread.CurrentThread.ManagedThreadId;
			ThreadLockManager.tryUnlockThreadLock(currentId);
			assertFalse(locked.isLocked(), "锁定锁应被解锁");
			assertFalse(idle.isLocked(), "空闲锁保持未锁定(本就未锁定)");
		}
		finally
		{
			safeDestroy(locked);
			safeDestroy(idle);
		}
	}

	// ─── 线程 ID 不匹配时不解锁 ─────────────────────────────────────
	// 验证: 锁锁定于当前线程, 传入一个"非当前线程"的 ID 时, tryUnlock 不生效。
	private static void testTryUnlockWrongThreadID()
	{
		ThreadLock a = null;
		try
		{
			a = newLock();
			a.waitForUnlock();
			assertTrue(a.isLocked(), "锁A应已锁定");
			assertEqual(Thread.CurrentThread.ManagedThreadId, a.getThreadLockID(), "锁A的线程ID应为当前线程ID");

			int wrongId = Thread.CurrentThread.ManagedThreadId + 100000; // 一个几乎不可能相等的"外部线程 ID"
			ThreadLockManager.tryUnlockThreadLock(wrongId);
			assertTrue(a.isLocked(), "ID 不匹配时 tryUnlock 不应解锁");
		}
		finally
		{
			safeDestroy(a);
		}
	}

	// ─── 多个锁: 只解锁当前线程持有的所有锁 ────────────────────────
	private static void testTryUnlockMultipleLocks()
	{
		ThreadLock a = null;
		ThreadLock b = null;
		ThreadLock c = null;
		try
		{
			a = newLock();
			b = newLock();
			c = newLock();
			a.waitForUnlock();
			b.waitForUnlock();
			// c 未获取
			assertTrue(a.isLocked(), "A 锁定");
			assertTrue(b.isLocked(), "B 锁定");

			int currentId = Thread.CurrentThread.ManagedThreadId;
			ThreadLockManager.tryUnlockThreadLock(currentId);
			assertFalse(a.isLocked(), "A 解锁");
			assertFalse(b.isLocked(), "B 解锁");
			assertFalse(c.isLocked(), "C 保持未锁定");
		}
		finally
		{
			safeDestroy(a);
			safeDestroy(b);
			safeDestroy(c);
		}
	}

	// ─── 辅助 ───────────────────────────────────────────────────────
	private static ThreadLock newLock()
	{
		ThreadLock t = new ThreadLock();
		t.setTrackStack(false);     // 关闭堆栈追踪, 降低 GC 与噪音
		return t;
	}

	private static void safeDestroy(ThreadLock t)
	{
		if (t != null)
		{
			t.destroy();            // 反注册并从静态集合移除
		}
	}
}
