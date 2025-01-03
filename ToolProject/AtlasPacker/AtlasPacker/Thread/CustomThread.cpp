#include "CustomThread.h"
#include "txMemoryTrace.h"
#include "TimeLock.h"

CustomThread::CustomThread(const string& name)
{
	mName = name;
	mFinish = true;
	mRunning = false;
	mCallback = NULL;
	mThread = NULL_THREAD;
	mTimeLock = NULL;
	mPause = false;
	mIsBackground = true;
}

CustomThread::~CustomThread()
{
	destroy();
}

void CustomThread::destroy()
{
	stop();
}

void CustomThread::start(CustomThreadCallback callback, void* args, int frameTimeMS)
{
	//LOG_INFO("准备启动线程 : %s", mName.c_str());
	if (mThread != NULL_THREAD)
	{
		//LOG_ERROR("线程已经启动 : %s", mName.c_str());
		return;
	}
	mTimeLock = NEW(TimeLock, mTimeLock, frameTimeMS);
	mRunning = true;
	mCallback = callback;
	mArgs = args;
	CREATE_THREAD(mThread, run, this);
	//LOG_INFO("线程启动成功 : %s", mName.c_str());
}

void CustomThread::stop()
{
	//LOG_INFO("准备退出线程 : %s", mName.c_str());
	mRunning = false;
	mPause = false;
	while (!mIsBackground && !mFinish) {}
	CLOSE_THREAD(mThread);
	mCallback = NULL;
	DELETE(mTimeLock);
	//LOG_INFO("线程退出完成! 线程名 : %s", mName.c_str());
}

void CustomThread::updateThread()
{
	mFinish = false;
	while (mRunning)
	{
		mTimeLock->update();
		try
		{
			if (mPause)
			{
				continue;
			}
			if (!mCallback(mArgs))
			{
				break;
			}
		}
		catch (...)
		{
			//LOG_INFO("捕获线程异常! 线程名 : %s", mName.c_str());
		}
	}
	mFinish = true;
}