using System;
using System.Threading;
using static UnityUtility;

// 对线程的封装
public class MyThread
{
	protected RefBoolCallback mCallback;			// 线程执行回调
	protected ThreadTimeLock mTimeLock = new(0);	// 用于普通线程锁帧
	protected Thread mThread;						// 线程对象
	protected AutoResetEvent mWakeEvent;			// 事件线程休眠/唤醒使用
	protected string mName;							// 线程名字
	protected volatile bool mIsBackground = true;	// 是否为后台线程
	protected volatile bool mRunning;				// 线程是否正在执行
	protected volatile bool mFinish = true;			// 线程是否已经完成执行
	protected volatile int mEventWaitTime = -1;		// 事件线程等待时间,-1表示无限等待
	protected bool mHasPrintThreadID;				// 用于控制打印当前线程的ID
	protected bool mEventMode;						// 是否为事件驱动线程
	public MyThread(string name)
	{
		mName = name;
	}
	public void destroy()
	{
		stop();
		mWakeEvent?.Dispose();
		mWakeEvent = null;
	}
	public void setBackground(bool background)
	{
		mIsBackground = background;
		if (mThread != null)
		{
			mThread.IsBackground = mIsBackground;
		}
	}
	// 普通周期线程
	public void start(RefBoolCallback callback, int frameTimeMS = 15, int forceSleep = 5)
	{
		startInternal(callback, false, frameTimeMS, forceSleep, -1);
	}
	// 事件线程,没有事件时完全休眠,wake后立即执行一轮
	// waitTimeMS < 0表示无限等待;>=0表示超时后也执行一轮
	public void startEvent(RefBoolCallback callback, int waitTimeMS = -1)
	{
		startInternal(callback, true, 0, 0, waitTimeMS);
	}
	public bool isFinished() { return mFinish; }
	public bool isEventMode() { return mEventMode; }
	public void setEventWaitTime(int waitTimeMS) { mEventWaitTime = waitTimeMS; }
	public int getEventWaitTime() { return mEventWaitTime; }
	// 事件线程唤醒。AutoResetEvent会保存一次未消费信号,连续多次wake允许合并。
	public void wake()
	{
		if (!mEventMode || mThread == null)
		{
			return;
		}
		mWakeEvent?.Set();
	}
	public void stop()
	{
		Thread thread = mThread;
		if (thread == null)
		{
			return;
		}
		try
		{
			// 必须先修改退出条件,再唤醒事件线程。
			// 否则线程可能先消费wake,执行完一轮后再次进入WaitOne,随后stop再等待mFinish就会永久卡住。
			mRunning = false;
			if (mEventMode)
			{
				mWakeEvent?.Set();
			}
			// 事件线程内部正在使用mWakeEvent,即使是后台线程也必须等待真正退出后才能继续销毁。
			while ((mEventMode || !mIsBackground) && !mFinish)
			{
				Thread.Sleep(0);
			}
			// 事件线程等待mFinish后自然退出,不再额外Abort。
			// 普通线程继续保留原有Abort兜底。
			if (!mEventMode && thread.IsAlive)
			{
				thread.Abort();
			}
			mThread = null;
			mCallback = null;
			if (mEventMode)
			{
				mWakeEvent?.Reset();
			}
			mEventMode = false;
			mEventWaitTime = -1;
		}
		catch(Exception e)
		{
			logException(e, "线程退出出现异常:" + mName);
		}
		log("线程退出完成! 线程名 : " + mName);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void startInternal(RefBoolCallback callback, bool eventMode, int frameTimeMS, int forceSleep, int eventWaitTime)
	{
		if (mThread != null)
		{
			return;
		}
		mEventMode = eventMode;
		mEventWaitTime = eventWaitTime;
		mHasPrintThreadID = false;
		if (mEventMode)
		{
			mWakeEvent ??= new(false);
			// stop后可能残留一次未消费信号,重新启动事件线程前必须复位。
			mWakeEvent.Reset();
		}
		else
		{
			mTimeLock.setFrameTime(frameTimeMS);
			mTimeLock.setForceSleep(forceSleep);
		}
		mRunning = true;
		mFinish = false;
		mCallback = callback;
		mThread = new(run);
		mThread.Name = mName;
		mThread.IsBackground = mIsBackground;
		mThread.Start();
		log("线程启动成功 : " + mName);
	}
	protected void waitEvent()
	{
		int waitTime = mEventWaitTime;
		if (waitTime < 0)
		{
			mWakeEvent.WaitOne();
		}
		else
		{
			mWakeEvent.WaitOne(waitTime);
		}
	}
	protected void run()
	{
		mFinish = false;
		try
		{
			while (mRunning)
			{
				if (!mHasPrintThreadID)
				{
					mHasPrintThreadID = true;
					log("线程ID:" + Thread.CurrentThread.ManagedThreadId + ", name:" + mName);
				}
				try
				{
					if (mEventMode)
					{
						waitEvent();
						// stop()会先把mRunning置false再wake,这里直接退出,不再额外执行一次业务回调。
						if (!mRunning)
						{
							break;
						}
					}
					else
					{
						mTimeLock.update();
					}
					bool run = true;
					mCallback?.Invoke(ref run);
					if (!run)
					{
						break;
					}
				}
				catch (ThreadAbortException)
				{
					// 调用Thread.Abort而正常终止线程
					ThreadLockManager.tryUnlockThreadLock(Thread.CurrentThread.ManagedThreadId);
					break;
				}
				catch (Exception e)
				{
					logException(e, "捕获线程异常! 线程名 : " + mName);
				}
			}
		}
		finally
		{
			mFinish = true;
		}
	}
}