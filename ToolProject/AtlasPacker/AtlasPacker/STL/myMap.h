#ifndef _MY_MAP_H_
#define _MY_MAP_H_

#include "mySTL.h"
#include <map>

template<typename Key, typename Value>
class myMap : public mySTL
{
public:
	typedef typename map<Key, Value>::iterator iterator;
	typedef typename map<Key, Value>::reverse_iterator reverse_iterator;
	typedef typename map<Key, Value>::const_iterator const_iterator;
public:
	myMap(){}
	myMap(initializer_list<pair<const Key, Value>> _Ilist)
	{
		mMap.insert(_Ilist);
	}
	virtual ~myMap(){ clear(); }
	reverse_iterator rbegin() {return mMap.rbegin();}
	reverse_iterator rend() {return mMap.rend();}
	iterator begin() {return mMap.begin();}
	iterator end() {return mMap.end();}
	const_iterator cbegin() const { return mMap.cbegin(); }
	const_iterator cend() const { return mMap.cend(); }
	// 获取列表中指定顺序的值,如果获取失败,则返回设置的defaultValue,适用于value是指针类型或者整数类型的列表
	const Value& getAtIndex(int index, const Value& defaultValue) const
	{
		int curIndex = 0;
		FOREACH_CONST(iter, mMap)
		{
			if (index == curIndex)
			{
				return iter->second;
			}
			++curIndex;
		}
		return defaultValue;
	}
	// 尝试获取值的指针,适用于value是非指针类型的列表,由于返回出去的值允许被修改,所以返回值不能添加const,此函数也不是const
	Value* get(const Key& key)
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			return &(iter->second);
		}
		return NULL;
	}
	// 尝试获取值的指针,适用于value是非指针类型的列表,返回出去的值不允许被修改
	const Value* getConst(const Key& key) const
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			return &(iter->second);
		}
		return NULL;
	}
	// 获取值,如果获取失败,则返回设置的defaultValue,适用于value是指针类型或者整数类型的列表
	const Value& get(const Key& key, const Value& defaultValue) const
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			return iter->second;
		}
		return defaultValue;
	}
	void set(const Key& key, const Value& value)
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			iter->second = value;
		}
		else
		{
			insert(key, value);
		}
	}
	const_iterator find(const Key& key) const { return mMap.find(key); }
	iterator find(const Key& key) { return mMap.find(key); }
	bool contains(const Key& key) const { return mMap.find(key) != mMap.end(); }
	// 尝试修改列表中的值
	void trySet(const Key& key, const Value& value, bool* success = NULL)
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			iter->second = value;
			if (success != NULL)
			{
				*success = true;
			}
		}
		else
		{
			if (success != NULL)
			{
				*success = false;
			}
		}
	}
	// 尝试向列表中插入值,插入成功则返回插入值的引用,如果key已经存在,则返回对应的value的引用
	Value& tryInsert(const Key& key, const Value& value, bool* success = NULL)
	{
		auto iter = mMap.find(key);
		if (iter == mMap.end())
		{
			if (success != NULL)
			{
				*success = true;
			}
			return mMap.insert(make_pair(key, value)).first->second;
		}
		if (success != NULL)
		{
			*success = false;
		}
		return iter->second;
	}
	bool insert(const Key& key, const Value& value, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		auto iter = mMap.find(key);
		if (iter == mMap.end())
		{
			mMap.insert(make_pair(key, value));
			return true;
		}
		return false;
	}
	void erase(const iterator& iter, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		mMap.erase(iter);
	}
	// 返回值表示移除成功或失败
	bool erase(const Key& key, bool check = true)
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
#if _DEBUG
			erase(iter, check);
#else
			mMap.erase(iter);
#endif
			return true;
		}
		return false;
	}
	void clear()
	{
#if _DEBUG
		checkLock();
#endif
		mMap.clear();
	}
	bool merge(const myMap<Key, Value>& other)
	{
		FOREACH_CONST(iter, other)
		{
#if _DEBUG
			bool ret = insert(iter->first, iter->second);
#else
			bool ret = mMap.insert(make_pair(iter->first, iter->second)).second;
#endif
			if (!ret)
			{
				return false;
			}
		}
		return true;
	}
	uint size() const {return (uint)mMap.size();}
	Value& operator[](const Key& k) {return mMap[k];}
	void clone(myMap<Key, Value>& target) const
	{
		target.mMap.clear();
		FOREACH_CONST(iter, mMap)
		{
			target.mMap.insert(make_pair(iter->first, iter->second));
		}
#if _DEBUG
		target.resetLock();
#endif
	}
	void keyToList(myVector<Key>& keyList) const
	{
		FOREACH_CONST(iter, mMap)
		{
			keyList.push_back(iter->first);
		}
	}
	bool keyToList(Value* keyList, int maxCount) const
	{
		int index = 0;
		FOREACH_CONST(iter, mMap)
		{
			if (index >= maxCount)
			{
				return false;
			}
			keyList[index++] = iter->first;
		}
		return true;
	}
	void valueToList(myVector<Value>& valueList) const
	{
		FOREACH_CONST(iter, mMap)
		{
			valueList.push_back(iter->second);
		}
	}
	void valueToListFilter(myVector<Value>& valueList, const Value& exceptValue) const
	{
		FOREACH_CONST(iter, mMap)
		{
			if (iter->second != exceptValue)
			{
				valueList.push_back(iter->second);
			}
		}
	}
	template<size_t Length>
	uint valueToListFilter(array<Value, Length>& valueList, const Value& exceptValue, uint startIndex = 0) const
	{
		if (startIndex >= mMap.size())
		{
			return 0;
		}
		uint indexInMap = 0;
		uint curDataCount = 0;
		FOREACH_CONST(iter, mMap)
		{
			if (++indexInMap <= startIndex)
			{
				continue;
			}
			if (curDataCount >= Length)
			{
				break;
			}
			if (iter->second != exceptValue)
			{
				valueList[curDataCount++] = iter->second;
			}
		}
		return curDataCount;
	}
	template<size_t Length>
	uint valueToList(array<Value, Length>& valueList, uint startIndex = 0) const
	{
		if (startIndex >= mMap.size())
		{
			return 0;
		}
		uint indexInMap = 0;
		uint curDataCount = 0;
		FOREACH_CONST(iter, mMap)
		{
			if (++indexInMap <= startIndex)
			{
				continue;
			}
			if (curDataCount >= Length)
			{
				break;
			}
			valueList[curDataCount++] = iter->second;
		}
		return curDataCount;
	}
	uint valueToList(Value* valueList, uint maxCount, uint startIndex = 0) const
	{
		if (startIndex >= mMap.size())
		{
			return 0;
		}
		uint indexInMap = 0;
		uint curDataCount = 0;
		FOREACH_CONST(iter, mMap)
		{
			if (++indexInMap <= startIndex)
			{
				continue;
			}
			if (curDataCount >= maxCount)
			{
				break;
			}
			valueList[curDataCount++] = iter->second;
		}
		return curDataCount;
	}
public:
	map<Key, Value> mMap;
};

#endif