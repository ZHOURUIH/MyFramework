#ifndef _TIME_LOCK_H_
#define _TIME_LOCK_H_

#include "ServerDefine.h"

class TimeLock
{
public:
	TimeLock(long frameTimeMS, long forceSleep = 5);
	// 返回值表示上一帧经过的时间
	long update();
	void setForceSleep(long forceTimeMS){mForceSleep = forceTimeMS;}
protected:
	long mFrameTimeMS;
	long mLastTime;
	long mCurTime;
	long mForceSleep;          // 每帧无暂停时间时强制暂停的毫秒数,避免线程单帧任务繁重时,导致单帧消耗时间大于设定的固定单帧时间时,CPU占用过高的问题
};

#endif