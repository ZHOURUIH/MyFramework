using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static UnityUtility;
using static FrameDefine;
using static FrameUtility;

// Excel导出数据的管理器
public class ExcelManager : FrameSystem
{
	protected Dictionary<Type, ExcelTable> mTableList = new();  // 表格数据列表,根据表格中数据的类型进行索引,表格数据全部都是延迟加载,只有用到的时候才会加载
	// 资源可访问后开始加载所有的表格文件
	public override void resourceAvailable()
	{
		foreach (ExcelTable item in mTableList.Values)
		{
			item.setResourceAvailable(true);
		}
	}
	public void loadAllAsync(Action callback)
	{
		if (mTableList.Count == 0)
		{
			callback?.Invoke();
			return;
		}
		DateTime startTime = DateTime.Now;
		// 提前加载资源包和其中的子资源
		mResourceManager.preloadAssetBundleAsync(EXCEL, (AssetBundleInfo assetBundle) =>
		{
			assetBundle?.loadAllSubAssets();
			// 然后再加载每个表格
			int tableCount = mTableList.Count;
			int finishCount = 0;
			foreach (ExcelTable item in mTableList.Values)
			{
				item.openFileAsync(() =>
				{
					if (++finishCount == tableCount)
					{
						log("打开所有Excel表格耗时:" + (int)(DateTime.Now - startTime).TotalMilliseconds + "毫秒");
						callback?.Invoke();
					}
				});
			}
		});
	}
	public void reloadAllAsync(Action callback)
	{
		int finishCount = 0;
		int tableCount = mTableList.Count;
		foreach (ExcelTable item in mTableList.Values)
		{
			item.openFileAsync(() =>
			{
				item.reload();
				if (++finishCount == tableCount)
				{
					callback?.Invoke();
				}
			});
		}
	}
	public Dictionary<Type, ExcelTable> getTableList() { return mTableList; }
	public ExcelTable registe(string name, Type tableType, Type dataType)
	{
		var table = mTableList.add(dataType, createInstance<ExcelTable>(tableType));
		table.setDataType(dataType);
		table.setTableName(name);
		return table;
	}
	public void checkAll()
	{
		foreach (ExcelTable item in mTableList.Values)
		{
			item.checkAllData();
		}
	}
}