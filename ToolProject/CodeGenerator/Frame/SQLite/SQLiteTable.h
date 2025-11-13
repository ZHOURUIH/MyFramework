#pragma once

#include "SQLiteTableBase.h"
#include "SelectCount.h"
#include "StringUtility.h"
#include "MathUtility.h"
#include "BinaryUtility.h"
#include "IsSubClassOf.h"

template<class T, typename TypeCheck = typename IsSubClassOf<SQLiteData, T>::mType>
class SQLiteTable : public SQLiteTableBase
{
public:
	~SQLiteTable() override
	{
		for (const auto& iter : mDataList)
		{
			delete iter.second;
		}
		mDataList.clear();
	}
	T* query(const int id, const bool showError = true)
	{
		if (!mDataList.contains(id))
		{
			T* item = new T();
			// 如果找不到则销毁,并且提示错误信息
			if (!queryData(id, *item))
			{
				if (showError)
				{
					ERROR("can not find item id:" + StringUtility::intToString(id) + " in " + mTableName);
				}
				delete item;
			}
			// 查找成功则加入缓存列表
			else
			{
				mDataList.insert(id, item);
			}
		}
		return mDataList.get(id, nullptr);
	}
	const myMap<int, T*>& queryAll()
	{
		// 如果没有查询过,或者数据的数量与已查询数量不一致才会查询全部
		if (mDataList.size() == 0 || doQueryCount() != mDataList.size())
		{
			myVector<T*> list;
			queryAllData(list);
			for(T*& value : list)
			{
				// 没有查询过的数据才插入列表,已经查询过的则不再需要,直接释放
				if (!mDataList.insert(value->mID, value))
				{
					delete value;
				}
			}
		}
		return mDataList;
	}
	void checkData(const int checkID, const int dataID, const string& refTableName)
	{
		if (checkID > 0 && query(checkID, false) == nullptr)
		{
			ERROR("can not find item id:" + StringUtility::intToString(checkID) + " in " + mTableName + ", ref ID:" + StringUtility::intToString(dataID) + ", ref Table:" + refTableName);
		}
	}
	template<typename T0>
	void checkData(const myVector<T0>& checkIDList, const int dataID, const string& refTableName)
	{
		FOR_VECTOR(checkIDList)
		{
			if (query(checkIDList[i], false) == nullptr)
			{
				ERROR("can not find item id:" + StringUtility::intToString(checkIDList[i]) + " in " + mTableName + ", ref ID:" + StringUtility::intToString(dataID) + ", ref Table:" + refTableName);
			}
		}
	}
	template<typename T0, typename T1>
	void checkListPair(const myVector<T0>& list0, const myVector<T1>& list1, const int dataID)
	{
		if (list0.size() != list1.size())
		{
			ERROR(string("list pair size not match, table:") + mTableName + ", ref ID:" + intToString(dataID));
		}
	}
	void checkDataAllColName() override
	{
		T().checkAllColName(this);
	}
protected:
	void queryAllData(myVector<T*>& dataList)
	{
		doSelect(dataList);
	}
	bool queryData(const int id, T& data)
	{
		Array<128> conditionString{ 0 };
		StringUtility::sqlConditionUInt(conditionString, "ID", id);
		return doSelect(data, conditionString.str());
	}
	int doQueryCount()
	{
		Array<256> queryStr{ 0 };
		StringUtility::strcat_t(queryStr, "SELECT count(ID) FROM ", mTableName, " WHERE ID > 0");
		SelectCount selectCount;
		SQLiteDataReader(mSQLite3, queryStr.str()).parseReader(selectCount);
		return selectCount.mRowCount;
	}
	void doSelect(myVector<T*>& dataList, const char* conditionString = nullptr)
	{
		Array<256> queryStr{ 0 };
		if (conditionString != nullptr)
		{
			StringUtility::strcat_t(queryStr, "SELECT * FROM ", mTableName, " WHERE ", conditionString);
		}
		else
		{
			StringUtility::strcat_t(queryStr, "SELECT * FROM ", mTableName);
		}
		SQLiteDataReader(mSQLite3, queryStr.str()).parseReader(dataList);
	}
	bool doSelect(T& data, const char* conditionString = nullptr)
	{
		Array<256> queryStr{ 0 };
		if (conditionString != nullptr)
		{
			StringUtility::strcat_t(queryStr, "SELECT * FROM ", mTableName, " WHERE ", conditionString, " LIMIT 1");
		}
		else
		{
			StringUtility::strcat_t(queryStr, "SELECT * FROM ", mTableName, " LIMIT 1");
		}
		return SQLiteDataReader(mSQLite3, queryStr.str()).parseReader(data);
	}
	void queryAllToList(myVector<T*>& dataList)
	{
		auto& allList = queryAll();
		if (allList.size() == 0)
		{
			return;
		}
		int maxCount = 0;
		for (const auto& iter : allList)
		{
			maxCount = MathUtility::getMax(maxCount, iter.second->mID);
		}
		// 因为ID都是从1开始的,所以数量会比下标多1
		dataList.resize(++maxCount);
		BinaryUtility::memset(dataList.data(), 0, sizeof(T*)* maxCount);
		for (const auto& iter : allList)
		{
			dataList[iter.first] = iter.second;
		}
	}
protected:
	myMap<int, T*> mDataList;
};