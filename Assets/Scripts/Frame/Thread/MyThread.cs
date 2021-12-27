using System;
using System.Threading;

// 对线程的封装
public class MyThread : FrameBase
{
	protected MyThreadCallback mCallback;		// 线程执行回调
	protected ThreadTimeLock mTimeLock;			// 用于线程锁帧
	protected Thread mThread;					// 线程对象
	protected BOOL mRun;						// 用于线程执行的输出参数,决定线程是否继续运行
	protected string mName;						// 线程名字
	protected volatile bool mIsBackground;		// 是否为后台线程,如果是后台线程,则在应用程序关闭时,子线程会自动强制关闭
	protected volatile bool mRunning;			// 线程是否正在执行
	protected volatile bool mFinish;			// 线程是否已经完成执行
	public MyThread(string name)
	{
		mRun = new BOOL();
		mTimeLock = new ThreadTimeLock(0);
		mName = name;
		mIsBackground = true;
		mFinish = true;
	}
	public void destroy()
	{
		stop();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mTimeLock.setFrameTime(0);
		mThread = null;
		mRun.set(false);
		mName = null;
		mIsBackground = true;
		mRunning = false;
		mFinish = true;
	}
	public void setBackground(bool background)
	{
		mIsBackground = background;
		if (mThread != null)
		{
			mThread.IsBackground = mIsBackground;
		}
	}
	public void start(MyThreadCallback callback, int frameTimeMS = 15, int forceSleep = 5)
	{
		if (mThread != null)
		{
			return;
		}
		mTimeLock = new ThreadTimeLock(frameTimeMS);
		mTimeLock.setForceSleep(forceSleep);
		mRunning = true;
		mCallback = callback;
		mThread = new Thread(run);
		mThread.Name = mName;
		mThread.Start();
		mThread.IsBackground = mIsBackground;
		logForce("线程启动成功 : " + mName);
	}
	public bool isFinished() { return mFinish; }
	public void stop()
	{
		if (mThread == null)
		{
			return;
		}
		try
		{
			mRunning = false;
			while (!mIsBackground && !mFinish)
			{
				Thread.Sleep(0);
			}
			mThread = null;
			mCallback = null;
			mTimeLock = null;
		}
		catch(Exception e)
		{
			logError("线程退出出现异常:" + mName + ", exception:" + e.Message);
		}
		logForce("线程退出完成! 线程名 : " + mName);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void run()
	{
		mFinish = false;
		while (mRunning)
		{
			mTimeLock.update();
			try
			{
				mRun.set(true);
				mCallback?.Invoke(mRun);
				if (!mRun.mValue)
				{
					break;
				}
			}
			catch (Exception e)
			{
				logError("捕获线程异常! 线程名 : " + mName + ", " + e.Message + ", " + e.StackTrace);
			}
		}
		mFinish = true;
		mThread?.Abort();
	}
}