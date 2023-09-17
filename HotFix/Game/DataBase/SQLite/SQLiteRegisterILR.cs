using System;
using static FrameBase;

public class SQLiteRegisterILR
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