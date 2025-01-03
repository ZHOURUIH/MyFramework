using System.Collections.Generic;

public class ExcelTableT<T> : ExcelTable where T : ExcelData
{
	protected List<T> mDataList = new();
	protected bool mDataAvailable;
	public T query(int id, bool errorIfNull = true)
	{
		return getData<T>(id, errorIfNull);
	}
	public List<T> queryAll()
	{
		if (!mDataAvailable)
		{
			initDataList();
		}
		return mDataList;
	}
	public override void clear()
	{
		mDataAvailable = false;
		mDataList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void initDataList()
	{
		foreach (ExcelData item in getDataMap().Values)
		{
			mDataList.Add(item as T);
		}
		mDataAvailable = true;
	}
}