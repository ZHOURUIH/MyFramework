#pragma once

#include "SQLiteData.h"

class TDDescription : public SQLiteData
{
public:
	string mOwner;
	string mName;
	string mType;
	string mDesc;
public:
	TDDescription()
	{
		registeParam(mOwner, 1);
		registeParam(mName, 2);
		registeParam(mType, 3);
		registeParam(mDesc, 4);
	}
	void clone(SQLiteData* target) override;
	void checkAllColName(SQLiteTableBase* table) override {}
};