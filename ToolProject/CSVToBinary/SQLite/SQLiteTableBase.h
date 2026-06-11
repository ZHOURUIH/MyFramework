#ifndef _SQLITE_TABLE_BASE_H_
#define _SQLITE_TABLE_BASE_H_

#include "SQLiteDataReader.h"
#include "FrameBase.h"

class SQLiteTableBase : public FrameBase
{
	BASE(FrameBase);
public:
	SQLiteTableBase();
	void setTableName(const string& tableName) { mTableName = tableName; }
	const string& getTableName() const { return mTableName; }
	void init(const string& fullPath);
	virtual ~SQLiteTableBase();
public:
	SQLiteDataReader* doSelect(const char* conditionString = nullptr);
	bool doUpdate(const char* updateString, const char* conditionString);
	bool doInsert(const char* valueString);
	bool executeNonQuery(const char* queryString);
	SQLiteDataReader* executeQuery(const char* queryString);
	void releaseReader(SQLiteDataReader*& reader);
	// 因为此处要兼容非常规数据表格类型的数据类型(SelectCount),所以不使用T,而是重新定义一个新的模板类型
	template<typename Table>
	bool parseReader(SQLiteDataReader* reader, Table& data)
	{
		bool result = false;
		if (reader->read())
		{
			data.parse(reader);
			result = true;
		}
		releaseReader(reader);
		return result;
	}
	// 因为此处要兼容非常规数据表格类型的数据类型(SelectCount),所以不使用T,而是重新定义一个新的模板类型
	template<typename Table>
	void parseReader(SQLiteDataReader* reader, Vector<Table*>& dataList)
	{
		if (reader == nullptr)
		{
			return;
		}
		while (reader->read())
		{
			Table* data = NEW(Table, data);
			data->parse(reader);
			dataList.push_back(data);
		}
		releaseReader(reader);
	}
protected:
	string mTableName;
	sqlite3* mSQlite3 = nullptr;
};

#endif