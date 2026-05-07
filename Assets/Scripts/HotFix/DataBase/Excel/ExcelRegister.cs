// auto generate start
using System;
using static GBR;
using static FrameBaseHotFix;

public class ExcelRegister
{
	public static void registeAll()
	{

		// 进入热更以后,所有资源都处于可用状态
		mExcelManager.resourceAvailable();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void registeTable<T>(out T table, Type dataType, string tableName) where T : ExcelTable
	{
		table = mExcelManager.registe(tableName, typeof(T), dataType) as T;
	}
}
// auto generate end