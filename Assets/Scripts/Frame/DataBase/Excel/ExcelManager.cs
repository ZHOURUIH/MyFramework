using System;
using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;

// Excel导出数据的管理器
public class ExcelManager : FrameSystem
{
	protected Dictionary<Type, ExcelTable> mTableList;	// 表格数据列表,根据表格中数据的类型进行索引,表格数据全部都是延迟加载,只有用到的时候才会加载
	public ExcelManager()
	{
		mTableList = new Dictionary<Type, ExcelTable>();
	}
	// 资源可访问后开始加载所有的表格文件
	public override void resourceAvailable()
	{
		foreach (var item in mTableList)
		{
			item.Value.setResourceAvailable(true);
		}
	}
	public T getData<T>(int id, bool errorIfNull = true) where T : ExcelData
	{
		if (!mTableList.TryGetValue(typeof(T), out ExcelTable table))
		{
			if (errorIfNull)
			{
				logError("表格未注册:" + typeof(T));
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
				logError("表格未注册:" + type);
			}
			return null;
		}
		return table.getData<ExcelData>(id, errorIfNull);
	}
	public Dictionary<int, ExcelData> getDataList(Type type)
	{
		if (!mTableList.TryGetValue(type, out ExcelTable table))
		{
			logError("表格未注册:" + type);
			return null;
		}
		return table.getDataList();
	}
	public Dictionary<Type, ExcelTable> getTableList() { return mTableList; }
	public Dictionary<int, ExcelData> getDataList<T>() where T : ExcelData { return getDataList(typeof(T)); }
	public ExcelTable registe(string name, Type tableType, Type dataType)
	{
		var table = createInstance<ExcelTable>(tableType);
		table.setClassType(dataType);
		table.setTableName(name);
		mTableList.Add(dataType, table);
		return table;
	}
}