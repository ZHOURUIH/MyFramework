#ifndef _MY_SET_H_
#define _MY_SET_H_

#include "mySTL.h"
#include <set>

template<typename T>
class mySet : public mySTL
{
public:
	typedef typename set<T>::iterator iterator;
	typedef typename set<T>::reverse_iterator reverse_iterator;
	typedef typename set<T>::const_iterator const_iterator;
public:
	mySet() {}
	virtual ~mySet() { clear(); }
	iterator find(const T& elem) const { return mSet.find(elem); }
	iterator begin() { return mSet.begin(); }
	iterator end() { return mSet.end(); }
	reverse_iterator rbegin() { return mSet.rbegin(); }
	reverse_iterator rend() { return mSet.rend(); }
	const_iterator cbegin() const { return mSet.cbegin(); }
	const_iterator cend() const { return mSet.cend(); }
	// 返回第一个元素,并且将该元素移除
	T pop_front(bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		auto iter = mSet.begin();
		T value = *iter;
		mSet.erase(iter);
		return value;
	}
	void setData(const T* values, uint count, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		FOR_I(count)
		{
			mSet.insert(values[i]);
		}
	}
	bool insert(const T& elem)
	{
#if _DEBUG
		checkLock();
#endif
		return mSet.insert(elem).second;
	}
	void erase(const iterator& iter, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mSet.erase(iter);
	}
	bool erase(const T& value, bool check = true)
	{
		iterator iter = mSet.find(value);
		if (iter != mSet.end())
		{
#if _DEBUG
			erase(iter, check);
#else
			mSet.erase(iter);
#endif
			return true;
		}
		return false;
	}
	bool contains(const T& value) const { return mSet.find(value) != mSet.end(); }
	void clear()
	{
#if _DEBUG
		checkLock();
#endif
		mSet.clear();
	}
	uint size() const { return (uint)mSet.size(); }
	void clone(mySet<T>& target) const
	{
		target.mSet.clear();
		FOREACH_CONST(iter, mSet)
		{
			target.mSet.insert(*iter);
		}
#if _DEBUG
		target.resetLock();
#endif
	}
	bool merge(const mySet<T>& other)
	{
		FOREACH_CONST(iter, other)
		{
			if (!mSet.insert(*iter).second)
			{
				return false;
			}
		}
		return true;
	}
public:
	set<T> mSet;
};

#endif