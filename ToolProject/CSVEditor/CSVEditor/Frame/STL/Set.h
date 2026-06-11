#pragma once

#include "UsingSTD.h"
#include "Array.h"
#include "ArrayList.h"
#include "Vector.h"

template<typename T>
class Set
{
public:
	typedef typename set<T>::iterator iterator;
	typedef typename set<T>::reverse_iterator reverse_iterator;
	typedef typename set<T>::const_iterator const_iterator;
public:
	virtual ~Set() { mSet.clear(); }
	Set() = default;
	Set(const Set<T>& other) :
		mSet(other.mSet)
	{
#ifdef WIN32
		if (mSet.size() > 0)
		{
			LOG("Set请尝试使用移动构造,减少拷贝构造");
		}
#endif
	}
	Set(Set<T>&& other) noexcept :
		mSet(move(other.mSet))
	{}
	Set<T>& operator=(Set<T>&& other) noexcept
	{
		mSet = move(other.mSet);
		return *this;
	}
	Set<T>& operator=(const Set<T>& other)
	{
		mSet = other.mSet;
#ifdef WIN32
		LOG("Set请尝试使用移动赋值,减少拷贝赋值");
#endif
		return *this;
	}
	iterator find(const T& elem) const	{ return mSet.find(elem); }
	iterator begin()					{ return mSet.begin(); }
	iterator end()						{ return mSet.end(); }
	const_iterator begin() const		{ return mSet.begin(); }
	const_iterator end() const			{ return mSet.end(); }
	reverse_iterator rbegin()			{ return mSet.rbegin(); }
	reverse_iterator rend()				{ return mSet.rend(); }
	const_iterator cbegin() const		{ return mSet.cbegin(); }
	const_iterator cend() const			{ return mSet.cend(); }
	bool isEmpty() const				{ return mSet.size() == 0; }
	// 返回第一个元素,并且将该元素移除
	T pop_front()
	{
		auto iter = mSet.begin();
		const T value = *iter;
		mSet.erase(iter);
		return value;
	}
	template<int Length>
	void setData(const Array<Length, T>& values, const int count)
	{
		FOR(count)
		{
			mSet.insert(values[i]);
		}
	}
	template<int Length>
	void setData(const ArrayList<Length, T>& values)
	{
		FOR(values.size())
		{
			mSet.insert(values[i]);
		}
	}
	void setData(const T* values, const int count)
	{
		FOR(count)
		{
			mSet.insert(values[i]);
		}
	}
	void setData(const Vector<T>& values)
	{
		for (const auto& value : values)
		{
			mSet.insert(value);
		}
	}
	bool insert(const T& elem)
	{
		return mSet.insert(elem).second;
	}
	// 因为要兼容T类型的派生类,所以定义了一个新的类型
	template<typename TElement>
	void insert(const Set<TElement>& other)
	{
		if (other.size() == 0)
		{
			return;
		}
		for (const TElement& iter : other)
		{
			mSet.insert(iter);
		}
	}
	// 因为要兼容T类型的派生类,所以定义了一个新的类型
	template<typename TElement>
	void insert(const Vector<TElement>& other)
	{
		if (other.size() == 0)
		{
			return;
		}
		for (const TElement& iter : other)
		{
			mSet.insert(iter);
		}
	}
	void erase(const iterator& iter)
	{
		mSet.erase(iter);
	}
	bool erase(const T& value)
	{
		iterator iter = mSet.find(value);
		if (iter != mSet.end())
		{
			mSet.erase(iter);
			return true;
		}
		return false;
	}
	bool contains(const T& value) const { return mSet.find(value) != mSet.end(); }
	void clear()
	{
		if (mSet.size() == 0)
		{
			return;
		}
		mSet.clear();
	}
	int size() const { return (int)mSet.size(); }
	// 添加克隆函数的目的是为了显式调用拷贝,而非自动调用拷贝,可以避免可以使用移动构造而没有使用的情况
	void clone(Set<T>& target) const
	{
		target.mSet = mSet;
	}
public:
	set<T> mSet;
private:
	static const Set<T> mDefaultList;
};

template<typename T>
const Set<T> Set<T>::mDefaultList;