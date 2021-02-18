using System;
using System.Collections.Generic;

public class DataBase : FrameSystem
{
	protected Dictionary<Type, List<Data>> mDataStructList;
	protected Dictionary<string, Type> mDataFileDefine;
	protected Dictionary<Type, string> mDataDefineFile;
	protected Dictionary<Type, int> mDataSizeMap;
	public DataBase()
	{
		mDataStructList = new Dictionary<Type,List<Data>>();
		mDataFileDefine = new Dictionary<string, Type>();
		mDataDefineFile = new Dictionary<Type, string>();
		mDataSizeMap = new Dictionary<Type, int>();
	}
	// 初始化所有数据
	public override void init()
	{
		base.init();
		loadAllDataFromFile();
	}
	public override void destroy() 
	{
		base.destroy();
		mDataStructList.Clear();
	}
	public Dictionary<Type, string> getDataDefineFile() { return mDataDefineFile; }
	public void loadAllDataFromFile()
	{
		// 此处只能加载本地的文件,如果资源根目录在远端,则需要手动加载
		if(ResourceManager.mLocalRootPath)
		{
			foreach (var item in mDataFileDefine)
			{
				string filePath = ResourceManager.mResourceRootPath + FrameDefine.SA_GAME_DATA_FILE_PATH + item.Key + FrameDefine.DATA_SUFFIX;
				if(!isFileExist(filePath))
				{
					continue;
				}
				byte[] file;
				int fileSize = openFile(filePath, out file, true);
				if (file != null && fileSize > 0)
				{
					parseFile(file, fileSize, item.Value);
				}
				releaseFileBuffer(file);
			}
		}
	}
	public void parseFile(byte[] file, int fileSize, Type type)
	{
		if(mDataStructList.ContainsKey(type))
		{
			return;
		}
		// 解析文件
		List<Data> dataList = new List<Data>();
		int dataSize = getDataSize(type);
		byte[] dataBuffer = mBytesPool.newBytes(getGreaterPow2(dataSize));
		int dataCount = fileSize / dataSize;
		for (int i = 0; i < dataCount; ++i)
		{
			Data newData = createInstance<Data>(type);
			if (newData == null)
			{
				logError("can not create data ,type:" + type);
				return;
			}
			newData.setType(type);
			memcpy(dataBuffer, file, 0, i * dataSize, dataSize);
			newData.read(dataBuffer);
			dataList.Add(newData);
		}
		mBytesPool.destroyBytes(dataBuffer);
		mDataStructList.Add(type, dataList);
	}
	public void destroyAllData()
	{
		mDataStructList.Clear();
	}
	public void destroyData(Type type)
	{
		mDataStructList.Remove(type);
	}
	public List<Data> getAllData(Type type)
	{
		mDataStructList.TryGetValue(type, out List<Data> dataList);
		return dataList;
	}
	// 得到数据数量
	public int getDataCount(Type type)
	{
		if(mDataStructList.TryGetValue(type, out List<Data> dataList))
		{
			return dataList.Count;
		}
		return 0;
	}
	// 查询数据
	public Data queryData(Type type, int index)
	{
		if (mDataStructList.TryGetValue(type, out List<Data> dataList))
		{
			return dataList[index];
		}
		return null;
	}
	public void addData(Type type, Data data, int pos = -1)
	{
		if (data == null)
		{
			return;
		}
		if (mDataStructList.TryGetValue(type, out List<Data> dataList))
		{
			if (pos == -1)
			{
				dataList.Add(data);
			}
			else if (pos >= 0 && pos <= dataList.Count)
			{
				dataList.Insert(pos, data);
			}
		}
		else
		{
			dataList = new List<Data>();
			dataList.Add(data);
			mDataStructList.Add(type, dataList);
		}
	}
	public void registeData(Type classType)
	{
		Data temp = createInstance<Data>(classType);
		temp.setType(classType);
		string dataName = classType.ToString();
		mDataFileDefine.Add(dataName, classType);
		mDataDefineFile.Add(classType, dataName);
		mDataSizeMap.Add(classType, temp.getDataSize());
	}
	// 根据数据名得到数据定义
	public string getDataNameByDataType(Type type)
	{
		mDataDefineFile.TryGetValue(type, out string name);
		return name;
	}
	// 根据数据定义得到数据名
	public Type getDataTypeByDataName(string name)
	{
		mDataFileDefine.TryGetValue(name, out Type type);
		return type;
	}
	public int getDataSize(Type type)
	{
		mDataSizeMap.TryGetValue(type, out int size);
		return size;
	}
}