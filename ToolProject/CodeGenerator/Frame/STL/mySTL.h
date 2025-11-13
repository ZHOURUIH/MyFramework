#ifndef _MY_STL_H_
#define _MY_STL_H_

#include <string>
#include <vector>
#include <atomic>

typedef unsigned int uint;
typedef unsigned char byte;

using namespace std;

#if _DEBUG
enum class STL_LOCK : byte
{
	SL_NONE,
	SL_READ,
	SL_WRITE,
};
#endif

class mySTL
{
public:
	mySTL();
	mySTL(const mySTL& other)
	{
#if _DEBUG
		mLock.exchange(other.mLock);
		mReadLockCount.exchange(other.mReadLockCount);
		mLine.exchange(other.mLine);
		mFile.exchange(other.mFile);
#endif
	}
	mySTL& operator=(const mySTL& other)
	{
#if _DEBUG
		mLock.exchange(other.mLock);
		mReadLockCount.exchange(other.mReadLockCount);
		mLine.exchange(other.mLine);
		mFile.exchange(other.mFile);
#endif
		return *this;
	}
	virtual ~mySTL() {}
#if _DEBUG
	// 循环遍历列表之前必须锁定
	void lock(STL_LOCK lockType, const char* file, uint line);
	// 循环遍历列表结束以后必须解锁
	void unlock(STL_LOCK lockType);
#endif
protected:
	void directError(const string& info);
#if _DEBUG
	void checkLock();
	void resetLock();
#endif
protected:
#if _DEBUG
	volatile atomic<STL_LOCK> mLock;
	volatile atomic<uint> mReadLockCount;	// 读锁定的次数,读锁定可以叠加,计算读锁定的次数,当读锁定次数为0时,取消锁定
	volatile atomic<const char*> mFile;
	volatile atomic<uint> mLine;
#endif
};

#endif