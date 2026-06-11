#pragma once

#include "UsingSTD.h"

template<typename T>
class Stack
{
public:
	void push(const T& value) { mStack.push(value); }
	T pop()
	{
		T value = mStack.top();
		mStack.pop(); 
		return value;
	}
	T& top() { return mStack.top(); }
	int size() const { return (int)mStack.size(); }
public:
	stack<T> mStack;
};