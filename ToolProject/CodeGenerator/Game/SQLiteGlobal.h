#pragma once

#include "SQLiteTable.h"
#include "TDGlobal.h"

class SQLiteGlobal : public SQLiteTable<TDGlobal>
{
public:
	void checkAllData() override {}
protected:
};