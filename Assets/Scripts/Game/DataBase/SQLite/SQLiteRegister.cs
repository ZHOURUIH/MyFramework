using System;

public class SQLiteRegisterMain : FrameBase
{
	public static void registeAllTable()
	{
		;
	}
	//-------------------------------------------------------------------------------------------------------------
	protected static void registeTable<Table, Data>(ref Table table, string tableName) where Table : SQLiteTable where Data : SQLiteData
	{
		table = mSQLiteManager.registeTable(typeof(Table), typeof(Data), tableName) as Table;
	}
}