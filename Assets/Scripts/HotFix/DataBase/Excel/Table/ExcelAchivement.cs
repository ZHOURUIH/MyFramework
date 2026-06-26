using System;
using System.Collections.Generic;
using static GBR;

public class ExcelAchivement : ExcelTableT<EDAchivement>
{
	// auto generate start
	protected override void checkAllDataDefault() {}
	protected override void postParseFile()
	{
		EDAchivement.postLoadAll(this);
	}
	// auto generate end
}