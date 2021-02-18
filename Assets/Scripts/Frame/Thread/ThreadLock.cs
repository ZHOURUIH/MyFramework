using System;
using System.Threading;

public class ThreadLock : FrameBase
{
	protected string mFileName;         // 加锁的文件名
	protected long mLockCount;          // 是否锁定
	protected int mLockThreadID;        // 当前获得锁的线程ID
	protected int mLine;                // 加锁的行号
	protected bool mTraceStack;         // 是否追踪加锁的堆栈,默认不追踪
	public void setTrackStack(bool trace) { mTraceStack = trace; }
	public bool isLocked() { return Interlocked.Read(ref mLockCount) == 1; }
	public void waitForUnlock()
	{
		if (mLockThreadID == Thread.CurrentThread.ManagedThreadId && isLocked())
		{
			logError("线程死锁");
		}
		// 一直尝试将mLockCount设置为1,然后判断设置之前mLockCount是否为0
		// 如果mLockCount在这之前为0,则表示锁在其他线程被释放,当前线程成功获得锁
		while (Interlocked.Exchange(ref mLockCount, 1) != 0) { }
		mLockThreadID = Thread.CurrentThread.ManagedThreadId;
		if (mTraceStack)
		{
			mFileName = getCurSourceFileName(2);
			mLine = getLineNum(2);
		}
	}
	public void unlock()
	{
		mLockThreadID = 0;
		if (mTraceStack)
		{
			mFileName = null;
			mLine = 0;
		}
		Interlocked.Exchange(ref mLockCount, 0);
	}
}