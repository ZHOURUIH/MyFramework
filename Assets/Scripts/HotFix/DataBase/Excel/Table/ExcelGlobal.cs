using UnityEngine;
using System.Collections.Generic;

public class ExcelGlobal : ExcelTableT<EDGlobal>
{
    // auto generate start
	protected override void checkAllDataDefault() {}
	protected override void postParseFile()
	{
		EDGlobal.postLoadAll(this);
	}
    // auto generate end
}