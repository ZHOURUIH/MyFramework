#pragma once

#include "FrameDefine.h"

class SQLiteDataReader
{
public:
	SQLiteDataReader(sqlite3* sqlite, const char* queryStr);
	virtual ~SQLiteDataReader();
public:	
	void startQuery(sqlite3* sqlite, const char* queryStr);
	void setSQLite3STMT(sqlite3_stmt* stmt) { mStmt = stmt; }
	// 读取一行数据,需要循环调用来读取多行数据
	bool read() const { return mStmt != nullptr && sqlite3_step(mStmt) == SQLITE_ROW; }
	int getColumnCount() const { return sqlite3_column_count(mStmt); }
	const char* getColumnName(int col) const { return sqlite3_column_name(mStmt, col); }
	SQLITE_DATATYPE getDataType(int col) const { return (SQLITE_DATATYPE)sqlite3_column_type(mStmt, col); }
	string getString(int col, bool toANSI = true) const;
	int getInt(int col) const { return sqlite3_column_int(mStmt, col); }
	llong getLLong(int col) const { return sqlite3_column_int64(mStmt, col); }
	float getFloat(int col) const { return (float)sqlite3_column_double(mStmt, col); }
	const char* getBlob(int col, int& length) const;
	template<typename Table>
	bool parseReader(Table& data)
	{
		const bool result = read();
		if (result)
		{
			data.parse(this);
		}
		return result;
	}
	template<typename Table>
	void parseReader(myVector<Table*>& dataList)
	{
		while (read())
		{
			Table* data = new Table();
			data->parse(this);
			dataList.push_back(data);
		}
	}
protected:
	sqlite3_stmt* mStmt = nullptr;
};