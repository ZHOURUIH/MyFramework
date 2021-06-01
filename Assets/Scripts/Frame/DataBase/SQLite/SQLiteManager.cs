#if !UNITY_IOS && !NO_SQLITE
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;

public class SQLiteManager : FrameSystem
{
	protected Dictionary<string, SQLiteTable> mTableNameList;		// 根据表格名查找表格
	protected Dictionary<Type, SQLiteTable> mTableDataTypeList;		// 根据数据类型查找表格
	protected Dictionary<Type, SQLiteTable> mTableList;				// 根据表格类型查找表格
	public SQLiteManager()
	{
		mTableNameList = new Dictionary<string, SQLiteTable>();
		mTableDataTypeList = new Dictionary<Type, SQLiteTable>();
		mTableList = new Dictionary<Type, SQLiteTable>();
	}
	public override void resourceAvailable()
	{
		base.resourceAvailable();
		// 资源更新完毕后需要将所有已经加载的表格重新加载一次
		foreach(var item in mTableList)
		{
			item.Value.init(GameDefine.SQLITE_ENCRYPT_KEY);
		}
	}
	public SQLiteTable registeTable(Type type, Type dataType, string tableName)
	{
		var table = createInstance<SQLiteTable>(type);
		table.setTableName(tableName);
		table.setDataType(dataType);
		table.init(GameDefine.SQLITE_ENCRYPT_KEY);
		mTableList.Add(Typeof(table), table);
		mTableNameList.Add(tableName, table);
		mTableDataTypeList.Add(dataType, table);
		return table;
	}
	public override void destroy()
	{
		base.destroy();
		foreach (var item in mTableList)
		{
			item.Value.destroy();
		}
		SqliteConnection.ClearAllPools();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
	public Dictionary<Type, SQLiteTable> getTableList() { return mTableList; }
	public SQLiteTable getTableByType(Type type)
	{
		mTableList.TryGetValue(type, out SQLiteTable table);
		return table;
	}
	public SQLiteTable getTableByDataType(Type type)
	{
		mTableDataTypeList.TryGetValue(type, out SQLiteTable table);
		return table;
	}
	public SQLiteTable getTable(string tableName)
	{
		mTableNameList.TryGetValue(tableName, out SQLiteTable table);
		return table;
	}
}
#endif