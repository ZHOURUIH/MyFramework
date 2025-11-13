#pragma once

#include "SQLiteTable.h"
#include "TDDescription.h"

class SQLiteDescription : public SQLiteTable<TDDescription>
{
public:
	void checkAllData() override {}
protected:
};