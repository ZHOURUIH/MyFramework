using System;
using System.Collections.Generic;
using static GBR;

public class ExcelTest : ExcelTableT<EDTest>
{
	// auto generate start
	protected override void checkAllDataDefault()
	{
		foreach (EDTest item in queryAll())
		{
			checkEnum(item.mTestEnum, "mTestEnum", item.mID);
			mExcelAchivement.checkData(item.mTestLinkTable0, item.mID, this);
			mExcelAchivement.checkData(item.mTestLinkTable1, item.mID, this);
			if (!item.mTestPath.isEmpty())
			{
				checkPath(item.mTestPath);
			}
			checkListPair(item.mTestList0, item.mTestList1, item.mID);
		}
	}
	// auto generate end
}