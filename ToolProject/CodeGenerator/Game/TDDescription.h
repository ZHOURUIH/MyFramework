#pragma once

#include "SQLiteData.h"

class TDDescription : public SQLiteData
{
public:
	string mOwner;
	string mName;
	string mType;
	string mDesc;
	string mLinkTable;
public:
	TDDescription()
	{
		registeParam(mOwner, 1);
		registeParam(mName, 2);
		registeParam(mType, 3);
		registeParam(mDesc, 4);
		registeParam(mLinkTable, 5);
	}
	void clone(SQLiteData* target) override { ERROR("无法克隆"); }
	void checkAllColName(SQLiteTableBase* table) override {}
};