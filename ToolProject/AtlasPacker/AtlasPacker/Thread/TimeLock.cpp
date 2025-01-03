#include "TimeLock.h"
#include "SystemUtility.h"

TimeLock::TimeLock(long frameTimeMS, long forceSleep)
{
	mFrameTimeMS = frameTimeMS;
	mForceSleep = 5;
	mLastTime = SystemUtility::getTimeMS();
	mCurTime = mLastTime;
}

long TimeLock::update()
{
	long endTime = SystemUtility::getTimeMS();
	long remainMS = mFrameTimeMS - (endTime - mCurTime);
	if (remainMS > 0)
	{
		SystemUtility::sleep(remainMS);
	}
	else if (mForceSleep > 0)
	{
		SystemUtility::sleep(mForceSleep);
	}
	mCurTime = SystemUtility::getTimeMS();
	long frameTime = mCurTime - mLastTime;
	mLastTime = mCurTime;
	return frameTime;
}