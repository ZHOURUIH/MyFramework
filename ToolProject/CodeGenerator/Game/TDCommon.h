#pragma once

#include "SQLiteData.h"

class TDCommon : public SQLiteData
{
public:
	myMap<string, string> mDataList;		// 根据字段名获取数据内容
public:
	void clone(SQLiteData* target) override { ERROR("无法克隆"); }
	void checkAllColName(SQLiteTableBase* table) override {}
	void parse(SQLiteDataReader* reader) override;
	const string& getData(const string& name) { mDataList.get(name, ""); }
	const myMap<string, string>& getDataList() const { return mDataList; }
};