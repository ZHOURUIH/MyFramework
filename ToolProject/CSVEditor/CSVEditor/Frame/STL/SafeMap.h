#pragma once

#include "UsingSTD.h"
#include "Map.h"
#include "Vector.h"
#include "MapModify.h"

template<typename Key, typename Value>
class SafeMap
{
public:
	typedef Map<Key, Value> List;
	typedef Key KeyType;
	typedef Value ValueType;
public:
	virtual ~SafeMap() { clear(); }
	const Map<Key, Value>& startForeach()
	{
		if (mForeaching)
		{
			ERROR("正在遍历列表,无法再次开始遍历");
		}
		mForeaching = true;
		return mUpdateList;
	}
	void endForeach()
	{
		mForeaching = false;
		// 在结束遍历时同步一次
		if (mModifyList.size() > 0)
		{
			sync();
		}
	}
	bool isForeaching() const											{ return mForeaching; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 当遍历过程中不能确定列表一定不会被修改时,都应该使用startForeach进行遍历
	// 当确定遍历过程中列表不会被修改时,才会使用getMainList进行过遍历
	const Map<Key, Value>& getMainList() const							{ return mMainList; }
	// 尝试获取值的指针,适用于value是非指针类型的列表
	Value* getPtr(const Key& key)										{ return mMainList.getPtr(key); }
	const Value* getPtrConst(const Key& key) const						{ return mMainList.getPtrConst(key); }
	// 获取值,如果获取失败,则返回设置的defaultValue,适用于value是指针类型或者整数类型的列表
	const Value& tryGet(const Key& key, const Value& defaultValue) const{ return mMainList.tryGet(key, defaultValue); }
	const Value& tryGet(const Key& key) const							{ return mMainList.tryGet(key); }
	bool contains(const Key& key) const									{ return mMainList.contains(key); }
	int size() const													{ return mMainList.size(); }
	bool insert(const Key& key, const Value& value)
	{
		if (!mMainList.insert(key, value))
		{
			return false;
		}
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			mModifyList.emplace_back(key, value);
		}
		// 没有在遍历,则对mUpdateList做与mMainList相同的操作
		else
		{
			mUpdateList.insert(key, value);
		}
		return true;
	}
	bool erase(const Key& key)
	{
		if (!mMainList.erase(key))
		{
			return false;
		}
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			mModifyList.emplace_back(key);
		}
		else
		{
			mUpdateList.erase(key);
		}
		return true;
	}
	// Safe类型的容器不能使用tryInsert等返回引用的函数,这样会导致遍历的列表和存储的列表不一致,因为没办法将两个列表的引用返回出去
	// 清空所有数据,不能正在遍历时调用
	void clear()
	{
		if (mForeaching)
		{
			ERROR("正在遍历列表,无法清空列表");
		}
		mMainList.clear();
		mUpdateList.clear();
		mModifyList.clear();
	}
protected:
	void sync()
	{
		const int modifyCount = mModifyList.size();
		const int mainCount = mMainList.size();
		// 主列表为空,则直接清空列表即可
		if (mainCount == 0)
		{
			mUpdateList.clear();
		}
		else
		{
			// 更新操作较少,则遍历更新操作列表进行数据同步
			if (modifyCount < mainCount)
			{
				FOR(modifyCount)
				{
					auto& modifyValue = mModifyList[i];
					if (modifyValue.mAdd)
					{
						mUpdateList.insert(modifyValue.mKey, modifyValue.mValue);
					}
					else
					{
						mUpdateList.erase(modifyValue.mKey);
					}
				}
			}
			// 更新操作较多,则直接复制列表
			else
			{
				mMainList.clone(mUpdateList);
			}
		}
		mModifyList.clear();
	}
protected:
	Vector<MapModify<Key, Value>> mModifyList;	// 记录操作的列表
	Map<Key, Value> mUpdateList;				// 用于遍历更新的列表
	Map<Key, Value> mMainList;					// 用于存储实时数据的列表
	bool mForeaching = false;					// 是否正在遍历中
};