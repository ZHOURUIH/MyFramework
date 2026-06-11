#ifndef _SELECT_COUNT_H_
#define _SELECT_COUNT_H_

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

#endif