#if !UNITY_IOS && !NO_SQLITE
using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections.Generic;
using System;

public class SQLite : FrameComponent
{
	protected SqliteConnection mConnection;
	protected SqliteCommand mCommand;
	protected Dictionary<Type, SQLiteTable> mTableList;
	public SQLite(string name)
		: base(name)
	{
		mTableList = new Dictionary<Type, SQLiteTable>();
		try
		{
			string fullPath = CommonDefine.F_ASSETS_DATA_BASE_PATH + GameDefine.DATA_BASE_FILE;
			if (isFileExist(fullPath))
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				// 将文件拷贝到persistentDataPath目录中,因为只有该目录才拥有读写权限
				string persisFullPath = CommonDefine.F_PERSIS_DATA_BASE_PATH + GameDefine.DATA_BASE_FILE;
				logInfo("persisFullPath:" + persisFullPath, LOG_LEVEL.LL_FORCE);
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
			logInfo("打开数据库失败:" + e.Message, LOG_LEVEL.LL_FORCE);
		}
	}
	public T registeTable<T>(string tableName) where T : SQLiteTable, new()
	{
		T table = new T();
		table.setTableName(tableName);
		mTableList.Add(typeof(T), table);
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
			mConnection = new SqliteConnection("URI=file:" + fullFileName);   // 创建SQLite对象的同时，创建SqliteConnection对象  
			mConnection.Open();                         // 打开数据库链接
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
			connect(CommonDefine.F_PERSIS_DATA_BASE_PATH + GameDefine.DATA_BASE_FILE);
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
	public void getTable<T>(out T table) where T : SQLiteTable
	{
		table = null;
		if (mTableList.ContainsKey(typeof(T)))
		{
			table = mTableList[typeof(T)] as T;
		}
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