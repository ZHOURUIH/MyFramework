using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

public class TDDemo : SQLiteData
{
	public static string DemoColName = "DemoColName";
	public string mDemoColName;
	public override void parse(SqliteDataReader reader)
	{
		base.parse(reader);
		parseParam(reader, ref mDemoColName, DemoColName);
	}
}

public class SQLiteDemo : SQLiteTable
{}