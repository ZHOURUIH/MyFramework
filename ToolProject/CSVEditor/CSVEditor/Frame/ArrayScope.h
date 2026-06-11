#pragma once

#include "FrameMacro.h"

template<typename T>
class ArrayScope
{
public:
	ArrayScope(int length)
	{
		mArray = new T[length];
		memset(mArray, 0, length);
	}
	~ArrayScope()
	{
		delete[] mArray;
		mArray = nullptr;
	}
public:
	T* mArray = nullptr;
};