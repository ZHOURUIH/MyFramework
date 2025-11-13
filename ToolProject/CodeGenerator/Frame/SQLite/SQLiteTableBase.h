#pragma once

#include "SQLiteDataReader.h"
#include "FrameDefine.h"

class SQLiteTableBase
{
public:
	void setTableName(const char* tableName) { mTableName = tableName; }
	const char* getTableName() const { return mTableName; }
	sqlite3* getSQLite3() const { return mSQLite3; }
	void init(const string& fullPath);
	virtual ~SQLiteTableBase();
	virtual void checkAllData() {}
	void checkColName(const string& colName, int index);
	virtual void checkDataAllColName() = 0;
protected:
	bool doUpdate(const char* updateString, const char* conditionString) const;
	bool doInsert(const char* valueString) const;
	bool executeNonQuery(const char* queryString) const;
protected:
	const char* mTableName = nullptr;
	sqlite3* mSQLite3 = nullptr;
};