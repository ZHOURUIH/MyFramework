#if !UNITY_IOS && !NO_SQLITE
using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections.Generic;
using System;

public class TableData : GameBase
{
	public Dictionary<string, string> mValues;
	public static string ID = "ID";
	public int mID;
	public SQLiteTable mTable;
	public TableData()
	{
		mValues = new Dictionary<string, string>();
	}
	public virtual void parse(SqliteDataReader reader)
	{
		parseParam(reader, ref mID, ID);
	}
	public virtual void insert(ref string valueString)
	{
		appendValueInt(ref valueString, mID);
	}
	public string getValue(string paramName) { return mValues[paramName]; }
	//--------------------------------------------------------------------------------------------------------
	protected void parseParam(SqliteDataReader reader, ref List<float> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToFloatArray(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<int> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToIntArray(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<ushort> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToUShortArray(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<uint> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToUIntArray(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<byte> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToByteArray(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<string> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToStringArray(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref string value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = str;
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref float value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = stringToFloat(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref uint value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (uint)stringToInt(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref int value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = stringToInt(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref ushort value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (ushort)stringToInt(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref short value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (short)stringToInt(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref byte value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (byte)stringToInt(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref bool value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = stringToInt(str) != 0;
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref Vector2 value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = stringToVector2(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref Vector2Int value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = stringToVector2Int(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref Vector3 value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = stringToVector3(str);
		mValues.Add(paramName, str);
	}
}

public class SQLiteTable : GameBase
{
	protected string mTableName;
	protected Type mTableClassType;
	protected SortedDictionary<string, List<SQLiteTable>> mLinkTable; // 字段索引的表格
	protected SortedDictionary<int, TableData> mDataList;
	public SQLiteTable(Type tableClassType)
	{
		mTableClassType = tableClassType;
		mLinkTable = new SortedDictionary<string, List<SQLiteTable>>();
		mDataList = new SortedDictionary<int, TableData>();
	}
	public void setTableName(string name) { mTableName = name; }
	public string getTableName() { return mTableName; }
	public virtual void linkTable() { }
	public List<SQLiteTable> getLinkTables(string paramName)
	{
		return mLinkTable.ContainsKey(paramName) ? mLinkTable[paramName] : null;
	}
	public SQLiteTable getLinkTable(string paramName)
	{
		return mLinkTable.ContainsKey(paramName) ? mLinkTable[paramName][0] : null;
	}
	public void link(string paramName, SQLiteTable table)
	{
		if (!mLinkTable.ContainsKey(paramName))
		{
			mLinkTable.Add(paramName, new List<SQLiteTable>());
		}
		mLinkTable[paramName].Add(table);
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
	public TableData query(int id, out TableData data)
	{
		if (mDataList.ContainsKey(id))
		{
			data = mDataList[id];
			return data;
		}
		string condition = "";
		appendConditionInt(ref condition, TableData.ID, id, "");
		parseReader(doQuery(condition), out data);
		mDataList.Add(id, data);
		return data;
	}
	public T query<T>(int id, out T data) where T : TableData, new()
	{
		if (typeof(T) != mTableClassType)
		{
			logError("sqlite table type error, this type:" + mTableClassType.ToString() + ", param type:" + typeof(T).ToString());
			data = null;
			return data;
		}
		if (mDataList.ContainsKey(id))
		{
			data = mDataList[id] as T;
			return data;
		}
		string condition = "";
		appendConditionInt(ref condition, TableData.ID, id, "");
		parseReader(doQuery(condition), out data);
		mDataList.Add(id, data);
		return data;
	}
	public T query<T>(int id) where T : TableData, new()
	{
		T data;
		query(id, out data);
		return data;
	}
	public void queryAll<T>(out List<T> dataList) where T : TableData, new()
	{
		dataList = null;
		if (typeof(T) != mTableClassType)
		{
			logError("sqlite table type error, this type:" + mTableClassType.ToString() + ", param type:" + typeof(T).ToString());
			return;
		}
		parseReader(doQuery(), out dataList);
		foreach(var item in dataList)
		{
			if(!mDataList.ContainsKey(item.mID))
			{
				mDataList.Add(item.mID, item);
			}
		}
	}
	public void insert<T>(T data) where T : TableData
	{
		if(mDataList.ContainsKey(data.mID))
		{
			return;
		}
		if (typeof(T) != mTableClassType)
		{
			logError("sqlite table type error, this type:" + mTableClassType.ToString() + ", param type:" + typeof(T).ToString());
			return;
		}
		string valueString = EMPTY_STRING;
		data.insert(ref valueString);
		removeLastComma(ref valueString);
		doInsert(valueString);
		mDataList.Add(data.mID, data);
	}
	//---------------------------------------------------------------------------------------------------------------------------
	protected void parseReader<T>(SqliteDataReader reader, out T data) where T : TableData, new()
	{
		data = null;
		if (typeof(T) != mTableClassType)
		{
			logError("sqlite table type error, this type:" + mTableClassType.ToString() + ", param type:" + typeof(T).ToString());
			return;
		}
		if (reader != null && reader.Read())
		{
			data = new T();
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(SqliteDataReader reader, out TableData data)
	{
		data = null;
		if (reader != null && reader.Read())
		{
			data = createInstance<TableData>(mTableClassType);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader<T>(SqliteDataReader reader, out List<T> dataList) where T : TableData, new()
	{
		dataList = new List<T>();
		if (typeof(T) != mTableClassType)
		{
			logError("sqlite table type error, this type:" + mTableClassType.ToString() + ", param type:" + typeof(T).ToString());
			return;
		}
		if (reader != null)
		{
			while (reader.Read())
			{
				T data = new T();
				data.mTable = this;
				data.parse(reader);
				dataList.Add(data);
			}
			reader.Close();
		}
	}
}
#endif