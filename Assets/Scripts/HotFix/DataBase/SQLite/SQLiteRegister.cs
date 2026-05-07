// auto generate start
using System;
using static GBR;
using static FrameBaseHotFix;

public class SQLiteRegister
{
	public static void registeAll()
	{

		// 进入热更以后,所有资源都处于可用状态
		mSQLiteManager.resourceAvailable();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void registeTable<T>(out T table, Type dataType, string tableName) where T : SQLiteTable
	{
		table = mSQLiteManager.registeTable(typeof(T), dataType, tableName) as T;
	}
}
// auto generate end