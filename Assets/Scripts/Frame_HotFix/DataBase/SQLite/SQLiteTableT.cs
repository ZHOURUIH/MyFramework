#if USE_SQLITE
using System;
using System.Collections.Generic;
using static UnityUtility;

// 表示一个SQLite表格
public class SQLiteTableT<T> : SQLiteTable where T : SQLiteData
{
	public T query(int id, bool errorIfNull = true)
	{
		return queryInternal(id, errorIfNull) as T;
	}
	public void insert(T data)
	{
		insertInternal(data);
	}
	public void queryDataList(List<T> dataList)
	{
		queryDataList(null, dataList);
	}
	public Dictionary<int, SQLiteData> queryAll()
	{
		using var a = new ListScope<T>(out var list);
		queryDataList(null, list);
		return mDataMap;
	}
	public void queryDataList(string condition, List<T> dataList)
	{
		parseReader(typeof(T), doQuery(condition), dataList);
		foreach (T data in dataList)
		{
			mDataMap.TryAdd(data.mID, data);
		}
	}
}
#endif