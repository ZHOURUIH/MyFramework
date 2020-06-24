using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadTimeLock
{
	protected DateTime mLastTime;
	protected DateTime mCurTime;
	protected int mFrameTimeMS;
	// 每帧无暂停时间时强制暂停的毫秒数
	// 避免线程单帧任务繁重时,导致单帧消耗时间大于设定的固定单帧时间时,CPU占用过高的问题
	protected int mForceSleep;
	public ThreadTimeLock(int frameTimeMS)
	{
		mLastTime = DateTime.Now;
		mCurTime = mLastTime;
		mFrameTimeMS = frameTimeMS;
		mForceSleep = 0;
	}
	public void setForceSleep(int timeMS){mForceSleep = timeMS;}
	public void setFrameTime(int timeMS){mFrameTimeMS = timeMS;}
	public double update()
	{
		DateTime endTime = DateTime.Now;
		long remainMS = mFrameTimeMS - (long)(endTime - mLastTime).TotalMilliseconds;
		if(remainMS > 0)
		{
			Thread.Sleep((int)remainMS);
		}
		else if(mForceSleep > 0)
		{
			Thread.Sleep(mForceSleep);
		}
		mCurTime = DateTime.Now;
		double frameTime = (mCurTime - mLastTime).TotalMilliseconds;
		mLastTime = mCurTime;
		return frameTime;
	}
}