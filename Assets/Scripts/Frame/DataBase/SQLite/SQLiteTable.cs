#if !NO_SQLITE
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static CSharpUtility;
using static BinaryUtility;
using static FrameBase;
using static FrameDefine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.Profiling;
#endif
using UnityEngine;
using UnityEditor;

// 表示一个SQLite表格
public class SQLiteTable : ClassObject
{
	protected Dictionary<int, SQLiteData> mDataMap;		// 以数据ID为索引的数据缓存列表
	protected SqliteConnection mConnection;				// SQLite所需的Connection
	protected SqliteCommand mCommand;					// SQLite所需的Command
	protected string mTableName;						// 表格名称
	protected Type mDataClassType;						// 数据类型
	public SQLiteTable()
	{
		mDataMap = new Dictionary<int, SQLiteData>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDataMap.Clear();
		mConnection = null;
		mCommand = null;
		mTableName = null;
		mDataClassType = null;
	}
	public byte[] generateEncryptKey()
	{
		string preKey = "ASLD" + mTableName;
		string key = generateFileMD5(stringToBytes(preKey)) + "23y35y9832635872349862365274732047chsudhgkshgwshfoweh238c42384fync9388v45982nc3484";
		return stringToBytes(key);
	}
	public void load()
	{
		try
		{
			clearCommand();
			clearConnection();
			clearAll();
			// 解密文件
			byte[] encryptKey = generateEncryptKey();
			TextAsset textAsset = null;
			if (mResourceManager != null)
			{
				textAsset = mResourceManager.loadResource<TextAsset>(R_SQLITE_PATH + mTableName + ".bytes");
			}
			else
			{
#if UNITY_EDITOR
				textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(P_SQLITE_PATH + mTableName + ".bytes");
#else
				return;
#endif
			}
			byte[] fileBuffer = textAsset.bytes;
			int fileSize = fileBuffer.Length;
			for (int i = 0; i < fileSize; ++i)
			{
				fileBuffer[i] ^= encryptKey[i % encryptKey.Length];
			}
			// 将解密后的数据写入新的目录,需要写入临时目录,编辑器中写入固定路径即可
#if UNITY_EDITOR
			string newPath = F_TEMPORARY_CACHE_PATH + "../../" + mTableName + "/" + mTableName;
#else
			string folder = generateFileMD5(BinaryUtility.stringToBytes(mTableName));
#if UNITY_STANDALONE_WIN
			string newPath = F_TEMPORARY_CACHE_PATH + "../../" + folder;
#else
			string newPath = FrameUtility.availableWritePath("temp/" + folder);
#endif
#endif
			writeFile(newPath, fileBuffer, fileSize);
			if (mResourceManager != null)
			{
				mResourceManager.unload(ref textAsset);
			}
			else
			{
#if UNITY_EDITOR
				Resources.UnloadAsset(textAsset);
#endif
			}
			connect(newPath);
		}
		catch (Exception e)
		{
			destroy();
			logException(e, "打开数据库失败");
		}
	}
	public void destroy()
	{
		clearCommand();
		clearConnection();
		clearAll();
	}
	public void connect(string fullFileName)
	{
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
	public SqliteDataReader doQuery(string condition = null)
	{
		if (!isEmpty(condition))
		{
			return queryReader("SELECT * FROM " + mTableName + " WHERE " + condition);
		}
		else
		{
			return queryReader("SELECT * FROM " + mTableName);
		}
	}
	public void doUpdate(string updateString, string conditionString)
	{
		queryNonReader(strcat("UPDATE ", mTableName, " SET ", updateString, " WHERE ", conditionString));
	}
	public void doInsert(string valueString)
	{
		queryNonReader(strcat("INSERT INTO ", mTableName, " VALUES (", valueString, ")"));
	}
	public SQLiteData query(int id, bool errorIfNull = true)
	{
		if (mDataMap.TryGetValue(id, out SQLiteData data))
		{
			return data;
		}
		using (new ClassScope<MyStringBuilder>(out var condition))
		{
			appendConditionInt(condition, SQLiteData.ID, id, EMPTY);
			parseReader(doQuery(condition.ToString()), out data);
		}
		mDataMap.Add(id, data);
		if (data == null && errorIfNull)
		{
			logError("表格中找不到指定数据: ID:" + id + ", Type:" + mDataClassType);
		}
		return data;
	}
	public void insert<T>(T data) where T : SQLiteData
	{
		if(mDataMap.ContainsKey(data.mID))
		{
			return;
		}
		if (Typeof(data) != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + Typeof(data));
			return;
		}
		using (new ClassScope<MyStringBuilder>(out var valueString))
		{
			data.insert(valueString);
			removeLastComma(valueString);
			doInsert(valueString.ToString());
		}
		mDataMap.Add(data.mID, data);
	}
	public void queryDataList<T>(string condition, Type type, List<T> dataList) where T : SQLiteData
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		parseReader(type, doQuery(condition), dataList);
		int count = dataList.Count;
		for (int i = 0; i < count; ++i)
		{
			SQLiteData data = dataList[i];
			if (!mDataMap.ContainsKey(data.mID))
			{
				mDataMap.Add(data.mID, data);
			}
		}
	}
	public void queryDataList<T>(Type type, List<T> dataList) where T : SQLiteData
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		parseReader(type, doQuery(), dataList);
		int count = dataList.Count;
		for (int i = 0; i < count; ++i)
		{
			SQLiteData data = dataList[i];
			if (!mDataMap.ContainsKey(data.mID))
			{
				mDataMap.Add(data.mID, data);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void clearAll()
	{
		mDataMap.Clear();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.BeginSample("sqlite parseReader " + mTableName);
#endif
		while (reader.Read())
		{
			var data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
			dataList.Add(data);
		}
		reader.Close();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.EndSample();
#endif
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