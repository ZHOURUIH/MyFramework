#ifndef _SAFE_VECTOR_H_
#define _SAFE_VECTOR_H_

#include "mySTL.h"
#include "Vector.h"

// 如果Value是非基础数据类型,则尽量设置为指针,否则在同步时可能无法获取正确的结果
template<typename T>
struct VectorModify
{
	T mValue;
	bool mAdd;
	VectorModify(const T& value, bool add):
		mValue(value),
		mAdd(add)
	{}
};

template<typename T>
class SafeVector : public mySTL
{
public:
	SafeVector():
		mForeaching(false)
	{}
	virtual ~SafeVector(){ clear(); }
	const Vector<T>& startForeach()
	{
		if (mForeaching)
		{
			IndependentLog::directError("正在遍历列表,无法再次开始遍历");
			return mUpdateList;
		}
		mForeaching = true;

		uint mainCount = mMainList.size();
		uint modifyCount = mModifyList.size();
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
				FOR_I(modifyCount)
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
				mUpdateList.clear();
				mUpdateList.merge(mMainList);
			}
		}
		mModifyList.clear();

		if (mUpdateList.size() != mainCount)
		{
			IndependentLog::directError("同步失败");
			mUpdateList.clear();
			mUpdateList.merge(mMainList);
		}
		return mUpdateList;
	}
	void endForeach()						{ mForeaching = false; }
	bool isForeaching() const				{ return mForeaching; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用GetUpdateList
	const Vector<T>& getMainList() const	{ return mMainList; }
	uint size() const						{ return mMainList.size(); }
	T* data() const							{ return mMainList.data(); }
	bool conains(const T& value) const		{ return mMainList.contains(value); }
	void push_back(const T& value)
	{
		mMainList.push_back(value);
		mModifyList.push_back(VectorModify<T>(value, true));
	}
	void eraseElement(const T& value)
	{
		if (!mMainList.eraseElement(value))
		{
			return;
		}
		mModifyList.push_back(VectorModify<T>(value, false));
	}
	// 清空所有数据,不能正在遍历时调用
	void clear()
	{
		mMainList.clear();
		mUpdateList.clear();
		mModifyList.clear();
	}
protected:
	Vector<VectorModify<T>> mModifyList;	// 记录操作的列表
	Vector<T> mUpdateList;					// 用于遍历更新的列表
	Vector<T> mMainList;					// 用于存储实时数据的列表
	bool mForeaching;
};

#endif