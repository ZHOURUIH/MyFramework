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
	protected string mDecryptFileName;							// SQLite本地缓存文件名(保留旧字段名兼容现有接口)
	protected string mTableName;								// 表格名称
	protected Type mDataClassType;                              // 数据类型
	protected LOAD_STATE mState;                                // 加载状态
	protected bool mResourceAvailable;							// 资源文件是否已经可以使用,加载前需要确保资源更新完毕,而不是读取到旧资源
	protected bool mLoadedFromCache;							// 本次是否直接打开已有明文SQLite文件,没有经过AssetBundle/TextAsset
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
		mLoadedFromCache = false;
	}
	// 异步加载SQLite。数据库已经改为明文后,优先直接打开本地版本缓存;
	// 只有首次安装或资源版本变化时才从AssetBundle取出bytes并落盘一次。
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
		mLoadedFromCache = false;
		prepareDatabaseFileName();
		if (tryOpenExistingDatabase())
		{
			mState = LOAD_STATE.LOADED;
			mLoadedFromCache = true;
			callback?.Invoke();
			return;
		}
		if (mResourceManager != null)
		{
			mResourceManager.loadGameResourceAsync<TextAsset>(R_SQLITE_PATH + mTableName + ".bytes", (textAsset)=>
			{
				postLoad(textAsset.get().bytes);
				mResourceManager?.unload(ref textAsset);
				mState = mCommand != null ? LOAD_STATE.LOADED : LOAD_STATE.NONE;
				callback?.Invoke();
			});
		}
		else
		{
			mState = LOAD_STATE.NONE;
			callback?.Invoke();
		}
	}
	// 同步加载SQLite。编辑器直接打开Assets/GameResources/SQLite下的明文数据库;
	// Player优先使用本地版本缓存,缓存不存在时才从资源包落盘一次。
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
		mLoadedFromCache = false;
		prepareDatabaseFileName();
		if (tryOpenExistingDatabase())
		{
			mState = LOAD_STATE.LOADED;
			mLoadedFromCache = true;
			return;
		}
		if (mResourceManager != null)
		{
			ResourceRef<TextAsset> textAsset = mResourceManager.loadGameResource<TextAsset>(R_SQLITE_PATH + mTableName + ".bytes");
			postLoad(textAsset.get().bytes);
			mResourceManager.unload(ref textAsset);
			mState = mCommand != null ? LOAD_STATE.LOADED : LOAD_STATE.NONE;
		}
		else if (isEditor())
		{
			// 正常情况下编辑器会在tryOpenExistingDatabase中直接打开源文件,这里只是保留兜底。
			var textAsset = loadAssetAtPath<TextAsset>(P_SQLITE_PATH + mTableName + ".bytes");
			postLoad(textAsset.bytes);
			Resources.UnloadAsset(textAsset);
			mState = mCommand != null ? LOAD_STATE.LOADED : LOAD_STATE.NONE;
		}
		else
		{
			mState = LOAD_STATE.NONE;
		}
	}
	public string getDecryptFileName() { return mDecryptFileName; }
	public bool isLoadedFromCache() { return mLoadedFromCache; }
	public static string getDecryptFilePath()
	{
		// SQLite缓存使用独立目录,不要再复用通用temp目录,避免清理时误删其他临时文件。
		return FrameUtility.availableWritePath("sqlite/");
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
		closeConnectionOnly();
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
	// 根据当前资源版本生成稳定的SQLite缓存名。资源版本变化时自然切换到新文件,
	// 不再为了判断文件是否变化而对整个数据库做MD5。
	protected void prepareDatabaseFileName()
	{
		if (!mDecryptFileName.isEmpty())
		{
			return;
		}
		string version = mAssetVersionSystem?.getPersistentAssetsVersion();
		if (version.isEmpty())
		{
			version = Application.version;
		}
		if (version.isEmpty())
		{
			version = "default";
		}
		version = version.Replace('/', '_').Replace('\\', '_').Replace(':', '_').Replace('*', '_').Replace('?', '_').Replace('"', '_').Replace('<', '_').Replace('>', '_').Replace('|', '_');
		mDecryptFileName = mTableName + "_" + version + ".db";
	}
	// 编辑器直接打开源SQLite文件;Player打开按资源版本缓存到可写目录的明文SQLite文件。
	protected string getDatabaseFileFullPath()
	{
		if (isEditor())
		{
			return F_GAME_RESOURCES_PATH + SQLITE + "/" + mTableName + ".bytes";
		}
		prepareDatabaseFileName();
		return getDecryptFilePath() + mDecryptFileName;
	}
	// 已有可直接使用的数据库时完全跳过AssetBundle/TextAsset加载。
	protected bool tryOpenExistingDatabase()
	{
		string path = getDatabaseFileFullPath();
		if (!isFileExist(path))
		{
			return false;
		}
		try
		{
			openDatabase(path);
			return mCommand != null;
		}
		catch (Exception e)
		{
			closeConnectionOnly();
			// Player缓存可能因为上次写入过程中异常退出而损坏,删掉后从资源重新生成。
			if (!isEditor())
			{
				deleteFile(path);
			}
			logWarning("SQLite缓存打开失败,将从资源重新生成, table:" + mTableName + ", error:" + e.Message);
			return false;
		}
	}
	protected void closeConnectionOnly()
	{
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
	}
	protected void openDatabase(string path)
	{
		mConnection = new("URI=file:" + path);
		mConnection.Open();
		mCommand = mConnection.CreateCommand();
		onConnectionOpened();
	}

	// 明文SQLite后处理:不解密、不计算整文件MD5。Player只在当前资源版本缓存不存在时写入一次。
	protected void postLoad(byte[] fileBuffer)
	{
		try
		{
			// SQLite明文数据库固定以"SQLite format 3\0"开头。这里保留一个极低成本检查,
			// 防止资源制作流程还在输出旧的加密文件时把无效数据写入缓存。
			if (fileBuffer == null || fileBuffer.Length < 16 ||
				fileBuffer[0] != (byte)'S' || fileBuffer[1] != (byte)'Q' || fileBuffer[2] != (byte)'L' || fileBuffer[3] != (byte)'i' ||
				fileBuffer[4] != (byte)'t' || fileBuffer[5] != (byte)'e' || fileBuffer[6] != (byte)' ' || fileBuffer[7] != (byte)'f' ||
				fileBuffer[8] != (byte)'o' || fileBuffer[9] != (byte)'r' || fileBuffer[10] != (byte)'m' || fileBuffer[11] != (byte)'a' ||
				fileBuffer[12] != (byte)'t' || fileBuffer[13] != (byte)' ' || fileBuffer[14] != (byte)'3' || fileBuffer[15] != 0)
			{
				logError("SQLite资源不是明文数据库,请确认已关闭SQLite资源加密并重新生成资源, table:" + mTableName);
				return;
			}
			prepareDatabaseFileName();
			string newPath = getDatabaseFileFullPath();
			if (!isEditor())
			{
				// 正常情况下版本文件不存在才会走到这里。额外比较长度用于修复上一次异常中断留下的残缺文件。
				if (!isFileExist(newPath) || getFileSize(newPath) != fileBuffer.Length)
				{
					writeFile(newPath, fileBuffer);
				}
			}
			openDatabase(newPath);
		}
		catch (Exception e)
		{
			closeConnectionOnly();
			mState = LOAD_STATE.NONE;
			logException(e, "打开数据库失败");
		}
	}
	// SQLite连接创建完成后的扩展点.仅用于表级索引/PRAGMA等一次性初始化,避免业务查询第一次命中时再承担初始化开销.
	protected virtual void onConnectionOpened() { }
}
#endif