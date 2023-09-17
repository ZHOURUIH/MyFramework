#if !NO_SQLITE
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;
using static CSharpUtility;
using static FrameBase;
using static FileUtility;
using static FrameUtility;

// SQLite数据表格管理器
public class SQLiteManager : FrameSystem
{
	protected Dictionary<string, SQLiteTable> mTableNameList;		// 根据表格名查找表格
	protected Dictionary<Type, SQLiteTable> mTableDataTypeList;		// 根据数据类型查找表格
	protected Dictionary<Type, SQLiteTable> mTableList;				// 根据表格类型查找表格
	protected bool mAutoLoad;										// 是否在资源可用时自动加载所有资源
	public SQLiteManager()
	{
		mTableNameList = new Dictionary<string, SQLiteTable>();
		mTableDataTypeList = new Dictionary<Type, SQLiteTable>();
		mTableList = new Dictionary<Type, SQLiteTable>();
		mAutoLoad = true;
	}
	public override void resourceAvailable()
	{
		if(!mAutoLoad)
		{
			return;
		}

		// 资源更新完毕后需要将所有已经加载的表格重新加载一次
		loadAll();
	}
	public void loadAll()
	{
		// 加载前先删除之前创建的所有临时文件
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		deleteFolder(availableWritePath("temp/"));
#endif
		foreach (var item in mTableList)
		{
			item.Value.load();
		}
	}
	public void setAutoLoad(bool autoLoad) { mAutoLoad = autoLoad; }
	public SQLiteTable registeTable(Type type, Type dataType, string tableName)
	{
		var table = createInstance<SQLiteTable>(type);
		table.setTableName(tableName);
		table.setDataType(dataType);
		mTableList.Add(Typeof(table), table);
		mTableNameList.Add(tableName, table);
		mTableDataTypeList.Add(dataType, table);
		// 如果在注册时资源已经可用了,则可以直接加载表格
		if (mGameFramework == null || mGameFramework.isResourceAvailable())
		{
			table.load();
		}
		return table;
	}
	public override void destroy()
	{
		base.destroy();
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		deleteFolder(availableWritePath("temp/"));
#endif
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