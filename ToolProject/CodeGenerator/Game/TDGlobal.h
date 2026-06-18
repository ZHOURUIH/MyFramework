#pragma once

#include "SQLiteData.h"

class TDGlobal : public SQLiteData
{
public:
	string mParamName;						// 参数名
	string mParamDesc;						// 参数注释
	string mParamType;						// 参数类型
	string mParamValue;						// 参数值
public:
	TDGlobal()
	{
		registeParam(mParamName, 1);
		registeParam(mParamDesc, 2);
		registeParam(mParamType, 3);
		registeParam(mParamValue, 4);
	}
	void clone(SQLiteData* target) override { ERROR("无法克隆"); }
	void checkAllColName(SQLiteTableBase* table) override {}
};