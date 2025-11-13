#if USE_SQLITE
#if UNITY_WEBGL
#error "webgl not support sqlite! that will report error message:"
#error "dlopen: Unable to open DLL! Dynamic linking is not supported in WebAssembly builds due to limitations to performance and code size. "
#error "Please statically link in the needed libraries."
#endif
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;
using static FrameUtility;
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static FrameBaseUtility;
using static FrameBaseHotFix;

// SQLite数据表格管理器
public class SQLiteManager : FrameSystem
{
	protected Dictionary<string, SQLiteTable> mTableNameList = new();		// 根据表格名查找表格
	protected Dictionary<Type, SQLiteTable> mTableDataTypeList = new();		// 根据数据类型查找表格
	protected Dictionary<Type, SQLiteTable> mTableList = new();				// 根据表格类型查找表格
	public override void resourceAvailable()
	{
		foreach (SQLiteTable item in mTableList.Values)
		{
			item.setResourceAvailable(true);
		}
	}
	public void loadAllAsync(Action callback)
	{
		// 如果还没有注册表格,不在执行,没有任何表格加载,而且还会删除已缓存的表格
		if (mTableNameList.Count == 0)
		{
			callback?.Invoke();
			return;
		}
		// 加载之前也清理一次,因为可能在退出时由于文件被占用而清理不掉
		deleteUselessTempFile();
		DateTime startTime = DateTime.Now;
		// 提前加载资源包和其中的子资源
		mResourceManager.preloadAssetBundleAsync(FrameDefine.SQLITE, (AssetBundleInfo assetBundle) =>
		{
			assetBundle?.loadAllSubAssets();
			// 然后再加载每个表格
			int tableCount = mTableList.Count;
			int finishCount = 0;
			foreach (SQLiteTable item in mTableList.Values)
			{
				item.loadAsync(() =>
				{
					if (++finishCount == tableCount)
					{
						log("打开所有SQLite表格耗时:" + (int)(DateTime.Now - startTime).TotalMilliseconds + "毫秒");
						callback?.Invoke();
					}
				});
			}
		});
	}
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