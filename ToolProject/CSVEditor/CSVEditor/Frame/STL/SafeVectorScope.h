#pragma once

#include "UsingSTD.h"
#include "Vector.h"
#include "SafeVector.h"

// 需要通过SAFE_VECTOR_SCOPE宏来使用
template<typename T>
class SafeVectorScope
{
protected:
	SafeVector<T>* mList = nullptr;
public:
	SafeVectorScope(SafeVector<T>& list)
	{
		mList = &list;
	}
	~SafeVectorScope()
	{
		mList->endForeach();
	}
};