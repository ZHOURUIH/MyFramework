using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Excel导出数据的管理器
public class ExcelManager : FrameSystem
{
	protected Dictionary<string, ExcelRegisteInfo> mTableNameList;	// 按表格名字索引的表格注册信息列表
	protected Dictionary<int, ExcelRegisteInfo> mTableTypeList;		// 按表格类型ID索引的表格注册信息列表
	protected Dictionary<Type, ExcelTable> mTableList;				// 表格数据列表,根据表格中数据的类型进行索引
	protected bool mAutoLoad;	// 是否在资源可访问时自动加载所有表格,如果表格资源较少,则可以使用此选项,如果较多,则在同步加载时会有较长时间卡顿,可手动加载
	public ExcelManager()
	{
		mTableNameList = new Dictionary<string, ExcelRegisteInfo>();
		mTableTypeList = new Dictionary<int, ExcelRegisteInfo>();
		mTableList = new Dictionary<Type, ExcelTable>();
		mAutoLoad = true;
	}
	// 资源可访问后开始加载所有的表格文件
	public override void resourceAvailable()
	{
		if (!mAutoLoad)
		{
			return;
		}
		loadAll();
	}
	public void setAutoLoad(bool autoLoad) { mAutoLoad = autoLoad; }
	public void loadAll()
	{
		foreach (var item in mTableNameList)
		{
			ExcelRegisteInfo info = item.Value;
			var table = new ExcelTable();
			table.setClassType(info.mClassType);
			table.read(availablePath(FrameDefine.SA_EXCEL_PATH + item.Key + ".data"));
			mTableList.Add(info.mClassType, table);
		}
	}
	public T getData<T>(int id, bool errorIfNull = true) where T : ExcelData
	{
		if (!mTableList.TryGetValue(typeof(T), out ExcelTable table))
		{
			if (errorIfNull)
			{
				logError("表格未注册:" + typeof(T).ToString());
			}
			return null;
		}
		return table.getData<T>(id, errorIfNull);
	}
	public ExcelData getData(Type type, int id, bool errorIfNull = true)
	{
		if (!mTableList.TryGetValue(type, out ExcelTable table))
		{
			if (errorIfNull)
			{
				logError("表格未注册:" + type.ToString());
			}
			return null;
		}
		return table.getData<ExcelData>(id, errorIfNull);
	}
	public Dictionary<int, ExcelData> getDataList(Type type)
	{
		if (!mTableList.TryGetValue(type, out ExcelTable table))
		{
			logError("表格未注册:" + type.ToString());
			return null;
		}
		return table.getDataList();
	}
	public Dictionary<int, ExcelData> getDataList<T>() where T : ExcelData { return getDataList(typeof(T)); }
	public void registe(int type, string name, Type classType)
	{
		ExcelRegisteInfo info = new ExcelRegisteInfo();
		info.mType = type;
		info.mClassType = classType;
		info.mName = name;
		mTableNameList.Add(name, info);
		mTableTypeList.Add(type, info);
	}
}