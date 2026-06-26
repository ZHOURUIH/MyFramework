// auto generate start
using System;
using System.Collections.Generic;
using UnityEngine;

// 全局变量表格
public class EDGlobal : ExcelData
{
	public string mParamName;						// 参数名
	public string mParamType;						// 参数类型
	public string mParamValue;						// 参数值
	public override bool read(SerializerRead reader)
	{
		bool result = base.read(reader);
		result = result && reader.readString(out mParamName);
		result = result && reader.readString(out mParamType);
		result = result && reader.readString(out mParamValue);
		return result;
	}
	public static void postLoadAll(ExcelTableT<EDGlobal> table){}
}
// auto generate end