#ifndef _MY_STACK_H_
#define _MY_STACK_H_

#include "mySTL.h"
#include <stack>

template<typename T>
class myStack : public mySTL
{
public:
	myStack(){}
	virtual ~myStack(){}
	void push(const T& value) { mStack.push(value); }
	T pop()
	{
		T value = mStack.top();
		mStack.pop(); 
		return value;
	}
	T& top() { return mStack.top(); }
	uint size() const { return (uint)mStack.size(); }
public:
	stack<T> mStack;
};

#endif