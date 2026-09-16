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
		mTableList.forValue(item => item.setResourceAvailable(true));
	}
	// 异步加载所有SQLite表格。每张表会先尝试直接打开本地版本缓存;
	// 只有缓存未命中时才会请求SQLite AssetBundle中的TextAsset。
	public void loadAllAsync(Action callback)
	{
		if (mTableNameList.Count == 0)
		{
			callback?.Invoke();
			return;
		}
		DateTime startTime = DateTime.Now;
		int tableCount = mTableList.Count;
		int finishCount = 0;
		foreach (var item in mTableList)
		{
			SQLiteTable table = item.Value;
			table.loadAsync(() =>
			{
				if (++finishCount == tableCount)
				{
					// 所有当前版本文件名都已经确定以后再清理旧版本缓存。
					// 原实现放在加载前清理,会因为mDecryptFileName尚未生成而把全部可复用缓存删除。
					deleteUselessTempFile();
					log("打开所有SQLite表格耗时:" + (int)(DateTime.Now - startTime).TotalMilliseconds + "毫秒");
					callback?.Invoke();
				}
			});
		}
	}
	public void checkAll()
	{
		mTableList.forValue(item => item.checkAllData());
	}
	// 注册一个SQLite表格,将其添加到查询字典中
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
		mTableList.forValue(item => item.destroy());
		SqliteConnection.ClearAllPools();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
	public Dictionary<Type, SQLiteTable> getTableList() { return mTableList; }
	public SQLiteTable getTableByType(Type type) { return mTableList.get(type); }
	public SQLiteTable getTableByDataType(Type type) { return mTableDataTypeList.get(type); }
	public SQLiteTable getTable(string tableName) { return mTableNameList.get(tableName); }
	//------------------------------------------------------------------------------------------------------------------------------
	// 删除旧资源版本遗留的SQLite明文缓存文件
	protected void deleteUselessTempFile()
	{
		foreach (string file in findFilesNonAlloc(SQLiteTable.getDecryptFilePath()))
		{
			string fileName = getFileNameWithSuffix(file);
			if (!mTableList.containsValue(item => item.getDecryptFileName() == fileName))
			{
				deleteFile(file);
			}
		}
	}
}
#else
// SQLite数据库管理器,在不支持SQLite的平台为空实现
public class SQLiteManager : FrameSystem
{ }
#endif