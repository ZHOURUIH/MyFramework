#ifndef _MY_VECTOR_H_
#define _MY_VECTOR_H_

#include "mySTL.h"
#include <vector>

template<typename T>
class myVector : public mySTL
{
public:
	typedef typename vector<T>::iterator iterator;
	typedef typename vector<T>::reverse_iterator reverse_iterator;
	typedef typename vector<T>::const_iterator const_iterator;
public:
	myVector() {}
	myVector(initializer_list<T> _Ilist)
	{
		mVector.insert(mVector.begin(), _Ilist);
	}
	virtual ~myVector() { clear(); }
	T* data() const { return (T*)mVector.data(); }
	uint size() const { return (uint)mVector.size(); }
	iterator begin() { return mVector.begin(); }
	iterator end() { return mVector.end(); }
	reverse_iterator rbegin() { return mVector.rbegin(); }
	reverse_iterator rend() { return mVector.rend(); }
	const_iterator cbegin() const { return mVector.cbegin(); }
	const_iterator cend() const { return mVector.cend(); }
	// 返回最后一个元素,并且将该元素移除
	T pop(bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		uint curSize = size();
		T value = mVector[curSize - 1];
		mVector.erase(mVector.begin() + curSize - 1);
		return value;
	}
	void addRange(const T* values, uint count, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		int oldSize = mVector.size();
		mVector.resize(oldSize + count);
		MEMCPY(mVector.data() + oldSize, count * sizeof(T), values, count * sizeof(T));
	}
	void setData(const T* values, uint count, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mVector.clear();
		mVector.resize(count);
		MEMCPY(mVector.data(), count * sizeof(T), values, count * sizeof(T));
	}
	void push_back(const T& elem, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mVector.push_back(elem);
	}
	iterator erase(const iterator& iter, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		return mVector.erase(iter);
	}
	iterator erase(const iterator& iter, const iterator& end, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		return mVector.erase(iter, end);
	}
	iterator erase(uint index, uint count = 1, bool check = true)
	{
#if _DEBUG
		if (index < 0 || index >= (int)mVector.size())
		{
			return mVector.end();
		}
		return erase(mVector.begin() + index, mVector.begin() + index + count, check);
#else
		return mVector.erase(mVector.begin() + index, mVector.begin() + index + count);
#endif
	}
	void eraseAll(const T& value, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		for (uint i = 0; i < (uint)mVector.size(); )
		{
			if (mVector[i] == value)
			{
				mVector.erase(mVector.begin() + i);
			}
			else
			{
				++i;
			}
		}
	}
	bool eraseElement(const T& value, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		uint count = (uint)mVector.size();
		FOR_I(count)
		{
			if (mVector[i] == value)
			{
				mVector.erase(mVector.begin() + i);
				return true;
			}
		}
		return false;
	}
	void clear()
	{
#if _DEBUG
		checkLock();
#endif
		mVector.clear();
	}
	void merge(const myVector<T>& other)
	{
		auto& otherVector = other.mVector;
		uint count = (uint)otherVector.size();
		FOR_I(count)
		{
#if _DEBUG
			push_back(other[i]);
#else
			mVector.push_back(otherVector[i]);
#endif
		}
	}
	void insert(int index, const T& elem, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mVector.insert(mVector.begin() + index, elem);
	}
	void insert(const iterator& iter, const T& elem, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mVector.insert(iter, elem);
	}
	const T& operator[](uint i) const
	{
		if (i < 0 || i >= (uint)mVector.size())
		{
			// 因为函数为const属性,但是directError不是const属性,所以此处调用directError会报错
			//directError("vector index out of range!");
		}
		return mVector[i];
	}
	T& operator[](uint i)
	{
		if (i < 0 || i >= (uint)mVector.size())
		{
			directError("vector index out of range!");
		}
		return mVector[i];
	}
	// 同时修改capacity和size
	void resize(uint s, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mVector.resize(s);
	}
	// 只增加capacity,不改变当前size
	void reserve(int size)
	{
		mVector.reserve(size);
	}
	bool contains(const T& value) const
	{
		return std::find(mVector.begin(), mVector.end(), value) != mVector.end();
	}
	int find(const T& value, uint startIndex = 0)
	{
		uint count = (uint)mVector.size();
		for(uint i = startIndex; i < count; ++i)
		{
			if (mVector[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
	void clone(myVector<T>& target) const
	{
		uint curSize = (uint)mVector.size();
		auto& temp = target.mVector;
		temp.clear();
		temp.resize(curSize);
		FOR_I(curSize)
		{
			temp[i] = mVector[i];
		}
#if _DEBUG
		target.resetLock();
#endif
	}
	void shrink_to_fit()
	{
		mVector.shrink_to_fit();
	}
public:
	vector<T> mVector;
};

#endif