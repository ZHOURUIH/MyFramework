#pragma once

#include "FrameDefine.h"

class SQLiteTableBase;
// 一个sqlite文件只允许有一个表格,且名字与文件名相同
class SQLiteManager
{
public:
	~SQLiteManager();
	SQLiteTableBase* getSQLite(const string& name) { return mSQLiteList.get(name, nullptr); }
	void addSQLiteTable(SQLiteTableBase* table, const char* tableName);
	void checkAll();
public:
	myMap<string, SQLiteTableBase*> mSQLiteList;
};