#if !UNITY_IOS && !NO_SQLITE
using UnityEngine;
using Mono.Data.Sqlite;
using System.Collections.Generic;

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
	public virtual void insert(MyStringBuilder valueString)
	{
		appendValueInt(valueString, mID);
	}
	public string getValue(string paramName) 
	{
		if(!mValues.TryGetValue(paramName, out string value))
		{
			logError("找不到字段名:" + paramName);
		}
		return value;
	}
	public virtual bool checkData() { return true; }
	//--------------------------------------------------------------------------------------------------------
	protected void parseParam(SqliteDataReader reader, ref List<bool> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToBools(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<float> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToFloats(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<int> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToInts(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<ushort> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToUShorts(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<uint> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToUInts(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<byte> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToBytes(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<sbyte> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToSBytes(str, value);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref List<string> value, string paramName)
	{
		string str = reader[paramName].ToString();
		stringToStrings(str, value);
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
		value = SToF(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref uint value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (uint)SToI(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref int value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = SToI(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref ushort value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (ushort)SToI(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref short value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (short)SToI(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref byte value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (byte)SToI(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref sbyte value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = (sbyte)SToI(str);
		mValues.Add(paramName, str);
	}
	protected void parseParam(SqliteDataReader reader, ref bool value, string paramName)
	{
		string str = reader[paramName].ToString();
		value = SToI(str) != 0;
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