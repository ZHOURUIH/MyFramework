#pragma once

#include "UsingSTD.h"

template<typename Key, typename Value>
class HashMap
{
public:
	typedef typename unordered_map<Key, Value>::iterator iterator;
	typedef typename unordered_map<Key, Value>::const_iterator const_iterator;
public:
	HashMap():
		mMap(16)
	{
		mMap.max_load_factor(0.25f);
	}
	explicit HashMap(int bucket):
		mMap(bucket)
	{
		mMap.max_load_factor(0.25f);
	}
	HashMap(const HashMap<Key, Value>& other):
		mMap(other.mMap)
	{
#ifdef WIN32
		if (mMap.size() > 0)
		{
			LOG("HashMap请尝试使用移动构造,减少拷贝构造");
		}
#endif
	}
	HashMap(HashMap<Key, Value>&& other) noexcept :
		mMap(move(other.mMap))
	{}
	explicit HashMap(initializer_list<pair<const Key, Value>> list)
	{
		mMap.insert(list);
	}
	HashMap<Key, Value>& operator=(HashMap<Key, Value>&& other) noexcept
	{
		mMap = move(other.mMap);
		return *this;
	}
	HashMap<Key, Value>& operator=(const HashMap<Key, Value>& other)
	{
		mMap = other.mMap;
#ifdef WIN32
		LOG("HashMap请尝试使用移动赋值,减少拷贝赋值");
#endif
		return *this;
	}
	virtual ~HashMap() { mMap.clear(); }
	iterator begin()					{ return mMap.begin(); }
	iterator end()						{ return mMap.end(); }
	const_iterator begin() const		{ return mMap.begin(); }
	const_iterator end() const			{ return mMap.end(); }
	const_iterator cbegin() const		{ return mMap.cbegin(); }
	const_iterator cend() const			{ return mMap.cend(); }
	// 获取列表中指定顺序的值,如果获取失败,则返回设置的defaultValue,适用于value是指针类型或者整数类型的列表
	const Value& getAtIndex(const int index, const Value& defaultValue) const
	{
		int curIndex = 0;
		for (const auto& iter : mMap)
		{
			if (index == curIndex++)
			{
				return iter.second;
			}
		}
		return defaultValue;
	}
	// 尝试获取值的指针,适用于value是非指针类型的列表,由于返回出去的值允许被修改,所以返回值不能添加const,此函数也不是const
	Value* getPtr(const Key& key)
	{
		auto iter = mMap.find(key);
		return iter != mMap.end() ? &(iter->second) : nullptr;
	}
	// 尝试获取值的指针,适用于value是非指针类型的列表,返回出去的值不允许被修改
	const Value* getPtrConst(const Key& key) const
	{
		auto iter = mMap.find(key);
		return iter != mMap.end() ? &(iter->second) : nullptr;
	}
	// 获取值,如果获取失败,则返回设置的defaultValue,适用于value是指针类型或者整数类型的列表
	const Value& tryGet(const Key& key, const Value& defaultValue) const
	{
		auto iter = mMap.find(key);
		return iter != mMap.end() ? iter->second : defaultValue;
	}
	// 获取值,如果获取失败,则返回默认值,适用于value是类对象,可以自动调用构造的
	const Value& tryGet(const Key& key) const
	{
		auto iter = mMap.find(key);
		return iter != mMap.end() ? iter->second : mEmptyValue;
	}
	// 获取值,如果获取失败,则报错,且返回默认值,适用于value是类对象,可以自动调用构造的
	const Value& get(const Key& key) const
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			return iter->second;
		}
		ERROR("failed to get value from HashMap!");
		return mEmptyValue;
	}
	const_iterator find(const Key& key) const { return mMap.find(key); }
	iterator find(const Key& key) { return mMap.find(key); }
	bool contains(const Key& key) const { return mMap.find(key) != mMap.end(); }
	// 尝试修改列表中的值,如果key存在则执行成功,key不存在则执行失败
	bool trySet(const Key& key, const Value& value)
	{
		auto iter = mMap.find(key);
		const bool result = iter != mMap.end();
		if (result)
		{
			iter->second = value;
		}
		return result;
	}
	// 尝试向列表中插入值或者更新对应键的值,返回值表示是否插入成功
	bool insertOrUpdate(const Key& key, const Value& value)
	{
		// linux不支持try_emplace,所以为了统一,还是先查询再emplace
		auto iter = mMap.find(key);
		const bool insertSuccess = iter == mMap.end();
		if (insertSuccess)
		{
			mMap.emplace(key, value);
		}
		else
		{
			iter->second = value;
		}
		return insertSuccess;
	}
	// 尝试向列表中插入值或者更新对应键的值,返回值表示是否插入成功
	bool insertOrUpdate(const Key& key, Value&& value)
	{
		// linux不支持try_emplace,所以为了统一,还是先查询再emplace
		auto iter = mMap.find(key);
		const bool insertSuccess = iter == mMap.end();
		if (insertSuccess)
		{
			mMap.emplace(key, move(value));
		}
		else
		{
			iter->second = move(value);
		}
		return insertSuccess;
	}
	// 向列表中插入键,插入成功则返回插入值的引用,如果key已经存在,则返回对应的value的引用
	Value& insertOrGet(const Key& key)
	{
		auto iter = mMap.find(key);
		if (iter == mMap.end())
		{
			return mMap.emplace(key, mEmptyValue).first->second;
		}
		return iter->second;
	}
	// 向列表中插入键,插入成功则返回插入值的引用,如果key已经存在,则返回对应的value的引用
	Value& insertOrGet(const Key& key, const Value& value)
	{
		// linux不支持try_emplace,所以为了统一,还是先查询再emplace
		auto iter = mMap.find(key);
		if (iter == mMap.end())
		{
			return mMap.emplace(key, value).first->second;
		}
		return iter->second;
	}
	// 向列表中插入键,插入成功则返回插入值的引用,如果key已经存在,则返回对应的value的引用
	Value& insertOrGet(const Key& key, Value&& value)
	{
		// linux不支持try_emplace,所以为了统一,还是先查询再emplace
		auto iter = mMap.find(key);
		if (iter == mMap.end())
		{
			return mMap.emplace(key, move(value)).first->second;
		}
		return iter->second;
	}
	// 插入key和value,如果key存在则插入失败
	bool insert(const Key& key, const Value& value)
	{
		return mMap.emplace(key, value).second;
	}
	// 插入key和value,如果key存在则插入失败
	bool insert(const Key& key, Value&& value)
	{
		return mMap.emplace(key, move(value)).second;
	}
	// 插入key,value为默认值,如果key存在则插入失败
	bool insert(const Key& key)
	{
		return mMap.emplace(key, mEmptyValue).second;
	}
	iterator erase(const iterator& iter)
	{
		return mMap.erase(iter);
	}
	// 将key从列表中移除,如果移除成功,则将value设置为被移除的元素的second,一般用于指针类型
	bool erase(const Key& key, Value& value)
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			value = iter->second;
			mMap.erase(iter);
			return true;
		}
		return false;
	}
	// 返回值表示移除成功或失败
	bool erase(const Key& key)
	{
		auto iter = mMap.find(key);
		if (iter != mMap.end())
		{
			mMap.erase(iter);
			return true;
		}
		return false;
	}
	// 因为clear本身会执行元素的析构,即使列表为空,也会执行较多的指令,所以先排除空列表的不必要的清空操作
	void clear(bool disposeMemory = false)
	{
		if (mMap.size() == 0)
		{
			return;
		}
		mMap.clear();
		if (disposeMemory)
		{
			dispose();
		}
	}
	void dispose()
	{
		unordered_map<Key, Value> temp;
		mMap.swap(temp);
	}
	bool merge(const HashMap<Key, Value>& other)
	{
		bool success = true;
		for (const auto& iter : other)
		{
			success = mMap.emplace(iter.first, iter.second).second && success;
		}
		return success;
	}
	int size() const { return (int)mMap.size(); }
	Value& operator[](const Key& k) { return mMap[k]; }
	// 添加克隆函数的目的是为了显式调用拷贝,而非自动调用拷贝,可以避免可以使用移动构造而没有使用的情况
	void clone(HashMap<Key, Value>& target) const
	{
		target.mMap = mMap;
	}
public:
	unordered_map<Key, Value> mMap;
	static const HashMap<Key, Value> mDefaultHashMap;
private:
	static Value mEmptyValue;
};

template<typename Key, typename Value>
Value HashMap<Key, Value>::mEmptyValue;

template<typename Key, typename Value>
const HashMap<Key, Value> HashMap<Key, Value>::mDefaultHashMap;