#ifndef _SQLITE_TABLE_PARAM_H_
#define _SQLITE_TABLE_PARAM_H_

#include "FrameDefine.h"

class SQLiteTableParam
{
public:
	SQLiteTableParam(void* pointer, const char* col, uint typeHashCode, const char* typeName):
		mPointer(pointer),
		mCol(col),
		mTypeHashCode(typeHashCode),
		mTypeName(typeName)
	{}
public:
	void* mPointer;
	const char* mCol;
	uint mTypeHashCode;
	const char* mTypeName;
};

#endif