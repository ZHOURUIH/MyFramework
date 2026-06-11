#ifndef _QUEUE_H_
#define _QUEUE_H_

#include "mySTL.h"
#include <queue>

using std::queue;

template<typename T>
class Queue : public mySTL
{
public:
	void push(const T& value) { mQueue.push(value); }
	T pop_front()
	{
		T value = front();
		pop();
		return value;
	}
	T& front() { return mQueue.front(); }
	void pop() { mQueue.pop(); }
	uint size() const { return (uint)mQueue.size(); }
public:
	queue<T> mQueue;
};

#endif