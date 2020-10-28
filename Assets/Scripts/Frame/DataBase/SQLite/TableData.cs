#if !UNITY_IOS && !NO_SQLITE
using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;

public class SQLiteData : GameBase
{
	public Dictionary<string, string> mValues;
	public SQLiteTable mTable;
	public static string ID = "ID";
	public int mID;
	public SQLiteData()
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
	protected void parseParam(SqliteDataReader reader, ref List<sbyte> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToSByteArray(str, value);
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
	protected void parseParam(SqliteDataReader reader, ref sbyte value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (sbyte)stringToInt(str);
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

#endif