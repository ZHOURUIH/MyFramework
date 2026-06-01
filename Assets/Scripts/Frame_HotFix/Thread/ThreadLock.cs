using System.Threading;
using static UnityUtility;
using static FrameUtility;

public class ThreadLock
{
	protected volatile int mLockThreadID;   // 当前获得锁的线程ID
	protected volatile int mLockCount;		// 是否锁定
	protected string mFileName;				// 加锁的文件名
	protected int mLine;					// 加锁的行号
	protected bool mTraceStack;				// 是否追踪加锁的堆栈,默认仅在编辑器下追踪,会有较大的GC
	protected bool mEnable = true;			// 是否启用线程锁
	public ThreadLock()
	{
		ThreadLockManager.registerLock(this);
	}
	public void destroy()
	{
		ThreadLockManager.unregisterLock(this);
	}
	public void setEnable(bool enable) { mEnable = enable; }
	public void setTrackStack(bool trace) { mTraceStack = trace; }
	public bool isLocked() { return mLockCount == 1; }
	public int getThreadLockID() { return mLockThreadID; }
	public void waitForUnlock()
	{
		if (!mEnable)
		{
			return;
		}

		int currentThreadId = Thread.CurrentThread.ManagedThreadId;
		if (mLockThreadID == currentThreadId && isLocked())
		{
			logError("线程重复加锁");
		}

		SpinWait spin = new();
		while (true)
		{
			// 一直尝试将mLockCount设置为1,然后判断设置之前mLockCount是否为0
			// 如果mLockCount在这之前为0,则表示锁在其他线程被释放,当前线程成功获得锁
			if (mLockCount == 0 && Interlocked.CompareExchange(ref mLockCount, 1, 0) == 0)
			{
				break;
			}
			spin.SpinOnce();
		}

		mLockThreadID = currentThreadId;
		if (mTraceStack)
		{
			// 这两行会有比较严重的GCAlloc,需要谨慎使用
			mFileName = getCurSourceFileName(2);
			mLine = getLineNum(2);
		}
	}
	public void unlock()
	{
		if (!mEnable)
		{
			return;
		}
		mLockThreadID = 0;
		if (mTraceStack)
		{
			mFileName = null;
			mLine = 0;
		}
		Interlocked.Exchange(ref mLockCount, 0);
	}
}