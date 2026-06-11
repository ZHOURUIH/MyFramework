#ifndef _SQLITE_DATA_READER_H_
#define _SQLITE_DATA_READER_H_

#include "FrameBase.h"

class SQLiteDataReader : public FrameBase
{
	BASE(FrameBase);
public:
	explicit SQLiteDataReader(sqlite3_stmt* stmt);	
	virtual ~SQLiteDataReader(); 
public:	
	bool read();// 读取一行数据,需要循环调用来读取多行数据
	void close();
	int getColumnCount();
	const char* getColumnName(int col);
	SQLITE_DATATYPE getDataType(int col);
	void getString(int col, string& str, bool toANSI = true);
	int getInt(int col);
	llong getLLong(int col);
	float getFloat(int col);
	const char* getBlob(int col, int& length);
protected:
	sqlite3_stmt* mStmt;
};

#endif