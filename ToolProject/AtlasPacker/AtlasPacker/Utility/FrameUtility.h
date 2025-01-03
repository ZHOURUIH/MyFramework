#ifndef _FRAME_UTILITY_H_
#define _FRAME_UTILITY_H_

#include "SystemUtility.h"

class FrameUtility : public SystemUtility
{
public:
	template<typename T>
	static bool arrayContains(T* list, uint count, const T& value)
	{
		if (list == NULL)
		{
			return false;
		}
		FOR_I(count)
		{
			if (list[i] == value)
			{
				return true;
			}
		}
		return false;
	}
	template<typename Key, typename Value>
	static void keyToList(const myMap<Key, Value>& map, myVector<Key>& keys, bool clear)
	{
		if (clear)
		{
			keys.clear();
		}
		FOREACH_CONST(iter, map)
		{
			keys.push_back(iter->first);
		}
	}
	template<typename Key, typename Value>
	static void valueToList(const myMap<Key, Value>& map, myVector<Value>& values, bool clear = true)
	{
		if (clear)
		{
			values.clear();
		}
		FOREACH_CONST(iter, map)
		{
			values.push_back(iter->second);
		}
	}
};

#endif
