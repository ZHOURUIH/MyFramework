#ifndef _MY_SAFE_VECTOR_H_
#define _MY_SAFE_VECTOR_H_

#include "mySTL.h"
#include "myVector.h"

// 如果Value是非基础数据类型,则尽量设置为指针,否则在同步时可能无法获取正确的结果
template<typename T>
struct VectorModify
{
	T mValue;
	bool mAdd;
	VectorModify(const T& value, bool add)
	{
		mValue = value;
		mAdd = add;
	}
};

template<typename T>
class mySafeVector : public mySTL
{
public:
	mySafeVector()
	{
		mForeaching = false;
	}
	virtual ~mySafeVector(){ clear(); }
	const myVector<T>& startForeach()
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
					mUpdateList.push_back(modifyValue.mValue);
				}
				else
				{
					mUpdateList.eraseElement(modifyValue.mValue);
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
	const myVector<T>& getMainList() const { return mMainList; }
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
	uint size() const { return mMainList.size(); }
	T* data() const { return mMainList.data(); }
protected:
	myVector<VectorModify<T>> mModifyList;	// 记录操作的列表
	myVector<T> mUpdateList;				// 用于遍历更新的列表
	myVector<T> mMainList;					// 用于存储实时数据的列表
	bool mForeaching;
};

#endif