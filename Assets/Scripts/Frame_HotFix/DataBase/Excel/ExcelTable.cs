using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;
using static FileUtility;
using static FrameUtility;
using static BinaryUtility;
using static StringUtility;
using static FrameDefine;
using static FrameBaseUtility;

// Excel表格的基类,表示一个表格
public class ExcelTable
{
	private Dictionary<int, ExcelData> mDataMap = new();    // 按数据ID进行索引的数据列表
	protected TextAsset mTableFileData;						// 未解析的表格文件数据
	protected byte[] mTableFileBytes;						// 为了支持非运行时也能够加载表格,所以可以传一个byte[],mTableFileBytes和mTableFileData只需要有一个有效就行
	protected Type mDataType;								// 数据类型
	protected string mTableName;							// 表格名字
	protected bool mResourceAvailable;						// 资源文件是否已经可以使用,加载前需要确保资源更新完毕,而不是读取到旧资源
	protected static Dictionary<string, bool> mCheckPathResultMap = new();	// 为了提高路径检查的效率,避免对相同的路径重复检查而耗费大量时间
	public string getTableName() { return mTableName; }
	public void setTableName(string name) { mTableName = name; }
	public void setDataType(Type type) { mDataType = type; }
	public void setResourceAvailable(bool available) { mResourceAvailable = available; }
	public bool isFileOpened() { return mDataMap.Count > 0 || mTableFileData != null || mTableFileBytes != null; }
	public virtual void checkAllData() { checkAllDataDefault(); }
	public void openFileAsync(Action callback)
	{
		if (!mResourceAvailable && isPlaying())
		{
			logError("表格资源当前不可使用,无法加载,type:" + mTableName);
			return;
		}
		if (mResourceManager == null)
		{
			callback?.Invoke();
			return;
		}
		string fileName = R_EXCEL_PATH + mTableName + ".bytes";
		mResourceManager.loadGameResourceAsync(fileName, (TextAsset asset) =>
		{
			if (asset == null)
			{
				logError("表格文件加载失败:" + mTableName + ", path:" + fileName);
			}
			mTableFileData = asset;
			onOpenFile();
			callback?.Invoke();
		});
	}
	public void setTableFileBytes(byte[] bytes) { mTableFileBytes = bytes; }
	public void reload()
	{
		//  如果已经有数据了，需要重新读一遍替换掉已有的
		if (mDataMap.Count > 0)
		{
			clearCache();
			clear();
			parseFileReload(mTableFileBytes ?? mTableFileData.bytes);
			mResourceManager?.unload(ref mTableFileData);
		}
	}
	public void parseFile(byte[] fileBuffer)
	{
		if (fileBuffer == null)
		{
			return;
		}

		// 解密
		decodeFile(fileBuffer);

		// 解析数据
		using var a = new ClassScope<SerializerRead>(out var reader);
		reader.init(fileBuffer);
		while (reader.getIndex() < reader.getDataSize())
		{
			var data = createInstance<ExcelData>(mDataType);
			if (!data.read(reader))
			{
				logError("表格解析失败,表格:" + mTableName + ", ID:" + data.mID);
				break;
			}
			if (!mDataMap.TryAdd(data.mID, data))
			{
				logError("表格中存在重复ID,表格:" + mTableName + ", ID:" + data.mID);
			}
		}
	}
	public void checkEnum<T>(T value, string varName, int dataID) where T : Enum
	{
		if (!isEnumValid(value))
		{
			logError("enum value error,name:" + varName + " in " + mTableName + ", ID:" + IToS(dataID) + ", Table:" + getTableName());
		}
	}
	public void checkEnum<T>(List<T> valueList, string varName, int dataID) where T : Enum
	{
		foreach (T value in valueList)
		{
			if (!isEnumValid(value))
			{
				logError("enum value error,name:" + varName + " in " + mTableName + ", ID:" + IToS(dataID) + ", Table:" + getTableName());
			}
		}
	}
	public void checkData(int id, int refDataID, string tableName)
	{
		if (id > 0 && getData(id, false) == null)
		{
			logError(mTableName + "中ID不存在:" + id + ", 引用此数据的表格:" + tableName + ", ID:" + refDataID);
		}
	}
	public void checkData(int id, int refDataID, ExcelTable table)
	{
		checkData(id, refDataID, table.mTableName);
	}
	public void checkData(List<int> ids, int refDataID, string tableName)
	{
		foreach (int id in ids)
		{
			if (getData(id, false) == null)
			{
				logError(mTableName + "中ID不存在:" + id + ", 引用此数据的表格:" + tableName + ", ID:" + refDataID);
			}
		}
	}
	public void checkData(List<int> ids, int refDataID, ExcelTable table)
	{
		checkData(ids, refDataID, table.mTableName);
	}
	public void checkData(List<ushort> ids, int refDataID, string tableName)
	{
		foreach (int id in ids)
		{
			if (getData(id, false) == null)
			{
				logError(mTableName + "中ID不存在:" + id + ", 引用此数据的表格:" + tableName + ", ID:" + refDataID);
			}
		}
	}
	public void checkData(List<ushort> ids, int refDataID, ExcelTable table)
	{
		checkData(ids, refDataID, table.mTableName);
	}
	public void checkListPair(IList list0, IList list1, int id)
	{
		if (list0.Count != list1.Count)
		{
			logError("列表长度不一致, ID:" + id + ", 表格:" + mTableName + ", 第一个长度:" + list0.Count + ", 第二个长度:" + list1.Count);
		}
	}
	public static void checkPath(string path, bool checkSpace = true)
	{
		if (!mCheckPathResultMap.TryAdd(path, true))
		{
			return;
		}
		if (path.Contains('\\'))
		{
			logError("文件路径存在反斜杠:" + path);
		}
		if (checkSpace && (path.Contains(' ') || path.Contains('　')))
		{
			logError("文件路径存在空格:" + path);
		}
		// 只在编辑器下才会检查
		if (isEditor() && !isFileExist(F_GAME_RESOURCES_PATH + path))
		{
			logError("文件不存在:" + path);
		}
	}
	// 清除数据和查询缓存
	public virtual void clear(){}
	public virtual void clearCache(){}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void onOpenFile() { }
	// 解密
	protected void decodeFile(byte[] fileBuffer)
	{
		string key = generateFileMD5(stringToBytes("ASLD" + mTableName)).ToUpper() + "23y35y983";
		int keyIndex = 0;
		int fileLength = fileBuffer.Length;
		for (int i = 0; i < fileLength; ++i)
		{
			fileBuffer[i] = (byte)((fileBuffer[i] ^ (byte)key[keyIndex]) + ((i << 1) & 0xFF));
			if (++keyIndex >= key.Length)
			{
				keyIndex = 0;
			}
		}
	}
	protected void readFile()
	{
		{
			using var a = new ProfilerScope("excel read:" + mTableName);
			parseFile(mTableFileBytes ?? mTableFileData.bytes);
		}
		// 解析以后就可以卸载文件数据
		mResourceManager?.unload(ref mTableFileData);
	}
	// 热重载表格数据
	protected void parseFileReload(byte[] fileBuffer)
	{
		if (!mResourceAvailable)
		{
			logError("表格资源当前不可使用,无法加载,type:" + mTableName);
			return;
		}
		if (fileBuffer == null)
		{
			return;
		}

		// 解密
		decodeFile(fileBuffer);

		using var a = new HashSetScope<int>(out var idsToRemove);
		idsToRemove.addRange(mDataMap.Keys);
		// 解析数据
		using var b = new ClassScope<SerializerRead>(out var reader);
		reader.init(fileBuffer);
		while (reader.getIndex() < reader.getDataSize())
		{
			// 假定id是不会变的。先读一个id用于和已有数据对应
			int index = reader.getIndex();
			reader.read(out int id);
			reader.setIndex(index);
			idsToRemove.Remove(id);
			// 如果已有同id的数据，那就重新读一遍来替换；否则需要创建并添加。
			if (mDataMap.TryGetValue(id, out ExcelData data))
			{
				if (!data.read(reader))
				{
					break;
				}
			}
			else
			{
				data = createInstance<ExcelData>(mDataType);
				if (!data.read(reader))
				{
					logError("表格解析失败,表格:" + mTableName + ", ID:" + data.mID);
					break;
				}
				if (!mDataMap.TryAdd(data.mID, data))
				{
					logError("表格中存在重复ID,表格:" + mTableName + ", ID:" + data.mID);
				}
			}
		}
		// 最后删除减少的行
		foreach (int k in idsToRemove)
		{
			mDataMap.Remove(k);
		}

		log("热重载表格数据：" + mTableName);
	}
	// 为了避免歧义,getData,getDataMap设置为不允许外部访问
	protected T getData<T>(int id, bool errorIfNull = true) where T : ExcelData
	{
		ExcelData data = null;
		if (id > 0)
		{
			if (mDataMap.Count == 0)
			{
				readFile();
			}
			data = mDataMap.get(id);
		}
		if (data as T == null && errorIfNull)
		{
			logError("在表格中找不到数据: ID:" + id + ", 表格:" + mDataType);
		}
		return data as T;
	}
	protected ExcelData getData(int id, bool errorIfNull = true)
	{
		if (mDataMap.Count == 0)
		{
			readFile();
		}
		ExcelData data = mDataMap.get(id);
		if (data == null && errorIfNull)
		{
			logError("在表格中找不到数据: ID:" + id + ", 表格:" + mDataType);
		}
		return data;
	}
	protected Dictionary<int, ExcelData> getDataMap()
	{
		if (mDataMap.Count == 0)
		{
			readFile();
		}
		return mDataMap;
	}
	protected virtual void checkAllDataDefault() { }
}