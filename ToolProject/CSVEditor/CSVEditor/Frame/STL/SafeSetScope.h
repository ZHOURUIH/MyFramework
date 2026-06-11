#pragma once

#include "UsingSTD.h"
#include "Set.h"
#include "SafeSet.h"

// 需要通过SAFE_SET_SCOPE宏来使用
template<typename T>
class SafeSetScope
{
protected:
	SafeSet<T>* mList = nullptr;
public:
	SafeSetScope(SafeSet<T>& list)
	{
		mList = &list;
	}
	~SafeSetScope()
	{
		mList->endForeach();
	}
};