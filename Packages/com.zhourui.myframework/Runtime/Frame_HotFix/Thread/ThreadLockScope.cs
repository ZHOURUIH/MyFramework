using System;

// 用于自动加锁解锁的类,需要配合using使用
// using (new ThreadLockScope(mLock))
// 但是需要注意的是并不能完全保证一定能释放锁,如果所在线程被强制终止时,可能无法正常执行Dispose
public struct ThreadLockScope : IDisposable
{
	private ThreadLock mLock;		// 线程锁
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