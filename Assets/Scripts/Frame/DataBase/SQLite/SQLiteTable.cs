#if !UNITY_IOS && !NO_SQLITE
using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;

public class SQLiteTable : GameBase
{
	protected string mTableName;
	protected Type mDataClassType;
	protected Dictionary<string, List<SQLiteTable>> mLinkTable; // 字段索引的表格
	protected Dictionary<int, SQLiteData> mDataList;
	public SQLiteTable()
	{
		mLinkTable = new Dictionary<string, List<SQLiteTable>>();
		mDataList = new Dictionary<int, SQLiteData>();
	}
	public void setDataType(Type dataClassType) { mDataClassType = dataClassType; }
	public void setTableName(string name) { mTableName = name; }
	public string getTableName() { return mTableName; }
	public virtual void linkTable() { }
	public List<SQLiteTable> getLinkTables(string paramName)
	{
		mLinkTable.TryGetValue(paramName, out List<SQLiteTable> tableList);
		return tableList;
	}
	public SQLiteTable getLinkTable(string paramName)
	{
		if (mLinkTable.TryGetValue(paramName, out List<SQLiteTable> tableList))
		{
			return tableList[0];
		}
		return null;
	}
	public void link(string paramName, SQLiteTable table)
	{
		if (!mLinkTable.TryGetValue(paramName, out List<SQLiteTable> tableList))
		{
			tableList = new List<SQLiteTable>();
			mLinkTable.Add(paramName, tableList);
		}
		tableList.Add(table);
	}
	public SqliteDataReader doQuery()
	{
		string queryStr = "SELECT * FROM " + mTableName;
		return mSQLite.queryReader(queryStr);
	}
	public SqliteDataReader doQuery(string condition)
	{
		string queryStr = "SELECT * FROM " + mTableName + " WHERE " + condition;
		return mSQLite.queryReader(queryStr);
	}
	public void doUpdate(string updateString, string conditionString)
	{
		string queryStr = "UPDATE " + mTableName + " SET " + updateString + " WHERE " + conditionString;
		mSQLite.queryNonReader(queryStr);
	}
	public void doInsert(string valueString)
	{
		string queryString = "INSERT INTO " + mTableName + " VALUES (" + valueString + ")";
		mSQLite.queryNonReader(queryString);
	}
	public SqliteDataReader queryReader(string queryString)
	{
		return mSQLite.queryReader(queryString);
	}
	public SQLiteData query(int id, out SQLiteData data)
	{
		if (mDataList.TryGetValue(id, out data))
		{
			return data;
		}
		string condition = EMPTY;
		appendConditionInt(ref condition, SQLiteData.ID, id, EMPTY);
		parseReader(doQuery(condition), out data);
		mDataList.Add(id, data);
		return data;
	}
	public SQLiteData query(Type type, int id, out SQLiteData data)
	{
		if (!mDataList.TryGetValue(id, out data))
		{
			string condition = EMPTY;
			appendConditionInt(ref condition, SQLiteData.ID, id, EMPTY);
			parseReader(doQuery(condition), out data);
			mDataList.Add(id, data);
		}
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			data = null;
		}
		return data;
	}
	public SQLiteData query(Type type, int id)
	{
		SQLiteData data;
		query(type, id, out data);
		return data;
	}
	public void queryAll<T>(Type type, List<T> dataList) where T : SQLiteData
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		parseReader(type, doQuery(), dataList);
		int count = dataList.Count;
		for(int i = 0; i < count; ++i)
		{
			T item = dataList[i];
			if (!mDataList.ContainsKey(item.mID))
			{
				mDataList.Add(item.mID, item);
			}
		}
	}
	public void insert<T>(T data) where T : SQLiteData
	{
		if(mDataList.ContainsKey(data.mID))
		{
			return;
		}
		if (Typeof(data) != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + Typeof(data));
			return;
		}
		string valueString = EMPTY;
		data.insert(ref valueString);
		removeLastComma(ref valueString);
		doInsert(valueString);
		mDataList.Add(data.mID, data);
	}
	//---------------------------------------------------------------------------------------------------------------------------
	protected void parseReader(Type type, SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if (reader != null && reader.Read())
		{
			data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (reader != null && reader.Read())
		{
			data = createInstance<SQLiteData>(mDataClassType);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(Type type, SqliteDataReader reader, IList dataList)
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if (reader != null)
		{
			while (reader.Read())
			{
				SQLiteData data = createInstance<SQLiteData>(type);
				data.mTable = this;
				data.parse(reader);
				dataList.Add(data);
			}
			reader.Close();
		}
	}
}
#endif