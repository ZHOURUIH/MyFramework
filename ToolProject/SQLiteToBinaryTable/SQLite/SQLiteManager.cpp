#include "FrameHeader.h"

SQLiteManager::~SQLiteManager()
{
	FOREACH(iter, mSQliteList)
	{
		DELETE(iter->second);
	}
	END(mSQliteList);
	mSQliteList.clear();
}

void SQLiteManager::addSQLiteTable(SQLiteTableBase* table, const char* tableName, const char* fullPath)
{
	table->setTableName(tableName);
	table->init(fullPath);
	mSQliteList.insert(table->getTableName(), table);
}