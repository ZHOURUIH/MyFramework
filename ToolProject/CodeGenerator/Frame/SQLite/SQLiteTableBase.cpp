#include "SQLiteTableBase.h"
#include "Array.h"
#include "StringUtility.h"

void SQLiteTableBase::init(const string& fullPath)
{
	const int ret = sqlite3_open(fullPath.c_str(), &mSQLite3);
	if (ret != SQLITE_OK)
	{
		ERROR("can not open sqlite, info : " + string(sqlite3_errmsg(mSQLite3)));
	}
}

SQLiteTableBase::~SQLiteTableBase()
{
	if (mSQLite3 != nullptr)
	{
		sqlite3_close(mSQLite3);
		mSQLite3 = nullptr;
	}
}

bool SQLiteTableBase::doUpdate(const char* updateString, const char* conditionString) const
{
	Array<1024> queryStr{ 0 };
	StringUtility::strcat_t(queryStr, "UPDATE ", mTableName, " SET ", updateString, " WHERE ", conditionString);
	return executeNonQuery(queryStr.str());
}

bool SQLiteTableBase::doInsert(const char* valueString) const
{
	Array<1024> queryStr{ 0 };
	StringUtility::strcat_t(queryStr, "INSERT INTO ", mTableName, " VALUES (", valueString, ")");
	return executeNonQuery(queryStr.str());
}

bool SQLiteTableBase::executeNonQuery(const char* queryString) const
{
	sqlite3_stmt* stmt = nullptr;
	if (sqlite3_prepare_v2(mSQLite3, queryString, -1, &stmt, nullptr) != SQLITE_OK)
	{
		return false;
	}
	sqlite3_step(stmt);
	return sqlite3_finalize(stmt) == SQLITE_OK;
}

void SQLiteTableBase::checkColName(const string& colName, const int index)
{
	// 检查列名是否与变量名一致
	SQLiteDataReader reader(mSQLite3, ("select * from " + string(mTableName)).c_str());
	const char* name = reader.getColumnName(index);
	if (name != colName)
	{
		ERROR(string("表格字段名与定义的变量名不一致,表格:") + mTableName + ", 表格字段名:" + name + ", 变量名:" + colName);
	}
}