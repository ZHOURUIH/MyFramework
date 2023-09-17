using System;

// 用于自动加锁解锁的类,需要配合using使用
// using (new ThreadLockScope(mLock))
public struct ThreadLockScope : IDisposable
{
	public ThreadLock mLock;
	public ThreadLockScope(ThreadLock threadLock)
	{
		mLock = threadLock;
		mLock?.waitForUnlock();
	}
	public void Dispose()
	{
		mLock?.unlock();
	}
}