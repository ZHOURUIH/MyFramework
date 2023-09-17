#if !NO_SQLITE
using UnityEngine;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;
using static UnityUtility;
using static StringUtility;

// SQLite数据基类
public class SQLiteData : ClassObject
{
	public Dictionary<int, string> mValues;		// 存储原始的字符串,Key是下标
	public SQLiteTable mTable;					// 数据所属的表格
	public static string ID = "ID";				// 固定的ID列名
	public int mID;								// 数据固定的唯一ID字段
	public SQLiteData()
	{
		mValues = new Dictionary<int, string>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mValues.Clear();
		mTable = null;
		mID = 0;
	}
	public virtual void parse(SqliteDataReader reader)
	{
		parseParam(reader, ref mID, 0);
	}
	public virtual void insert(ref string valueString)
	{
		appendValueInt(ref valueString, mID);
	}
	public virtual void insert(MyStringBuilder valueString)
	{
		appendValueInt(valueString, mID);
	}
	public string getValue(int index) 
	{
		if(!mValues.TryGetValue(index, out string value))
		{
			logError("找不到字段下标:" + index);
		}
		return value;
	}
	public virtual bool checkData() { return true; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected int getParamInt(SqliteDataReader reader, int index)
	{
		int value = 0;
		try
		{
			object data = reader[index];
			if(data.GetType() != typeof(long))
			{
				logError("该列数据不是int类型,是" + data.GetType() + ",无法获取, colIndex:" + index + ", table:" + mTable.getTableName());
			}
			value = (int)(long)data;
		}
		catch (Exception e)
		{
			logException(e, "colIndex:" + index);
		}
		return value;
	}
	protected long getParamLong(SqliteDataReader reader, int index)
	{
		long value = 0;
		try
		{
			object data = reader[index];
			if (data.GetType() != typeof(long))
			{
				logError("该列数据不是int类型,是" + data.GetType() + ",无法获取, colIndex:" + index + ", table:" + mTable.getTableName());
			}
			value = (long)data;
		}
		catch (Exception e)
		{
			logException(e, "colIndex:" + index);
		}
		return value;
	}
	protected float getParamFloat(SqliteDataReader reader, int index)
	{
		float value = 0;
		try
		{
			object data = reader[index];
			if (data.GetType() != typeof(float))
			{
				logError("该列数据不是float类型,是" + data.GetType() + ",无法获取, colIndex:" + index + ", table:" + mTable.getTableName());
			}
			value = (float)data;
		}
		catch (Exception e)
		{
			logException(e, "colIndex:" + index);
		}
		return value;
	}
	protected string getParamString(SqliteDataReader reader, int index)
	{
		string str = null;
		try
		{
			object data = reader[index];
			if (data.GetType() != typeof(string))
			{
				logError("该列数据不是string类型,是" + data.GetType() + ",无法获取, colIndex:" + index + ", table:" + mTable.getTableName());
			}
			str = (string)data;
		}
		catch (Exception e)
		{
			logException(e, "colIndex:" + index);
		}
		return str;
	}
	protected void parseParam(SqliteDataReader reader, ref List<bool> value, int index)
	{
		string str = getParamString(reader, index);
		stringToBools(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<float> value, int index)
	{
		string str = getParamString(reader, index);
		stringToFloats(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<int> value, int index)
	{
		string str = getParamString(reader, index);
		stringToInts(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<ushort> value, int index)
	{
		string str = getParamString(reader, index);
		stringToUShorts(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<uint> value, int index)
	{
		string str = getParamString(reader, index);
		stringToUInts(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<byte> value, int index)
	{
		string str = getParamString(reader, index);
		stringToBytes(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<sbyte> value, int index)
	{
		string str = getParamString(reader, index);
		stringToSBytes(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<string> value, int index)
	{
		string str = getParamString(reader, index);
		stringToStrings(str, value);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref string value, int index)
	{
		value = getParamString(reader, index);
		mValues.Add(index, value);
	}
	protected void parseParam(SqliteDataReader reader, ref float value, int index)
	{
		value = getParamFloat(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref long value, int index)
	{
		value = getParamLong(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref uint value, int index)
	{
		value = (uint)getParamInt(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref int value, int index)
	{
		value = getParamInt(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref ushort value, int index)
	{
		value = (ushort)getParamInt(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref short value, int index)
	{
		value = (short)getParamInt(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref byte value, int index)
	{
		value = (byte)getParamInt(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref sbyte value, int index)
	{
		value = (sbyte)getParamInt(reader, index);
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref bool value, int index)
	{
		value = getParamInt(reader, index) != 0;
		mValues.Add(index, EMPTY);
	}
	protected void parseParam(SqliteDataReader reader, ref Vector2 value, int index)
	{
		string str = getParamString(reader, index);
		value = SToVector2(str);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref Vector2Int value, int index)
	{
		string str = getParamString(reader, index);
		value = stringToVector2Int(str);
		mValues.Add(index, str);
	}
	protected void parseParam(SqliteDataReader reader, ref Vector3 value, int index)
	{
		string str = getParamString(reader, index);
		value = SToVector3(str);
		mValues.Add(index, str);
	}
}

#endif