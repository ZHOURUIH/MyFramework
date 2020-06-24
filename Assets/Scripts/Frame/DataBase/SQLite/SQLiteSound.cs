#if !UNITY_IOS && !NO_SQLITE
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

public class TDSound : TableData
{
	public static string COL_FILE_NAME = "FileName";
	public static string COL_DESC = "Desc";
	public static string COL_VOLUME_SCALE = "VolumeScale";
	public string mFileName;
	public string mDescribe;
	public float mVolumeScale;
	public override void parse(SqliteDataReader reader)
	{
		base.parse(reader);
		mFileName = reader[COL_FILE_NAME].ToString();
		mDescribe = reader[COL_DESC].ToString();
		mVolumeScale = stringToFloat(reader[COL_VOLUME_SCALE].ToString());
	}
	public override void insert(ref string valueString)
	{
		base.insert(ref valueString);
		appendValueString(ref valueString, mFileName);
		appendValueString(ref valueString, mDescribe);
		appendValueFloat(ref valueString, mVolumeScale);
	}
}

public class SQLiteSound : SQLiteTable
{
	public SQLiteSound()
		:base(typeof(TDSound)){}
};

#endif