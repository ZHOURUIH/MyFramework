#if USE_SQLITE
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;
using static CSharpUtility;
using static FileUtility;
using static StringUtility;
using static FrameBaseUtility;

// SQLite数据表格管理器
public class SQLiteManager : FrameSystem
{
	protected Dictionary<string, SQLiteTable> mTableNameList = new();		// 根据表格名查找表格
	protected Dictionary<Type, SQLiteTable> mTableDataTypeList = new();		// 根据数据类型查找表格
	protected Dictionary<Type, SQLiteTable> mTableList = new();				// 根据表格类型查找表格
	protected bool mAutoLoad = true;										// 是否在资源可用时自动加载所有资源
	public override void resourceAvailable()
	{
		if (!mAutoLoad)
		{
			return;
		}
		// 如果还没有注册表格,不在执行,没有任何表格加载,而且还会删除已缓存的表格
		if (mTableNameList.Count == 0)
		{
			return;
		}

		// 资源更新完毕后需要将所有已经加载的表格重新加载一次
		loadAll();
		// 加载之前也清理一次,因为可能在退出时由于文件被占用而清理不掉,所以全部加载后也清一次
		deleteUselessTempFile();
	}
	public void loadAll()
	{
		foreach (SQLiteTable item in mTableList.Values)
		{
			item.load();
		}
	}
	public void setAutoLoad(bool autoLoad) { mAutoLoad = autoLoad; }
	public void checkAll()
	{
		foreach (SQLiteTable item in mTableList.Values)
		{
			item.checkAllData();
		}
	}
	public SQLiteTable registeTable(Type type, Type dataType, string tableName)
	{
		var table = createInstance<SQLiteTable>(type);
		table.setTableName(tableName);
		table.setDataType(dataType);
		mTableList.Add(table.GetType(), table);
		mTableNameList.Add(tableName, table);
		mTableDataTypeList.Add(dataType, table);
		return table;
	}
	public override void destroy()
	{
		base.destroy();
		// 退出游戏的时候清理一次无用文件
		if (!isEditor())
		{
			deleteUselessTempFile();
		}
		foreach (SQLiteTable item in mTableList.Values)
		{
			item.destroy();
		}
		SqliteConnection.ClearAllPools();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
	public Dictionary<Type, SQLiteTable> getTableList() { return mTableList; }
	public SQLiteTable getTableByType(Type type) { return mTableList.get(type); }
	public SQLiteTable getTableByDataType(Type type) { return mTableDataTypeList.get(type); }
	public SQLiteTable getTable(string tableName) { return mTableNameList.get(tableName); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void deleteUselessTempFile()
	{
		foreach (string file in findFilesNonAlloc(SQLiteTable.getDecryptFilePath()))
		{
			bool needDelete = true;
			string fileName = getFileNameWithSuffix(file);
			foreach (SQLiteTable item in mTableList.Values)
			{
				if (item.getDecryptFileName() == fileName)
				{
					needDelete = false;
					break;
				}
			}
			if (needDelete)
			{
				deleteFile(file);
			}
		}
	}
}
#else
public class SQLiteManager : FrameSystem
{ }
#endif