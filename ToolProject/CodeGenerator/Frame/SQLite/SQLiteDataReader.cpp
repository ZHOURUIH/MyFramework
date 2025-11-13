#include "SQLiteDataReader.h"
#include "StringUtility.h"

SQLiteDataReader::SQLiteDataReader(sqlite3* sqlite, const char* queryStr)
{
	sqlite3_prepare_v2(sqlite, queryStr, -1, &mStmt, nullptr);
}

SQLiteDataReader::~SQLiteDataReader()
{
	if (mStmt != nullptr)
	{
		sqlite3_finalize(mStmt);
		mStmt = nullptr;
	}
}

string SQLiteDataReader::getString(const int col, const bool toANSI) const
{
	if (toANSI)
	{
		return StringUtility::UTF8ToANSI((char*)sqlite3_column_text(mStmt, col));
	}
	else
	{
		return (char*)sqlite3_column_text(mStmt, col);
	}
}

const char* SQLiteDataReader::getBlob(const int col, int& length) const
{
	length = sqlite3_column_bytes(mStmt, col);
	return (const char*)sqlite3_column_blob(mStmt, col);
}