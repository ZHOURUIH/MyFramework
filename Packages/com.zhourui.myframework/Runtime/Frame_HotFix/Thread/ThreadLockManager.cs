using System.Collections.Generic;

// 为了依赖关系考虑,线程锁的管理器相对独立,只存储线程锁的列表,不考虑生命周期
public class ThreadLockManager
{
	protected static HashSet<ThreadLock> mLockList = new();
	protected static object mLockListMutex = new();
	public static void registerLock(ThreadLock threadLock)
	{
		lock (mLockListMutex) 
		{
			mLockList.Add(threadLock); 
		}
	}
	public static void unregisterLock(ThreadLock threadLock)
	{
		lock (mLockListMutex) 
		{
			mLockList.Remove(threadLock); 
		}
	}
	public static void tryUnlockThreadLock(int threadID)
	{
		lock (mLockListMutex)
		{
			foreach (ThreadLock item in mLockList)
			{
				if (item.getThreadLockID() == threadID && item.isLocked())
				{
					item.unlock();
				}
			}
		}
	}
}