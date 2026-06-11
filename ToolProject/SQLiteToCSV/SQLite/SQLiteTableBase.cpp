#include "FrameHeader.h"

SQLiteTableBase::SQLiteTableBase()
{
	;
}

void SQLiteTableBase::init(const string& fullPath)
{
	int ret = sqlite3_open(fullPath.c_str(), &mSQlite3);
	if (ret != SQLITE_OK)
	{
		ERROR("can not open sqlite, info : " + string(sqlite3_errmsg(mSQlite3)));
	}
}
SQLiteTableBase::~SQLiteTableBase()
{
	if (mSQlite3 != nullptr)
	{
		sqlite3_close(mSQlite3);
		mSQlite3 = nullptr;
	}
}

SQLiteDataReader* SQLiteTableBase::doSelect(const char* conditionString)
{
	Array<256> queryStr{ 0 };
	if (conditionString != nullptr)
	{
		strcat_t(queryStr, "SELECT * FROM ", mTableName.c_str(), " WHERE ", conditionString);
	}
	else
	{
		strcat_t(queryStr, "SELECT * FROM ", mTableName.c_str());
	}
	return executeQuery(queryStr.toString());
}

bool SQLiteTableBase::doUpdate(const char* updateString, const char* conditionString)
{
	Array<1024> queryStr{ 0 };
	strcat_t(queryStr, "UPDATE ", mTableName.c_str(), " SET ", updateString, " WHERE ", conditionString);
	return executeNonQuery(queryStr.toString());
}
bool SQLiteTableBase::doInsert(const char* valueString)
{
	Array<1024> queryStr{ 0 };
	strcat_t(queryStr, "INSERT INTO ", mTableName.c_str(), " VALUES (", valueString, ")");
	return executeNonQuery(queryStr.toString());
}
bool SQLiteTableBase::executeNonQuery(const char* queryString)
{
	sqlite3_stmt* stmt = nullptr;
	if (sqlite3_prepare_v2(mSQlite3, queryString, -1, &stmt, nullptr) != SQLITE_OK)
	{
		return false;
	}
	sqlite3_step(stmt);
	return sqlite3_finalize(stmt) == SQLITE_OK;
}
SQLiteDataReader* SQLiteTableBase::executeQuery(const char* queryString)
{
	sqlite3_stmt* stmt = nullptr;
	if (sqlite3_prepare_v2(mSQlite3, queryString, -1, &stmt, nullptr) != SQLITE_OK)
	{
		return nullptr;
	}
	SQLiteDataReader* reader = NEW(SQLiteDataReader, reader, stmt);
	return reader;
}
void SQLiteTableBase::releaseReader(SQLiteDataReader*& reader)
{
	DELETE(reader);
}