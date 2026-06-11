#include "FrameHeader.h"

SQLiteDataReader::SQLiteDataReader(sqlite3_stmt* stmt):
	mStmt(stmt)
{}

SQLiteDataReader::~SQLiteDataReader()
{
	close();
}

bool SQLiteDataReader::read()
{
	if (mStmt == nullptr)
	{
		return false;
	}
	if (sqlite3_step(mStmt) != SQLITE_ROW)
	{
		return false;
	}
	return true;
}

void SQLiteDataReader::close()
{
	if (mStmt != nullptr)
	{
		sqlite3_finalize(mStmt);
		mStmt = nullptr;
	}
}
int SQLiteDataReader::getColumnCount()
{
	return sqlite3_column_count(mStmt);
}

const char* SQLiteDataReader::getColumnName(int col)
{
	return sqlite3_column_name(mStmt, col);
}

SQLITE_DATATYPE SQLiteDataReader::getDataType(int col)
{
	return (SQLITE_DATATYPE)sqlite3_column_type(mStmt, col);
}

void SQLiteDataReader::getString(int col, string& str, bool toANSI)
{
	if (toANSI)
	{
		UTF8ToANSI((char*)sqlite3_column_text(mStmt, col), str);
	}
	else
	{
		str = (char*)sqlite3_column_text(mStmt, col);
	}
}

int SQLiteDataReader::getInt(int col)
{
	return sqlite3_column_int(mStmt, col);
}

llong SQLiteDataReader::getLLong(int col)
{
	return sqlite3_column_int64(mStmt, col);
}

float SQLiteDataReader::getFloat(int col)
{
	return (float)sqlite3_column_double(mStmt, col);
}

const char* SQLiteDataReader::getBlob(int col, int& length)
{
	length = sqlite3_column_bytes(mStmt, col);
	return (const char*)sqlite3_column_blob(mStmt, col);
}