#include "ThreadArrayMemory.h"
#include "Utility.h"

void ThreadArrayMemory::destroy()
{
	FOREACH(iter, mMemorySearchMap)
	{
		char* data = (char*)iter->first;
		DELETE_ARRAY(data);
	}
	mMemorySearchMap.clear();
	mInuseMemoryList.clear();
	mUnuseMemoryList.clear();
}

void ThreadArrayMemory::release(void* data)
{
	if (mMemorySearchMap.contains(data))
	{
		auto& info = mMemorySearchMap[data];
		removeInuse(info.first, data, info.second);
		addUnuse(info.first, data, info.second);
	}
	else
	{
		ERROR("无法释放不从ArrayPool中申请的内存");
	}
}

void ThreadArrayMemory::addInuse(uint type, void* data, uint count)
{
	if (!mInuseMemoryList.contains(type))
	{
		mInuseMemoryList.insert(type, myMap<uint, mySet<void*>>());
	}
	auto& list = mInuseMemoryList[type];
	if (!list.contains(count))
	{
		list.insert(count, mySet<void*>());
	}
	if (!list[count].insert(data))
	{
		ERROR("已使用列表中已经存在该值");
	}
}

void ThreadArrayMemory::addUnuse(uint type, void* data, uint count)
{
	if (!mUnuseMemoryList.contains(type))
	{
		mUnuseMemoryList.insert(type, myMap<uint, mySet<void*>>());
	}
	auto& list = mUnuseMemoryList[type];
	if (!list.contains(count))
	{
		list.insert(count, mySet<void*>());
	}
	if (!list[count].insert(data))
	{
		ERROR("未使用列表中已经存在该值");
	}
}

void ThreadArrayMemory::removeInuse(uint type, void* data, uint count)
{
	if (!mInuseMemoryList[type][count].erase(data))
	{
		ERROR("已使用列表中找不到该值");
	}
}

void ThreadArrayMemory::removeUnuse(uint type, void* data, uint count)
{
	if (!mUnuseMemoryList[type][count].erase(data))
	{
		ERROR("未使用列表中找不到该值");
	}
}