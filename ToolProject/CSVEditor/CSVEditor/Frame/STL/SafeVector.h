#pragma once

#include "UsingSTD.h"
#include "Vector.h"
#include "ValueModify.h"

template<typename T>
class SafeVector
{
public:
	typedef Vector<T> List;
	typedef T ValueType;
public:
	virtual ~SafeVector() { clear(); }
	const Vector<T>& startForeach()
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
		if (mHasResetCount > 0)
		{
			mUpdateList.clearDefaultElement(mHasResetCount);
			mHasResetCount = 0;
		}
	}
	bool isForeaching() const				{ return mForeaching; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 当遍历过程中不能确定列表一定不会被修改时,都应该使用startForeach进行遍历
	// 当确定遍历过程中列表不会被修改时,才会使用getMainList进行过遍历
	const Vector<T>& getMainList() const	{ return mMainList; }
	void moveMainListTo(Vector<T>& target)	
	{
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			for (const T& item : mMainList)
			{
				mModifyList.emplace_back(item, false);
			}
		}
		else
		{
			mUpdateList.clear();
		}
		target = move(mMainList);
	}
	int size() const						{ return mMainList.size(); }
	T* data() const							{ return mMainList.data(); }
	bool conains(const T& value) const		{ return mMainList.contains(value); }
	void push_back(const T& value)
	{
		mMainList.push_back(value);
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			mModifyList.emplace_back(value, true);
		}
		else
		{
			mUpdateList.push_back(value);
		}
	}
	bool eraseAt(const int index)
	{
		if (index < 0 || index >= mMainList.size())
		{
			return false;
		}
		// 只有当正在遍历中对列表进行修改时,才会记录修改操作
		if (mForeaching)
		{
			mModifyList.emplace_back(mMainList[index], false);
		}
		else
		{
			mUpdateList.eraseElement(mMainList[index]);
		}
		mMainList.eraseAt(index);
		return true;
	}
	// 在遍历中移除元素有两种选择
	// 一种是直接移除,但是重置之后这一帧仍然可能会对移除的元素进行遍历,因为不一定移除的是当前元素之前还是之后的
	// 另外一种是重置元素为默认值,比如设置为空指针,由于不会修改列表,所以可以立即执行,重置之后这一帧就不会再去遍历
	// 不过在重置时,结束遍历时需要将列表中所有已经重置的元素进行清除,确保列表中不会有多余的元素
	// 而且重置的效率实际上比较低,因为每一次的遍历结束后都要再次遍历一次,相当于遍历了两次,但是运行结果是最准确的
	bool eraseElement(const T& value)
	{
		if (!mMainList.eraseElement(value))
		{
			return false;
		}
		// 正在遍历中,则只是重置元素
		if (mForeaching)
		{
			mUpdateList.resetElement(value);
			++mHasResetCount;
		}
		// 不在遍历中,则可以直接移除
		else
		{
			mUpdateList.eraseElement(value);
		}
		return true;
	}
	// 清空所有数据
	void clear()
	{
		if (mForeaching)
		{
			FOR_VECTOR(mMainList)
			{
				mModifyList.emplace_back(mMainList[i], false);
			}
		}
		else
		{
			mModifyList.clear();
			mUpdateList.clear();
		}
		mMainList.clear();
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
						mUpdateList.push_back(modifyValue.mValue);
					}
					else
					{
						mUpdateList.eraseElement(modifyValue.mValue);
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
	Vector<T> mUpdateList;				// 用于遍历更新的列表
	Vector<T> mMainList;				// 用于存储实时数据的列表
	bool mForeaching = false;			// 是否正在遍历中
	int mHasResetCount = 0;				// 在遍历中被重置的元素个数
};