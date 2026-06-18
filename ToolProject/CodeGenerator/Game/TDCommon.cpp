#include "TDCommon.h"
#include "StringUtility.h"

void TDCommon::parse(SQLiteDataReader* reader)
{
	const int colCount = reader->getColumnCount();
	FOR_I(colCount)
	{
		const string& value = reader->getString(i);
		const string& name = reader->getColumnName(i);
		mDataList.insert(name, value);
	}
	mID = StringUtility::SToI(mDataList.get("ID", ""));
}