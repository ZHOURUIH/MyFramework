#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Threading;
using static TestAssert;

// ThreadLock 线程锁测试
public static class ThreadLockTest
{
    public static void Run()
    {
        testLockAndUnlock();
        testIsLocked();
        testMultipleLock();
        testSetEnable();
        testThreadLockScope();
    }

    static void testLockAndUnlock()
    {
        ThreadLock lockObj = new ThreadLock();
        assertFalse(lockObj.isLocked(), "ThreadLock should not be locked initially");

        lockObj.waitForUnlock();
        assertTrue(lockObj.isLocked(), "ThreadLock should be locked after lock()");

        lockObj.unlock();
        assertFalse(lockObj.isLocked(), "ThreadLock should be unlocked after unlock()");

        lockObj.destroy();
    }

    static void testIsLocked()
    {
        ThreadLock lockObj = new ThreadLock();
        assertFalse(lockObj.isLocked(), "Fresh lock should not be locked");

        lockObj.waitForUnlock();
        assertTrue(lockObj.isLocked(), "Lock should be locked after acquisition");

        lockObj.unlock();
        assertFalse(lockObj.isLocked(), "Lock should be released after unlock");

        lockObj.destroy();
    }

    static void testMultipleLock()
    {
        ThreadLock lockObj = new ThreadLock();
        // ThreadLock 是不可重入的 spinlock，不能嵌套 waitForUnlock
        // 正确的多轮测试：获取→释放→再获取→再释放
        assertFalse(lockObj.isLocked(), "Should start unlocked");

        lockObj.waitForUnlock();
        assertTrue(lockObj.isLocked(), "Lock should be locked after first acquire");

        lockObj.unlock();
        assertFalse(lockObj.isLocked(), "Lock should be released after first unlock");

        lockObj.waitForUnlock();
        assertTrue(lockObj.isLocked(), "Lock should be locked after second acquire");

        lockObj.unlock();
        assertFalse(lockObj.isLocked(), "Lock should be released after second unlock");

        lockObj.destroy();
    }

    static void testSetEnable()
    {
        ThreadLock lockObj = new ThreadLock();
        lockObj.setEnable(false);
        // When disabled, lock should be a no-op
        lockObj.waitForUnlock();
        // isLocked may still report false when disabled, depending on implementation
        lockObj.unlock();

        lockObj.setEnable(true);
        lockObj.waitForUnlock();
        assertTrue(lockObj.isLocked(), "Lock should work after re-enable");
        lockObj.unlock();

        lockObj.destroy();
    }

    static void testThreadLockScope()
    {
        ThreadLock lockObj = new ThreadLock();
        assertFalse(lockObj.isLocked(), "Should start unlocked");

        using (new ThreadLockScope(lockObj))
        {
            assertTrue(lockObj.isLocked(), "Should be locked within scope");
        }

        assertFalse(lockObj.isLocked(), "Should be unlocked after scope exits");

        lockObj.destroy();
    }
}
#endif
