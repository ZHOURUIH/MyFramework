#ifndef _THREAD_ARRAY_MEMORY_H_
#define _THREAD_ARRAY_MEMORY_H_

#include "FrameDefine.h"
#include "SystemUtility.h"

class ThreadArrayMemory : public SystemUtility
{
public:
	virtual ~ThreadArrayMemory() { destroy(); }
	void destroy();
	template<typename T>
	T* get(uint count, bool zeroMemory)
	{
		uint type = (uint)typeid(T).hash_code();
		void* data = NULL;
		// 尝试从未使用列表中查找
		if (mUnuseMemoryList.contains(type))
		{
			auto& countList = mUnuseMemoryList[type];
			if (countList.contains(count))
			{
				auto& memList = countList[count];
				if (memList.size() > 0)
				{
					data = *(memList.begin());
					// 从未使用列表移除
					removeUnuse(type, data, count);
				}
			}
		}
		// 如果未使用列表中没有找到可以重复使用的,则创建一个
		if (data == NULL)
		{
			data = NEW_ARRAY(T, count, data);
			// 添加到申请列表中
			mMemorySearchMap.insert(data, make_pair(type, count));
		}
		// 添加到使用列表中
		addInuse(type, data, count);
		if (zeroMemory)
		{
			memset(data, 0, count * sizeof(T));
		}
		return (T*)data;
	}
	void release(void* data);
protected:
	void addInuse(uint type, void* data, uint count);
	void addUnuse(uint type, void* data, uint count);
	void removeInuse(uint type, void* data, uint count);
	void removeUnuse(uint type, void* data, uint count);
public:
	myMap<uint, myMap<uint, mySet<void*>>> mInuseMemoryList;	// 第一个uint是元素类型hash值,第二个uint是数组长度
	myMap<uint, myMap<uint, mySet<void*>>> mUnuseMemoryList;	// 第一个uint是元素类型hash值,第二个uint是数组长度
	myMap<void*, pair<uint, uint>> mMemorySearchMap;			// 存放所有申请过的数组的列表,string是数组元素类型hash值,uint是数组元素个数
};

#endif