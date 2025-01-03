#ifndef _MY_QUEUE_H_
#define _MY_QUEUE_H_

#include "mySTL.h"
#include <queue>

template<typename T>
class myQueue : public mySTL
{
public:
	myQueue(){}
	virtual ~myQueue(){}
	void push(const T& value) { mQueue.push(value); }
	T pop_front() 
	{
		T value = mQueue.front();
		mQueue.pop();
		return value;
	}
	T& front() { return mQueue.front(); }
	uint size() const { return mQueue.size(); }
public:
	queue<T> mQueue;
};

#endif