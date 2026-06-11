#ifndef _SQLITE_TABLE_H_
#define _SQLITE_TABLE_H_

#include "SQLiteTableBase.h"
#include "SelectCount.h"
#include "VectorPoolManager.h"

template<class T>
class SQLiteTable : public SQLiteTableBase
{
	BASE(SQLiteTableBase);
public:
	virtual ~SQLiteTable()
	{
		FOREACH(iter, mDataList)
		{
			DELETE(iter->second);
		}
		END(mDataList);
		mDataList.clear();
	}
	T* query(uint id, bool showError = true)
	{
		LOCK(mThreadLock);
		if (!mDataList.contains(id))
		{
			T* item = NEW(T, item);
			// 如果找不到则销毁,并且提示错误信息
			if (!queryData(id, *item))
			{
				if (showError)
				{
					ERROR("can not find item id:" + intToString(id) + " in " + mTableName);
				}
				DELETE(item);
			}
			// 查找成功则加入缓存列表
			else
			{
				mDataList.insert(id, item);
			}
		}
		UNLOCK(mThreadLock);
		return mDataList.get(id, nullptr);
	}
	const Map<uint, T*>& queryAll()
	{
		// 如果没有查询过,或者数据的数量与已查询数量不一致才会查询全部
		LOCK(mThreadLock);
		if (mDataList.size() == 0 || doQueryCount() != (int)mDataList.size())
		{
			VECTOR_THREAD(T*, list);
			queryAllData(list);
			FOR_CONST(list)
			{
				T*& value = list[i];
				// 没有查询过的数据才插入列表,已经查询过的则不再需要,直接释放
				if (!mDataList.insert(value->mID, value))
				{
					DELETE(value);
				}
			}
			END_CONST();
			UN_VECTOR_THREAD(T*, list);
		}
		UNLOCK(mThreadLock);
		return mDataList;
	}
	bool insert(const T& data)
	{
		string valueString;
		data.insert(valueString);
		return doInsert(valueString.c_str());
	}
	bool update(const T& data) 
	{
		string updateString;
		data.update(updateString);
		Array<128> conditionStr{ 0 };
		sqlConditionInt(conditionStr, SQLITE_ID_COL, data.mID);
		return doUpdate(updateString.c_str(), conditionStr.toString());
	}
protected:
	void queryAllData(Vector<T*>& dataList)
	{
		doSelect(dataList);
	}
	bool queryData(uint id, T& data)
	{
		Array<128> conditionString{ 0 };
		sqlConditionUInt(conditionString, SQLITE_ID_COL, id);
		return doSelect(data, conditionString.toString());
	}
	int doQueryCount()
	{
		Array<256> queryStr{ 0 };
		strcat_t(queryStr, "SELECT count(", SQLITE_ID_COL, ") FROM ", mTableName, " WHERE ", SQLITE_ID_COL, " > 0");
		SelectCount selectCount;
		parseReader(executeQuery(queryStr.toString()), selectCount);
		return selectCount.mRowCount;
	}
	void doSelect(Vector<T*>& dataList, const char* conditionString = nullptr)
	{
		Array<256> queryStr{ 0 };
		if (conditionString != nullptr)
		{
			strcat_t(queryStr, "SELECT * FROM ", mTableName, " WHERE ", conditionString);
		}
		else
		{
			strcat_t(queryStr, "SELECT * FROM ", mTableName);
		}
		parseReader(executeQuery(queryStr.toString()), dataList);
	}
	bool doSelect(T& data, const char* conditionString = nullptr)
	{
		Array<256> queryStr{ 0 };
		if (conditionString != nullptr)
		{
			strcat_t(queryStr, "SELECT * FROM ", mTableName, " WHERE ", conditionString);
		}
		else
		{
			strcat_t(queryStr, "SELECT * FROM ", mTableName);
		}
		return parseReader(executeQuery(queryStr.toString()), data);
	}
	void queryAllToList(Vector<T*>& dataList)
	{
		int maxCount = 0;
		auto& allList = queryAll();
		FOREACH_CONST(iter, allList)
		{
			maxCount = getMax(maxCount, iter->second->mID);
		}
		END_CONST();
		// 因为ID都是从1开始的,所以数量会比下标多1
		++maxCount;
		dataList.resize(maxCount);
		memset(dataList.data(), 0, sizeof(T*)* maxCount);
		FOREACH_CONST(iter, allList)
		{
			dataList[iter->first] = iter->second;
		}
		END_CONST();
	}
protected:
	Map<uint, T*> mDataList;
};

#endif