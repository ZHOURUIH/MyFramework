#pragma once

#include "UsingSTD.h"

template<typename T>
class Queue
{
	typedef typename queue<T>::iterator iterator;
	typedef typename queue<T>::reverse_iterator reverse_iterator;
	typedef typename queue<T>::const_iterator const_iterator;
public:
	iterator begin() { return mQueue.begin(); }
	iterator end() { return mQueue.end(); }
	const_iterator begin() const { return mQueue.begin(); }
	const_iterator end()  const { return mQueue.end(); }
	reverse_iterator rbegin() const { return mQueue.rbegin(); }
	reverse_iterator rend() const { return mQueue.rend(); }
	const_iterator cbegin() const { return mQueue.cbegin(); }
	const_iterator cend() const { return mQueue.cend(); }
	void addRange(const Vector<T*>& list)
	{
		for (T* item : list)
		{
			mQueue.push(item);
		}
	}
	void add(const T& value) { mQueue.push(value); }
	T pop(T defaultValue)
	{
		if (size() == 0)
		{
			return defaultValue;
		}
		T value = front();
		mQueue.pop();
		return value;
	}
	T& front() { return mQueue.front(); }
	void popOnly() { mQueue.pop(); }
	int size() const { return (int)mQueue.size(); }
	void clear() { mQueue.clear(); }
public:
	queue<T> mQueue;
};