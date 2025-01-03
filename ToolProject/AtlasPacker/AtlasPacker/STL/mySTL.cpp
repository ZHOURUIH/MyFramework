#include "mySTL.h"

mySTL::mySTL()
{
#if _DEBUG
	mLock = STL_LOCK::NONE;
	mReadLockCount = 0;
	mLine = 0;
	mFile = NULL;
#endif
}

#if _DEBUG
// 循环遍历列表之前必须锁定
void mySTL::lock(STL_LOCK lockType, const char* file, uint line)
{
	// 当前为锁定写入,则不允许再加任何锁,或者当前为读锁定,则不能添加写锁定
	if (mLock == STL_LOCK::WRITE || 
		(mLock == STL_LOCK::READ && lockType != STL_LOCK::READ) || 
		lockType == STL_LOCK::NONE)
	{
		directError("can not lock stl before unlock!");
		return;
	}
	mFile = file;
	mLine = line;
	mLock = lockType;
	if (mLock == STL_LOCK::READ)
	{
		// 读锁定计数加1
		++mReadLockCount;
	}
}
// 循环遍历列表结束以后必须解锁
void mySTL::unlock(STL_LOCK lockType)
{
	// 锁定类型不一致,则不能解锁
	if (mLock == STL_LOCK::NONE || mLock != lockType)
	{
		directError("can not unlock stl before lock!");
		return;
	}
	if (mLock == STL_LOCK::READ)
	{
		// 读锁定计数减1,当计数为0时解锁
		--mReadLockCount;
		if (mReadLockCount == 0)
		{
			mLock = STL_LOCK::NONE;
		}
	}
	else if (mLock == STL_LOCK::WRITE)
	{
		mLock = STL_LOCK::NONE;
	}
}
void mySTL::checkLock()
{
	if (mLock != STL_LOCK::NONE)
	{
		directError("stl is locked! can not modify stl!");
	}
}
// 重置锁
void mySTL::resetLock()
{
	mLock = STL_LOCK::NONE;
	mReadLockCount = 0;
	mFile = NULL;
	mLine = 0;
}
#endif
void mySTL::directError(const string& info) const
{
	//ERROR(info);
}