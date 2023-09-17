using System;
using UnityEngine;
using System.Collections.Generic;

// Excel表格的基类,表示一个表格
public class ExcelTableTemplate<T> : ExcelTable where T : ExcelData
{
	// 由于基类无法知道子类的具体类型,所以将List类型的列表定义到子类中.因为大部分时候外部使用的都是List类型的列表
	// 并且ILRuntime热更对于模板支持不太好,所以尽量避免使用模板
	// 此处定义一个List是为了方便外部可直接获取,避免每次queryAll时都会创建列表
	protected List<T> mDataList;
	protected bool mDataAvailable;
	public ExcelTableTemplate()
	{
		mDataList = new List<T>();
		mDataAvailable = false;
	}
	public T query(int id, bool errorIfNull = true)
	{
		return getData<T>(id, errorIfNull);
	}
	public List<T> queryAll()
	{
		if (!mDataAvailable)
		{
			foreach (var item in getDataList())
			{
				mDataList.Add(item.Value as T);
			}
			mDataAvailable = true;
		}
		return mDataList;
	}
}