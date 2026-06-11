#pragma once

#include "UsingSTD.h"
#include "Set.h"
#include "Vector.h"
#include "ValueModify.h"

template<typename T>
class SafeSet
{
public:
	typedef Set<T> List;
	typedef T ValueType;
public:
	virtual ~SafeSet() { clear(); }
	const Set<T>& startForeach()
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
	bool isForeaching() const			{ return mForeaching; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 当遍历过程中不能确定列表一定不会被修改时,都应该使用startForeach进行遍历
	// 当确定遍历过程中列表不会被修改时,才会使用getMainList进行过遍历
	const Set<T>& getMainList() const	{ return mMainList; }
	int size() const					{ return mMainList.size(); }
	T* data() const						{ return mMainList.data(); }
	bool contains(const T& value) const { return mMainList.contains(value); }
	bool insert(const T& value)
	{
		if (!mMainList.insert(value))
		{
			return false;
		}
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			mModifyList.emplace_back(value, true);
		}
		// 没有在遍历,则对mUpdateList做与mMainList相同的操作
		else
		{
			mUpdateList.insert(value);
		}
		return true;
	}
	bool erase(const T& value)
	{
		if (!mMainList.erase(value))
		{
			return false;
		}
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			mModifyList.emplace_back(value, false);
		}
		else
		{
			mUpdateList.erase(value);
		}
		return true;
	}
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
						mUpdateList.insert(modifyValue.mValue);
					}
					else
					{
						mUpdateList.erase(modifyValue.mValue);
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
	Vector<ValueModify<T>> mModifyList;	// 记录操作的列表
	Set<T> mUpdateList;					// 用于遍历更新的列表
	Set<T> mMainList;					// 用于存储实时数据的列表
	bool mForeaching = false;			// 是否正在遍历中
};