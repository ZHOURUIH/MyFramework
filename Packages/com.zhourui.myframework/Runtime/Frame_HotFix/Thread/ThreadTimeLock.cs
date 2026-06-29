using System;
using System.Threading;

// 用于线程锁帧
public class ThreadTimeLock
{
	protected DateTime mLastTime;   // 上一帧的时间
	protected int mFrameTimeMS;     // 每一帧希望执行的毫秒数,如果一帧执行时间低于此毫秒数,则会暂停一定时间
	// 避免线程单帧任务繁重时,导致单帧消耗时间大于设定的固定单帧时间时,CPU占用过高的问题
	protected int mForceSleep;      // 每帧无暂停时间时强制暂停的毫秒数,
	public ThreadTimeLock(int frameTimeMS)
	{
		mLastTime = DateTime.Now;
		mFrameTimeMS = frameTimeMS;
	}
	public void setForceSleep(int timeMS) { mForceSleep = timeMS; }
	public void setFrameTime(int timeMS) { mFrameTimeMS = timeMS; }
	public DateTime getFrameStartTime() { return mLastTime; }
	public double update()
	{
		DateTime endTime = DateTime.Now;
		long remainMS = mFrameTimeMS - (long)(endTime - mLastTime).TotalMilliseconds;
		if (remainMS > 0)
		{
			Thread.Sleep((int)remainMS);
		}
		else if (mForceSleep > 0)
		{
			Thread.Sleep(mForceSleep);
		}
		double frameTime = (DateTime.Now - mLastTime).TotalMilliseconds;
		mLastTime = DateTime.Now;
		return frameTime;
	}
}