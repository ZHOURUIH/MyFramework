#pragma once

#include "FrameDefine.h"
#include "SQLiteDataReader.h"
#include "SQLiteTableParam.h"

class SQLiteTableBase;
class SQLiteData
{
public:
	SQLiteData()
	{
		registeParam(mID, 0);
	}
	virtual ~SQLiteData();
	virtual void checkAllColName(SQLiteTableBase* table) = 0;
	void parse(SQLiteDataReader* reader);
	template<typename T>
	void registeParam(T& param, const byte index)
	{
		mParameters.insert(index, new SQLiteTableParam<T>(&param));
	}
	template<typename RealType, typename T>
	void registeEnumParam(T& param, const byte index)
	{
		mParameters.insert(index, new SQLiteTableParam<RealType>(&param));
	}
	template<typename RealType, typename T>
	void registeEnumListParam(T& param, const byte index)
	{
		mParameters.insert(index, new SQLiteTableParam<Vector<RealType>>(&param));
	}
	virtual void clone(SQLiteData* target) { target->mID = mID; }
public:
	int mID;					// 唯一ID,由于列名是定义在基类中,所以为了对应关系,将ID变量也定义在基类中
protected:
	myMap<byte, SQLiteTableParamBase*> mParameters;
};