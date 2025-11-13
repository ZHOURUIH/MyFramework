#pragma once

#include "SQLiteData.h"

class TDGlobal : public SQLiteData
{
public:
	string mParamName;								// 参数名
	string mParamDesc;								// 参数注释
	float mParamValue = 0.0f;						// 参数值
	int mParamValueInt = 0;							// 参数值,整数
public:
	TDGlobal()
	{
		registeParam(mParamName, 1);
		registeParam(mParamDesc, 2);
		registeParam(mParamValue, 3);
		registeParam(mParamValueInt, 4);
	}
	void clone(SQLiteData* target) override;
	void checkAllColName(SQLiteTableBase* table) override {}
};