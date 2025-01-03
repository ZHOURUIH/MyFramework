#ifndef _TX_MEMORY_CHECK_H_
#define _TX_MEMORY_CHECK_H_

#include "ThreadLock.h"

class txMemoryCheck
{
public:
	static void usePtr(void* ptr);
	static void unusePtr(void* ptr);
	static bool canAccess(void* ptr);
protected:
	static mySet<void*> mUsedPtrs;
	static ThreadLock mLock;
};

#endif