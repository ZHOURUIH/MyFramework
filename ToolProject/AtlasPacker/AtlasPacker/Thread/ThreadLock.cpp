#include "ThreadLock.h"
#include "SystemUtility.h"

void ThreadLock::waitForUnlock(const char* file, int line)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	int threadID = GetCurrentThreadId();
#elif RUN_PLATFORM == PLATFORM_ANDROID
	int threadID = pthread_self();
#endif
	// 原子自旋操作
	while (mLock.test_and_set())
	{
		SystemUtility::sleep(1);
	}
	memset((char*)mFile, 0, 256);
	memcpy((char*)mFile, file, strlen(file));
	mLine = line;
	mThreadID = threadID;
}

void ThreadLock::unlock()
{
	mLock.clear();
}