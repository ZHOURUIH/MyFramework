#pragma once

#include "SQLiteTable.h"
#include "TDCommon.h"

class SQLiteCommon : public SQLiteTable<TDCommon>
{
public:
	void checkAllData() override {}
protected:
};