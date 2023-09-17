using System;
using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static StringUtility;
using static FileUtility;
using static CSharpUtility;
using static BinaryUtility;
using static FrameDefine;
using UnityEngine.Profiling;

// Excel表格的基类,表示一个表格
public class ExcelTable
{
	private Dictionary<int, ExcelData> mDataMap;	// 按数据ID进行索引的数据列表
	protected Type mDataType;                       // 数据类型
	protected string mTableName;                    // 表格名字
	protected bool mResourceAvailable;				// 资源文件是否已经可以使用,加载前需要确保资源更新完毕,而不是读取到旧资源
	public ExcelTable()
	{
		mDataMap = new Dictionary<int, ExcelData>();
	}
	public string getTableName() { return mTableName; }
	public void setTableName(string name) { mTableName = name; }
	public void setClassType(Type type) { mDataType = type; }
	public void setResourceAvailable(bool available) { mResourceAvailable = available; }
	public void readFile()
	{
		string fileName = R_EXCEL_PATH + mTableName + ".bytes";
		var res = mResourceManager.loadResource<TextAsset>(fileName);
		Profiler.BeginSample("excel read");
		read(res.bytes);
		Profiler.EndSample();
		mResourceManager.unload(ref res);
	}
	public void read(byte[] fileBuffer)
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
		int fileLength = fileBuffer.Length;
		// 解密
		string key = generateFileMD5(stringToBytes("ASLD" + mTableName)) + "23y35y983";
		int keyIndex = 0;
		for (int i = 0; i < fileLength; ++i)
		{
			fileBuffer[i] = (byte)((fileBuffer[i] ^ (byte)key[keyIndex]) + ((i << 1) & 0xFF));
			if (++keyIndex >= key.Length)
			{
				keyIndex = 0;
			}
		}

		// 解析数据
		CLASS(out SerializerRead reader);
		reader.init(fileBuffer, fileLength);
		while (true)
		{
			if (reader.getIndex() >= reader.getDataSize())
			{
				break;
			}
			var data = createInstance<ExcelData>(mDataType);
			data.read(reader);
			mDataMap.Add(data.mID, data);
		}
		UN_CLASS(ref reader);
	}
	public T getData<T>(int id, bool errorIfNull = true) where T : ExcelData
	{
		if (mDataMap.Count == 0)
		{
			readFile();
		}
		mDataMap.TryGetValue(id, out ExcelData data);
		if (data as T == null && errorIfNull)
		{
			logError("在表格中找不到数据: ID:" + id + ", 表格:" + mDataType);
		}
		return data as T;
	}
	public ExcelData getData(int id, bool errorIfNull = true)
	{
		if (mDataMap.Count == 0)
		{
			readFile();
		}
		mDataMap.TryGetValue(id, out ExcelData data);
		if (data == null && errorIfNull)
		{
			logError("在表格中找不到数据: ID:" + id + ", 表格:" + mDataType);
		}
		return data;
	}
	public Dictionary<int, ExcelData> getDataList() 
	{
		if (mDataMap.Count == 0)
		{
			readFile();
		}
		return mDataMap; 
	}
	public virtual void checkData() { }
}