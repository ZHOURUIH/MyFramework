#include "ThreadLock.h"
#include "Utility.h"

void ThreadLock::waitForUnlock(const char* file, uint line)
{
	// 获取当前线程ID
	uint threadID = SystemUtility::getThreadID();
	// 原子自旋操作
	while (mLock.test_and_set())
	{
		SystemUtility::sleep(1);
	}
#if _DEBUG
	memset((char*)mFile, 0, 256);
	memcpy((char*)mFile, file, strlen(file));
#endif
	mLine = line;
	mThreadID = threadID;
}

void ThreadLock::unlock()
{
	mLock.clear();
}