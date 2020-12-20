using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadLock
{
	protected string mFileName;
	protected int mLockCount;         // 是否锁定
	protected int mLine;
	protected bool mTraceStack;
	public void setTrackStack(bool trace){mTraceStack = trace;}
	public bool isLocked(){return mLockCount == 1;}
	public void waitForUnlock()
	{
		while (Interlocked.Exchange(ref mLockCount, 1) != 0){}
		if(mTraceStack)
		{
			mFileName = UnityUtility.getCurSourceFileName(2);
			mLine = UnityUtility.getLineNum(2);
		}
	}
	public void unlock()
	{
		Interlocked.Exchange(ref mLockCount, 0);
		if (mTraceStack)
		{
			mFileName = null;
			mLine = 0;
		}
	}
}