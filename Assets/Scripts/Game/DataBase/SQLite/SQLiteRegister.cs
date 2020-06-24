using System;
using System.Collections;
using System.Collections.Generic;

public class SQLiteRegister : GameBase
{
	public static void registeAllTable()
	{
		registeTable(ref mSQLiteSound, "Sound");
		registeTable(ref mSQLiteDemo, "Monster");
		// 设置表格之间字段的索引关系
		mSQLite.linkAllTable();
	}
	//-------------------------------------------------------------------------------------------------------------
	protected static void registeTable<T>(ref T table, string tableName) where T : SQLiteTable, new()
	{
		table = mSQLite.registeTable<T>(tableName);
	}
}