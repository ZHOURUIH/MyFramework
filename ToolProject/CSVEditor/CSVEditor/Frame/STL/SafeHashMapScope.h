#pragma once

#include "HashMap.h"
#include "SafeHashMap.h"

// 需要通过SAFE_HASHMAP_SCOPE宏来使用
template<typename Key, typename Value>
class SafeHashMapScope
{
protected:
	SafeHashMap<Key, Value>* mList = nullptr;
public:
	SafeHashMapScope(SafeHashMap<Key, Value>& list)
	{
		mList = &list;
	}
	~SafeHashMapScope()
	{
		mList->endForeach();
	}
};