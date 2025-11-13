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
	const_iterator begin() const { return mMap.begin(); }
	const_iterator end() const { return mMap.end(); }
	const Value& get(const Key& k, const Value& defaultValue) const
	{
		auto iter = mMap.find(k);
		if (iter != mMap.end())
		{
			return iter->second;
		}
		return defaultValue;
	}
	void set(const Key& k, const Value& value)
	{
		auto iter = mMap.find(k);
		if (iter != mMap.end())
		{
			iter->second = value;
		}
		else
		{
			insert(k, value);
		}
	}
	const_iterator find(const Key& k) const { return mMap.find(k); }
	iterator find(const Key& k) {return mMap.find(k);}
	bool contains(const Key& key) const {return mMap.find(key) != mMap.end();}
	bool insert(const Key& k, const Value& value, bool check = true)
	{
#if _DEBUG
		if (check)
		{
			checkLock();
		}
#endif
		auto iter = mMap.find(k);
		if (iter == mMap.end())
		{
			mMap.insert(make_pair(k, value));
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
	int size() const { return (int)mMap.size(); }
	Value& operator[](const Key& k) { return mMap[k]; }
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
public:
	map<Key, Value> mMap;
};

#endif