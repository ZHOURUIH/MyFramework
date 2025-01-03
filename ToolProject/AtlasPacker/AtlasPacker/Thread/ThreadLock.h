#ifndef _THREAD_LOCK_H_
#define _THREAD_LOCK_H_

#include "ServerDefine.h"

class ThreadLock
{
public:
	ThreadLock()
	{
		mLock.clear();
		mLine = 0;
		mThreadID = 0;
	}
	~ThreadLock(){}
	void waitForUnlock(const char* file, int line);
	void unlock();
public:
	volatile std::atomic_flag mLock;	// 1表示锁定,0表示未锁定
	volatile char mFile[256];			// 加锁的文件名
	volatile int mLine;					// 加锁的行号
	volatile int mThreadID;// 加锁线程的ID
};

#endif