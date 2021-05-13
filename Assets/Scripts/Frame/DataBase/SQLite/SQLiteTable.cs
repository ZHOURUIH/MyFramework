#if !UNITY_IOS && !NO_SQLITE
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;

public class SQLiteTable : FrameBase
{
	protected Dictionary<int, SQLiteData> mDataList;
	protected SqliteConnection mConnection;
	protected SqliteCommand mCommand;
	protected string mTableName;
	protected Type mDataClassType;
	protected bool mFullData;		// 数据是否已经全部查询过了
	public SQLiteTable()
	{
		mDataList = new Dictionary<int, SQLiteData>();
	}
	public void init(byte[] encryptKey)
	{
		try
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			string fullPath = FrameDefine.F_PERSIS_DATA_BASE_PATH + mTableName + ".db";
#else
			string fullPath = FrameDefine.F_ASSETS_DATA_BASE_PATH + mTableName + ".db";
#endif
			if (isFileExist(fullPath))
			{
				// 解密文件
				byte[] fileBuffer;
				int fileSize = openFile(fullPath, out fileBuffer, true);
				for(int i = 0; i < fileSize; ++i)
				{
					fileBuffer[i] ^= encryptKey[i % encryptKey.Length];
				}
				// 将解密后的数据写入新的目录
				string newPath = getFilePath(fullPath) + "/temp/" + mTableName + ".db";
				writeFile(newPath, fileBuffer, fileSize);
				connect(newPath);
			}
		}
		catch (Exception e)
		{
			log("打开数据库失败:" + e.Message, LOG_LEVEL.FORCE);
		}
	}
	public void destroy()
	{
		clearCommand();
		clearConnection();
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
	public void setDataType(Type dataClassType) { mDataClassType = dataClassType; }
	public void setTableName(string name) { mTableName = name; }
	public string getTableName() { return mTableName; }
	public void checkSQLite() 
	{
		List<SQLiteData> list = new List<SQLiteData>();
		queryAll(mDataClassType, list);
		int count = list.Count;
		for (int i = 0; i < count; ++i)
		{
			if (!list[i].checkData())
			{
				logError("表格数据发现错误:Type:" + Typeof(this) + ", ID:" + list[i].mID);
			}
		}
	}
	public SqliteDataReader doQuery()
	{
		return queryReader("SELECT * FROM " + mTableName);
	}
	public SqliteDataReader doQuery(string condition)
	{
		return queryReader(END_STRING(STRING("SELECT * FROM ", mTableName, " WHERE ", condition)));
	}
	public void doUpdate(string updateString, string conditionString)
	{
		queryNonReader(END_STRING(STRING("UPDATE ", mTableName, " SET ", updateString, " WHERE ", conditionString)));
	}
	public void doInsert(string valueString)
	{
		queryNonReader(END_STRING(STRING("INSERT INTO ", mTableName, " VALUES (", valueString, ")")));
	}
	public SQLiteData query(int id, out SQLiteData data)
	{
		if (mDataList.TryGetValue(id, out data))
		{
			return data;
		}
		MyStringBuilder condition = STRING();
		appendConditionInt(condition, SQLiteData.ID, id, EMPTY);
		parseReader(doQuery(END_STRING(condition)), out data);
		mDataList.Add(id, data);
		return data;
	}
	public SQLiteData query(Type type, int id, out SQLiteData data)
	{
		if (!mDataList.TryGetValue(id, out data))
		{
			MyStringBuilder condition = STRING();
			appendConditionInt(condition, SQLiteData.ID, id, EMPTY);
			parseReader(doQuery(END_STRING(condition)), out data);
			mDataList.Add(id, data);
		}
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			data = null;
		}
		return data;
	}
	public void insert<T>(T data) where T : SQLiteData
	{
		if(mDataList.ContainsKey(data.mID))
		{
			return;
		}
		if (Typeof(data) != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + Typeof(data));
			return;
		}
		MyStringBuilder valueString = STRING();
		data.insert(valueString);
		removeLastComma(valueString);
		doInsert(END_STRING(valueString));
		mDataList.Add(data.mID, data);
	}
	public void queryAll<T>(Type type, List<T> dataList) where T : SQLiteData
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}

		// 已经全部查询过了,则返回已查询的数据
		if(mFullData)
		{
			dataList.Capacity = mDataList.Count;
			foreach(var item in mDataList)
			{
				dataList.Add(item.Value as T);
			}
			return;
		}

		// 没有全部查询过时,从表中全部查询一次,此处会舍弃之前查询的全部数据,重新申请一个字典,因为只有构造时才能设置初始容量
		mFullData = true;
		parseReader(type, doQuery(), dataList);
		mDataList = new Dictionary<int, SQLiteData>(dataList.Count);
		int count = dataList.Count;
		for (int i = 0; i < count; ++i)
		{
			var item = (SQLiteData)dataList[i];
			mDataList.Add(item.mID, item);
		}
	}
	//---------------------------------------------------------------------------------------------------------------------------
	protected SQLiteData query(Type type, int id)
	{
		SQLiteData data;
		query(type, id, out data);
		return data;
	}
	protected void parseReader(Type type, SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if(reader == null)
		{
			return;
		}
		if (reader.Read())
		{
			data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (reader != null && reader.Read())
		{
			data = createInstance<SQLiteData>(mDataClassType);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(Type type, SqliteDataReader reader, IList dataList)
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if (reader == null)
		{
			return;
		}
		while (reader.Read())
		{
			var data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
			dataList.Add(data);
		}
		reader.Close();
	}
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