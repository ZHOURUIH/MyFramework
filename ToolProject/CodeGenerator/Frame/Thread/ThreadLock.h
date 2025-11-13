#ifndef _THREAD_LOCK_H_
#define _THREAD_LOCK_H_

#include "FrameDefine.h"

class ThreadLock
{
public:
	ThreadLock()
	{
		mLock.clear();
		mLine = 0;
		mThreadID = 0;
#if _DEBUG
		memset((void*)mFile, 0, 256);
#endif
	}
	virtual ~ThreadLock(){}
	void waitForUnlock(const char* file, uint line);
	void unlock();
public:
	volatile atomic_flag mLock;	// 1表示锁定,0表示未锁定
#if _DEBUG
	volatile char mFile[256];	// 加锁的文件名
#endif
	volatile uint mLine;		// 加锁的行号
	volatile uint mThreadID;	// 加锁线程的ID
};

#endif