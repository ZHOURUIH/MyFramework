#if USE_SQLITE
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 表示一个SQLite表格
public class SQLiteTable : ClassObject
{
	protected Dictionary<int, SQLiteData> mDataMap = new();     // 以数据ID为索引的数据缓存列表
	protected SqliteConnection mConnection;						// SQLite所需的Connection
	protected SqliteCommand mCommand;							// SQLite所需的Command
	protected string mDecryptFileName;							// 解密以后的文件名
	protected string mTableName;								// 表格名称
	protected Type mDataClassType;                              // 数据类型
	protected LOAD_STATE mState;                                // 加载状态
	protected bool mResourceAvailable;							// 资源文件是否已经可以使用,加载前需要确保资源更新完毕,而不是读取到旧资源
	public override void resetProperty()
	{
		base.resetProperty();
		mDataMap.Clear();
		mConnection = null;
		mCommand = null;
		mDecryptFileName = null;
		mTableName = null;
		mDataClassType = null;
		mState = LOAD_STATE.NONE;
		mResourceAvailable = false;
	}
	// 返回值是解析以后生成的文件名
	public void loadAsync(Action callback)
	{
		if (!mResourceAvailable && isPlaying())
		{
			logError("表格资源当前不可使用,无法加载,type:" + mTableName);
			return;
		}
		if (mState == LOAD_STATE.LOADED)
		{
			callback?.Invoke();
			return;
		}
		mState = LOAD_STATE.LOADING;
		if (mResourceManager != null)
		{
			mResourceManager.loadGameResourceAsync<TextAsset>(R_SQLITE_PATH + mTableName + ".bytes", (textAsset)=>
			{
				mState = LOAD_STATE.LOADED;
				postLoad(textAsset.get().bytes);
				mResourceManager?.unload(ref textAsset);
				callback?.Invoke();
			});
		}
		else
		{
			mState = LOAD_STATE.LOADED;
			callback?.Invoke();
		}
	}
	// 同步加载表格数据,先从资源包加载bytes,再解密并创建SQLite连接
	public void load()
	{
		if (!mResourceAvailable && isPlaying())
		{
			logError("表格资源当前不可使用,无法加载,type:" + mTableName);
			return;
		}
		if (mState == LOAD_STATE.LOADED)
		{
			return;
		}
		mState = LOAD_STATE.LOADING;
		if (mResourceManager != null)
		{
			ResourceRef<TextAsset> textAsset = mResourceManager.loadGameResource<TextAsset>(R_SQLITE_PATH + mTableName + ".bytes");
			mState = LOAD_STATE.LOADED;
			postLoad(textAsset.get().bytes);
			mResourceManager.unload(ref textAsset);
		}
		else
		{
			if (!isEditor())
			{
				return;
			}
			var textAsset = loadAssetAtPath<TextAsset>(P_SQLITE_PATH + mTableName + ".bytes");
			mState = LOAD_STATE.LOADED;
			postLoad(textAsset.bytes);
			Resources.UnloadAsset(textAsset);
		}
	}
	public string getDecryptFileName() { return mDecryptFileName; }
	public static string getDecryptFilePath()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return F_TEMPORARY_CACHE_PATH + "../../" + getFolderName(F_PROJECT_PATH) + "/";
#else
		return FrameUtility.availableWritePath("temp/");
#endif
	}
	public override void destroy()
	{
		base.destroy();
		clearAll();
	}
	// 执行SQL查询,返回数据读取器
	public SqliteDataReader queryReader(string queryString)
	{
		if (mCommand == null)
		{
			return null;
		}
		mCommand.CommandText = queryString;
		try
		{
			return mCommand.ExecuteReader();
		}
		catch (Exception) { }
		return null;
	}
	// 执行SQL非查询语句(INSERT/UPDATE/DELETE)
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
	public virtual void checkAllData() { }
	public void setResourceAvailable(bool available) { mResourceAvailable = available; }
	public void setDataType(Type dataClassType) { mDataClassType = dataClassType; }
	public void setTableName(string name) { mTableName = name; }
	public string getTableName() { return mTableName; }
	// 执行条件查询,condition为空则查询全部
	public SqliteDataReader doQuery(string condition = null)
	{
		if (!condition.isEmpty())
		{
			return queryReader("SELECT * FROM " + mTableName + " WHERE " + condition);
		}
		else
		{
			return queryReader("SELECT * FROM " + mTableName);
		}
	}
	// 执行UPDATE更新语句
	public void doUpdate(string updateString, string conditionString)
	{
		queryNonReader(strcat("UPDATE ", mTableName, " SET ", updateString, " WHERE ", conditionString));
	}
	// 执行INSERT插入语句
	public void doInsert(string valueString)
	{
		queryNonReader(strcat("INSERT INTO ", mTableName, " VALUES (", valueString, ")"));
	}
	// 校验单个数据引用是否存在
	public void checkData(int checkID, int dataID, ExcelTable refTable)
	{
		if (checkID > 0 && queryInternal(checkID, false) == null)
		{
			logError("can not find item id:" + checkID + " in " + mTableName + ", ref ID:" + dataID + ", ref Table:" + refTable.getTableName());
		}
	}
	// 校验多个数据引用是否存在(int列表)
	public void checkData(List<int> checkIDList, int dataID, ExcelTable refTable)
	{
		foreach (int id in checkIDList)
		{
			if (queryInternal(id, false) == null)
			{
				logError("can not find item id:" + id + " in " + mTableName + ", ref ID:" + dataID + ", ref Table:" + refTable.getTableName());
			}
		}
	}
	// 校验多个数据引用是否存在(ushort列表)
	public void checkData(List<ushort> checkIDList, int dataID, ExcelTable refTable)
	{
		foreach (int id in checkIDList)
		{
			if (queryInternal(id, false) == null)
			{
				logError("can not find item id:" + id + " in " + mTableName + ", ref ID:" + dataID + ", ref Table:" + refTable.getTableName());
			}
		}
	}
	// 校验两个列表长度是否一致
	public void checkListPair<T0, T1>(List<T0> list0, List<T1> list1, int dataID)
	{
		if (list0.Count != list1.Count)
		{
			logError("list pair size not match, table:" + mTableName + ", ref ID:" + dataID);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 将数据插入到SQLite表格中,同时缓存到内存
	protected void insertInternal(SQLiteData data)
	{
		if (mDataMap.ContainsKey(data.mID))
		{
			return;
		}
		if (data.GetType() != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + data.GetType());
			return;
		}
		using var a = new MyStringBuilderScope(out var valueString);
		data.insert(valueString);
		valueString.removeLastComma();
		doInsert(valueString.ToString());
		mDataMap.Add(data.mID, data);
	}
	// 按ID查询数据,先从内存缓存查找,未命中则查询SQLite并加入缓存
	protected SQLiteData queryInternal(int id, bool errorIfNull = true)
	{
		if (id <= 0)
		{
			if (errorIfNull)
			{
				logError("表格中找不到指定数据: ID:" + id + ", Type:" + mDataClassType);
			}
			return null;
		}
		if (mDataMap.TryGetValue(id, out SQLiteData data))
		{
			return data;
		}
		using var a = new MyStringBuilderScope(out var condition);
		condition.addConditionInt(SQLiteData.ID, id, EMPTY);
		parseReader(doQuery(condition.ToString()), out data);
		mDataMap.Add(id, data);
		if (data == null && errorIfNull)
		{
			logError("表格中找不到指定数据: ID:" + id + ", Type:" + mDataClassType);
		}
		return data;
	}
	// 清理所有资源,关闭SQLite连接并清空缓存
	protected void clearAll()
	{
		mState = LOAD_STATE.NONE;
		if (mCommand != null)
		{
			mCommand.Cancel();
			mCommand.Dispose();
			mCommand = null;
		}
		if (mConnection != null)
		{
			mConnection.Close();
			mConnection.Dispose();
			mConnection = null;
		}
		mDataMap.Clear();
	}
	// 从SqliteDataReader中解析单条数据(带类型检查)
	protected void parseReader(Type type, SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if (reader == null)
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
	// 从SqliteDataReader中解析单条数据(使用已注册的数据类型)
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
	// 从SqliteDataReader中解析多条数据到列表中
	protected void parseReader<T>(Type type, SqliteDataReader reader, List<T> dataList) where T : SQLiteData
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
		using var a = new ProfilerScope("sqlite parseReader " + mTableName);
		while (reader.Read())
		{
			var data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
			dataList.Add(data as T);
		}
		reader.Close();
	}
	// 获取解密后的文件完整路径
	protected string getDecryptFileFullPath()
	{
		if (isEditor())
		{
			return getDecryptFilePath() + mTableName + "/" + mTableName;
		}
		else
		{
			return getDecryptFilePath() + mDecryptFileName;
		}
	}
	// 加载完成后的后处理:解密数据、写入临时文件、创建SQLite连接
	protected void postLoad(byte[] fileBuffer)
	{
		try
		{
			// 解密文件,只解密128分之1的数据,减少耗时
			string suffixKey = "23y35y9832635872349862365274732047chsudhgkshgwshfoweh238c42384fync9388v45982nc3484";
			byte[] encryptKey = (generateFileMD5(("ASLD" + mTableName).toBytes()).ToUpper() + suffixKey).toBytes();
			int fileSize = fileBuffer.Length;
			int index = 0;
			for (int i = 0; i < fileSize >> 7; ++i)
			{
				fileBuffer[i] ^= encryptKey[index];
				if (++index >= encryptKey.Length)
				{
					index = 0;
				}
			}

			// 将解密后的数据写入新的目录,需要写入临时目录,编辑器中写入固定路径即可
			mDecryptFileName = generateFileMD5(fileBuffer).ToUpper();
			string newPath = getDecryptFileFullPath();
			// 编辑器下每次都写入更新
			if (isEditor() || !isFileExist(newPath))
			{
				writeFile(newPath, fileBuffer);
			}

			// 创建连接
			if (isFileExist(newPath))
			{
				mConnection = new("URI=file:" + newPath);
				mConnection.Open();
			}
			mCommand = mConnection?.CreateCommand();
		}
		catch (Exception e)
		{
			destroy();
			logException(e, "打开数据库失败");
		}
	}
}
#endif