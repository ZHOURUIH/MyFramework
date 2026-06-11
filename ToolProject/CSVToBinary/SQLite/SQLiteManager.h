#ifndef _SQLITE_MANAGER_H_
#define _SQLITE_MANAGER_H_

#include "FrameBase.h"

class SQLiteTableBase;
// 一个sqlite文件只允许有一个表格,且名字与文件名相同
class SQLiteManager : public FrameBase
{
public:
	~SQLiteManager();
	SQLiteTableBase* getSQLite(const string& name) { return mSQliteList.get(name, nullptr); }
	void addSQLiteTable(SQLiteTableBase* table, const char* tableName, const char* fullPath);
public:
	Map<string, SQLiteTableBase*> mSQliteList;
};

#endif