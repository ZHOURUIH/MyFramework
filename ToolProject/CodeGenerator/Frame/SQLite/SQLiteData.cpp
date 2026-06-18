#include "SQLiteData.h"
#include "BinaryUtility.h"
#include "StringUtility.h"

SQLiteData::~SQLiteData()
{
	for (const auto& item : mParameters)
	{
		delete item.second;
	}
	mParameters.clear();
}

void SQLiteData::parse(SQLiteDataReader* reader)
{
	for (const auto& item : mParameters)
	{
		const int index = item.first;
		const SQLiteTableParamBase* param = item.second;
		if (param->mTypeHashCode == BinaryUtility::mIntType)
		{
			*static_cast<int*>(param->mPointer) = reader->getInt(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mUIntType)
		{
			*static_cast<uint*>(param->mPointer) = reader->getInt(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mBoolType)
		{
			*static_cast<bool*>(param->mPointer) = reader->getInt(index) != 0;
		}
		else if (param->mTypeHashCode == BinaryUtility::mCharType)
		{
			*static_cast<char*>(param->mPointer) = reader->getInt(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mByteType)
		{
			*static_cast<byte*>(param->mPointer) = reader->getInt(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mShortType)
		{
			*static_cast<short*>(param->mPointer) = reader->getInt(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mUShortType)
		{
			*static_cast<ushort*>(param->mPointer) = reader->getInt(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mFloatType)
		{
			*static_cast<float*>(param->mPointer) = reader->getFloat(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mLLongType)
		{
			*static_cast<llong*>(param->mPointer) = reader->getLLong(index);
		}
		else if (param->mTypeHashCode == BinaryUtility::mByteListType)
		{
			StringUtility::SToBs(reader->getString(index), *static_cast<myVector<byte>*>(param->mPointer));
		}
		else if (param->mTypeHashCode == BinaryUtility::mUShortListType)
		{
			StringUtility::SToUSs(reader->getString(index), *static_cast<myVector<ushort>*>(param->mPointer));
		}
		else if (param->mTypeHashCode == BinaryUtility::mIntListType)
		{
			StringUtility::SToIs(reader->getString(index), *static_cast<myVector<int>*>(param->mPointer));
		}
		else if (param->mTypeHashCode == BinaryUtility::mUIntListType)
		{
			StringUtility::SToUIs(reader->getString(index), *static_cast<myVector<uint>*>(param->mPointer));
		}
		else if (param->mTypeHashCode == BinaryUtility::mFloatListType)
		{
			StringUtility::SToFs(reader->getString(index), *static_cast<myVector<float>*>(param->mPointer));
		}
		else if (param->mTypeHashCode == BinaryUtility::mVector2IntType)
		{
			*static_cast<Vector2Int*>(param->mPointer) = StringUtility::SToV2I(reader->getString(index));
		}
		else if (param->mTypeHashCode == BinaryUtility::mVector2ShortType)
		{
			*static_cast<Vector2Short*>(param->mPointer) = StringUtility::SToV2S(reader->getString(index));
		}
		else if (param->mTypeHashCode == BinaryUtility::mVector2UShortType)
		{
			*static_cast<Vector2UShort*>(param->mPointer) = StringUtility::SToV2US(reader->getString(index));
		}
		else if (param->mTypeHashCode == BinaryUtility::mStringType)
		{
			*static_cast<string*>(param->mPointer) = reader->getString(index, true);
		}
		else
		{
#ifdef WINDOWS
			ERROR("SQLite参数类型错误:" + param->mTypeName);
#else
			ERROR("SQLite参数类型错误");
#endif
			continue;
		}
	}
}