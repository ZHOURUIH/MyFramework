#pragma once

#include "FrameDefine.h"

class SQLiteTableParamBase
{
public:
	SQLiteTableParamBase(void* pointer, int typeHashCode) :
		mPointer(pointer),
		mTypeHashCode(typeHashCode) {}
public:
	void* mPointer = nullptr;
	int mTypeHashCode = 0;
#ifdef WINDOWS
	string mTypeName;
#endif
};

template<typename T>
class SQLiteTableParam : public SQLiteTableParamBase
{
public:
	SQLiteTableParam(void* pointer):
		SQLiteTableParamBase(pointer, (int)typeid(T).hash_code())
	{
#ifdef WINDOWS
		mTypeName = typeid(T).name();
#endif
	}
public:
};