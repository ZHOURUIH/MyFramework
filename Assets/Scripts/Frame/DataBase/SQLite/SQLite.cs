#if !UNITY_IOS && !NO_SQLITE
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System;

public class SQLite : FrameSystem
{
	protected Dictionary<Type, SQLiteTable> mTableList;
	protected SqliteConnection mConnection;
	protected SqliteCommand mCommand;
	public SQLite()
	{
		mTableList = new Dictionary<Type, SQLiteTable>();
		try
		{
			string fullPath = FrameDefine.F_ASSETS_DATA_BASE_PATH + GameDefine.DATA_BASE_FILE;
			if (isFileExist(fullPath))
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				// 将文件拷贝到persistentDataPath目录中,因为只有该目录才拥有读写权限
				string persisFullPath = FrameDefine.F_PERSIS_DATA_BASE_PATH + GameDefine.DATA_BASE_FILE;
				logInfo("persisFullPath:" + persisFullPath, LOG_LEVEL.FORCE);
				copyFile(fullPath, persisFullPath);
				fullPath = persisFullPath;
#endif
				mConnection = new SqliteConnection("URI=file:" + fullPath);   // 创建SQLite对象的同时，创建SqliteConnection对象  
				mConnection.Open();                         // 打开数据库链接
				if (mConnection != null && mCommand == null)
				{
					mCommand = mConnection.CreateCommand();
				}
			}
		}
		catch (Exception e)
		{
			log("打开数据库失败:" + e.Message, LOG_LEVEL.FORCE);
		}
	}
	public SQLiteTable registeTable(Type type, Type dataType, string tableName)
	{
		var table = createInstance<SQLiteTable>(type);
		table.setTableName(tableName);
		table.setDataType(dataType);
		mTableList.Add(Typeof(table), table);
		return table;
	}
	public void linkAllTable()
	{
		foreach(var item in mTableList)
		{
			item.Value.linkTable();
		}
	}
	public void connect(string fullFileName)
	{
		clearCommand();
		clearConnection();
		if (isFileExist(fullFileName))
		{
			// 创建SQLite对象的同时，创建SqliteConnection对象  
			mConnection = new SqliteConnection("URI=file:" + fullFileName);
			// 打开数据库链接
			mConnection.Open();
		}
		if (mConnection != null)
		{
			mCommand = mConnection.CreateCommand();
		}
	}
	public override void destroy()
	{
		base.destroy();
		clearCommand();
		clearConnection();
		SqliteConnection.ClearAllPools();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
	// 数据库文件不存在时,创建数据库文件
	public void createDataBase()
	{
		if (mConnection == null)
		{
			connect(FrameDefine.F_PERSIS_DATA_BASE_PATH + GameDefine.DATA_BASE_FILE);
		}
	}
	public void createTable(string tableName, string format)
	{
		queryNonReader("CREATE TABLE IF NOT EXISTS " + tableName + "(" + format + ");");
	}
	public SqliteDataReader queryReader(string queryString)
	{
		if (mCommand == null)
		{
			return null;
		}
		mCommand.CommandText = queryString;
		SqliteDataReader reader = null;
		try
		{
			reader = mCommand.ExecuteReader();
		}
		catch (Exception) { }
		return reader;
	}
	public void queryNonReader(string queryString)
	{
		if (mCommand == null)
		{
			return;
		}
		mCommand.CommandText = queryString;
		try
		{
			mCommand.ExecuteNonQuery();
		}
		catch (Exception) { }
	}
	public SQLiteTable getTable(Type type)
	{
		mTableList.TryGetValue(type, out SQLiteTable table);
		return table;
	}
	//---------------------------------------------------------------------------------------------------------------
	protected void clearConnection()
	{
		if (mConnection == null)
		{
			return;
		}
		mConnection.Close();
		mConnection.Dispose();
		mConnection = null;
	}
	protected void clearCommand()
	{
		if (mCommand == null)
		{
			return;
		}
		mCommand.Cancel();
		mCommand.Dispose();
		mCommand = null;
	}
}
#endif