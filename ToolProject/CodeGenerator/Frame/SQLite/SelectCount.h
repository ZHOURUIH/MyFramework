#pragma once

#include "SQLiteDataReader.h"

class SelectCount
{
public:
	int mRowCount = 0;
public:
	void parse(SQLiteDataReader* reader)
	{
		mRowCount = reader->getInt(0);
	}
};