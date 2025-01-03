#include "txMemoryCheck.h"

mySet<void*> txMemoryCheck::mUsedPtrs;
ThreadLock txMemoryCheck::mLock;

void txMemoryCheck::usePtr(void* ptr)
{
	LOCK(mLock);
	if (!mUsedPtrs.insert(ptr))
	{
		//LOG_ERROR("ptr is in use!");
	}
	UNLOCK(mLock);
}

void txMemoryCheck::unusePtr(void* ptr)
{
	LOCK(mLock);
	if (!mUsedPtrs.erase(ptr))
	{
		//LOG_ERROR("not find ptr! can not unuse it!");
	}
	UNLOCK(mLock);
}

bool txMemoryCheck::canAccess(void* ptr)
{
	bool ret = false;
	LOCK(mLock);
	ret = mUsedPtrs.contains(ptr);
	UNLOCK(mLock);
	return ret;
}