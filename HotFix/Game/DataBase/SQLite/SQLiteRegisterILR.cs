using System;
using System.Collections.Generic;

public class SQLiteRegisterILR : GB
{
	public static void registeAll()
	{
		;
	}
	//-------------------------------------------------------------------------------------------------------------
	protected static void registeTable<T>(out T sqliteTable, Type tableType, Type dataType, string tableName) where T : SQLiteTable
	{
		sqliteTable = mSQLiteManager.registeTable(tableType, dataType, tableName) as T;
	}
}