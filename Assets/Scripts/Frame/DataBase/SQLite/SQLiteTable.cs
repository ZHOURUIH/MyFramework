#if !UNITY_IOS && !NO_SQLITE
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
		return mSQLite.queryReader("SELECT * FROM " + mTableName);
	}
	public SqliteDataReader doQuery(string condition)
	{
		MyStringBuilder builder = newBuilder();
		builder.Append("SELECT * FROM ", mTableName, " WHERE ", condition);
		return mSQLite.queryReader(valueDestroyBuilder(builder));
	}
	public void doUpdate(string updateString, string conditionString)
	{
		MyStringBuilder builder = newBuilder();
		builder.Append("UPDATE ", mTableName, " SET ", updateString, " WHERE ", conditionString);
		mSQLite.queryNonReader(valueDestroyBuilder(builder));
	}
	public void doInsert(string valueString)
	{
		MyStringBuilder builder = newBuilder();
		builder.Append("INSERT INTO ", mTableName, " VALUES (", valueString, ")");
		mSQLite.queryNonReader(valueDestroyBuilder(builder));
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
		MyStringBuilder condition = newBuilder();
		appendConditionInt(condition, SQLiteData.ID, id, EMPTY);
		parseReader(doQuery(valueDestroyBuilder(condition)), out data);
		mDataList.Add(id, data);
		return data;
	}
	public SQLiteData query(Type type, int id, out SQLiteData data)
	{
		if (!mDataList.TryGetValue(id, out data))
		{
			MyStringBuilder condition = newBuilder();
			appendConditionInt(condition, SQLiteData.ID, id, EMPTY);
			parseReader(doQuery(valueDestroyBuilder(condition)), out data);
			mDataList.Add(id, data);
		}
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			data = null;
		}
		return data;
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
		MyStringBuilder valueString = newBuilder();
		data.insert(valueString);
		removeLastComma(valueString);
		doInsert(valueDestroyBuilder(valueString));
		mDataList.Add(data.mID, data);
	}
	//---------------------------------------------------------------------------------------------------------------------------
	protected SQLiteData query(Type type, int id)
	{
		SQLiteData data;
		query(type, id, out data);
		return data;
	}
	protected void queryAll(Type type, IList dataList)
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		parseReader(type, doQuery(), dataList);
		int count = dataList.Count;
		for (int i = 0; i < count; ++i)
		{
			var item = (SQLiteData)dataList[i];
			if (!mDataList.ContainsKey(item.mID))
			{
				mDataList.Add(item.mID, item);
			}
		}
	}
	protected void parseReader(Type type, SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if(reader == null)
		{
			return;
		}
		if (reader.Read())
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
		if (reader == null)
		{
			return;
		}
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
#endif