using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Excel表格的基类,表示一个表格
public class ExcelTable : FrameBase
{
	protected Dictionary<int, ExcelData> mDataList;	// 按数据ID进行索引的数据列表
	protected Type mDataType;						// 数据类型
	public ExcelTable()
	{
		mDataList = new Dictionary<int, ExcelData>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDataList.Clear();
		mDataType = null;
	}
	public void setClassType(Type type) { mDataType = type; }
	public void read(string fileName)
	{
		int fileLength = openFile(fileName, out byte[] fileBuffer, true);
		CLASS(out SerializerRead reader);
		reader.init(fileBuffer, fileLength);
		while(true)
		{
			if (reader.getIndex() >= reader.getDataSize())
			{
				break;
			}
			var data = createInstance<ExcelData>(mDataType);
			data.read(reader);
			mDataList.Add(data.mID, data);
		}
		UN_CLASS(reader);
		releaseFile(fileBuffer);
	}
	public T getData<T>(int id, bool errorIfNull = true) where T : ExcelData
	{
		mDataList.TryGetValue(id, out ExcelData data);
		if (data as T == null && errorIfNull)
		{
			logError("在表格中找不到数据: ID:" + id + ", 表格:" + mDataType.ToString());
		}
		return data as T;
	}
	public Dictionary<int, ExcelData> getDataList() { return mDataList; }
}