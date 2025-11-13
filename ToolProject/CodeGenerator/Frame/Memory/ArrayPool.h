#ifndef _ARRAY_POOL_H_
#define _ARRAY_POOL_H_

#include "SystemUtility.h"
#include "ThreadArrayMemory.h"

class ArrayPool
{
public:
	static void destroy();
	template<typename T>
	static T* newArray(int count, bool zeroMemory = false)
	{
		ThreadArrayMemory* threadArrayMemory = NULL;
		// 找到指定线程的内存列表
		uint threadID = SystemUtility::getThreadID();
		LOCK(mLock);
		if (!mThreadArrayMemoryList.contains(threadID))
		{
			threadArrayMemory = NEW(ThreadArrayMemory, threadArrayMemory);
			mThreadArrayMemoryList.insert(threadID, threadArrayMemory);
		}
		else
		{
			threadArrayMemory = mThreadArrayMemoryList[threadID];
		}
		UNLOCK(mLock);
		// 在线程内存列表中找到指定类型的内存列表
		return threadArrayMemory->get<T>(count, zeroMemory);
	}
	static void deleteArray(void* data);
protected:
	static ThreadLock mLock;
	static myMap<uint, ThreadArrayMemory*> mThreadArrayMemoryList;
};

#endif