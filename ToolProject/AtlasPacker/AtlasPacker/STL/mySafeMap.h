#ifndef _MY_SAFE_MAP_H_
#define _MY_SAFE_MAP_H_

#include "mySTL.h"
#include "myMap.h"
#include "myVector.h"

// 如果Value是非基础数据类型,则尽量设置为指针,否则在同步时可能无法获取正确的结果
template<typename Key, typename Value>
struct MapModify
{
	Key mKey;
	Value mValue;
	bool mAdd;
	MapModify(const Key& key)
	{
		mKey = key;
		mValue = (Value)(0);
		mAdd = false;
	}
	MapModify(const Key& key, const Value& value)
	{
		mKey = key;
		mValue = value;
		mAdd = true;
	}
};

template<typename Key, typename Value>
class mySafeMap : public mySTL
{
public:
	mySafeMap() 
	{
		mForeaching = false;
	}
	virtual ~mySafeMap() { clear(); }
	const myMap<Key, Value>& startForeach()
	{
		if (mForeaching)
		{
			directError("正在遍历列表,无法再次开始遍历");
			return mUpdateList;
		}
		mForeaching = true;

		uint modifyCount = mModifyList.size();
		if (modifyCount < mMainList.size())
		{
			FOR_I(modifyCount)
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
		else
		{
			mUpdateList.clear();
			mUpdateList.merge(mMainList);
		}
		mModifyList.clear();

		if (mUpdateList.size() != mMainList.size())
		{
			//ERROR("同步失败");
			mUpdateList.clear();
			mUpdateList.merge(mMainList);
		}
		return mUpdateList;
	}
	void endForeach() { mForeaching = false; }
	bool isForeaching() const { return mForeaching; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用GetUpdateList
	const myMap<Key, Value>& getMainList() const { return mMainList; }
	bool insert(const Key& key, const Value& value)
	{
		if (!mMainList.insert(key, value))
		{
			return false;
		}
		mModifyList.push_back(MapModify<Key, Value>(key, value));
		return true;
	}
	bool erase(const Key& key)
	{
		if (!mMainList.erase(key))
		{
			return false;
		}
		mModifyList.push_back(MapModify<Key, Value>(key));
		return true;
	}
	Value& tryInsert(const Key& key, const Value& value, bool* success = NULL)
	{
		bool curSuccess = false;
		Value& outValue = mMainList.tryInsert(key, value, &curSuccess);
		if (curSuccess)
		{
			mModifyList.push_back(MapModify<Key, Value>(key, value));
		}
		if (success != NULL)
		{
			*success = curSuccess;
		}
		return outValue;
	}
	// 尝试获取值的指针,适用于value是非指针类型的列表
	Value* get(const Key& key) { return mMainList.get(key); }
	const Value* getConst(const Key& key) const { return mMainList.getConst(key); }
	// 获取值,如果获取失败,则返回设置的defaultValue,适用于value是指针类型或者整数类型的列表
	const Value& get(const Key& key, const Value& defaultValue) const { return mMainList.get(key, defaultValue); }
	bool contains(const Key& key) const { return mMainList.contains(key); }
	// 清空所有数据,不能正在遍历时调用
	void clear()
	{
		mMainList.clear();
		mUpdateList.clear();
		mModifyList.clear();
	}
	void keyToList(myVector<Key>& keyList) const
	{
		mMainList.keyToList(keyList);
	}
	bool keyToList(Value* keyList, int maxCount) const
	{
		return mMainList.keyToList(keyList, maxCount);
	}
	void valueToList(myVector<Value>& valueList) const
	{
		mMainList.valueToList(valueList);
	}
	void valueToListFilter(myVector<Value>& valueList, const Value& exceptValue) const
	{
		mMainList.valueToListFilter(valueList, exceptValue);
	}
	template<size_t Length>
	uint valueToList(array<Value, Length>& valueList, uint startIndex = 0) const
	{
		return mMainList.valueToList(valueList, startIndex);
	}
	uint valueToList(Value* valueList, uint maxCount, uint startIndex = 0) const
	{
		return mMainList.valueToList(valueList, maxCount, startIndex);
	}
	uint size() const { return mMainList.size(); }
protected:
	myVector<MapModify<Key, Value>> mModifyList;// 记录操作的列表
	myMap<Key, Value> mUpdateList;				// 用于遍历更新的列表
	myMap<Key, Value> mMainList;				// 用于存储实时数据的列表
	bool mForeaching;
};

#endif