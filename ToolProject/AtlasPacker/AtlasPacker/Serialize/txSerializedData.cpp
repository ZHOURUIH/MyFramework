#include "txSerializedData.h"
#include "SystemUtility.h"

string txSerializedData::mIntType = typeid(int).name();
string txSerializedData::mShortType = typeid(short).name();
string txSerializedData::mFloatType = typeid(float).name();
string txSerializedData::mCharArrayType = typeid(char*).name();
string txSerializedData::mIntArrayType = typeid(int*).name();

bool txSerializedData::read(char* pBuffer, int bufferSize)
{
	uint bufferOffset = 0;
	bool ret = true;
	int parameterCount = mDataParameterList.size();
	for (int i = 0; i < parameterCount; ++i)
	{
		ret = ret && BinaryUtility::readBuffer(pBuffer, bufferSize, bufferOffset, mDataParameterList[i].mDataPtr, mDataParameterList[i].mDataSize, mDataParameterList[i].mDataSize);
	}
	return ret;
}

bool txSerializedData::write(char* pBuffer, int bufferSize)
{
	uint curWriteSize = 0;
	bool ret = true;
	int parameterCount = mDataParameterList.size();
	for (int i = 0; i < parameterCount; ++i)
	{
		ret = ret && BinaryUtility::writeBuffer(pBuffer, bufferSize, curWriteSize, mDataParameterList[i].mDataPtr, mDataParameterList[i].mDataSize);
	}
	return ret;
}

bool txSerializedData::writeData(const string& dataString, int paramIndex)
{
	if (paramIndex < 0 || paramIndex >= (int)mDataParameterList.size())
	{
		return false;
	}
	const DataParameter& dataParam = mDataParameterList[paramIndex];
	const string& paramType = dataParam.mDataType;
	if (paramType == mIntType)
	{
		*(int*)(dataParam.mDataPtr) = StringUtility::stringToInt(dataString);
	}
	else if (paramType == mShortType)
	{
		*(short*)(dataParam.mDataPtr) = StringUtility::stringToInt(dataString);
	}
	else if (paramType == mFloatType)
	{
		*(float*)(dataParam.mDataPtr) = StringUtility::stringToFloat(dataString);
	}
	else if (paramType == mCharArrayType)
	{
		memset(dataParam.mDataPtr, 0, dataParam.mDataSize);
		int copySize = MathUtility::getMin((int)dataString.length(), dataParam.mDataSize - 1);
		memcpy(dataParam.mDataPtr, dataString.c_str(), copySize);
	}
	else if (paramType == mIntArrayType)
	{
		myVector<string> valueList;
		StringUtility::split(dataString.c_str(), ";", valueList);
		int valueCount = valueList.size();
		for (int i = 0; i < valueCount; ++i)
		{
			((int*)(dataParam.mDataPtr))[i] = StringUtility::stringToInt(valueList[i]);
		}
	}
	return true;
}

bool txSerializedData::writeData(char* buffer, int bufferSize, int paramIndex)
{
	if (buffer == NULL || paramIndex < 0 || paramIndex >= (int)mDataParameterList.size())
	{
		return false;
	}
	if (bufferSize < mDataParameterList[paramIndex].mDataSize)
	{
		return false;
	}
	memcpy(mDataParameterList[paramIndex].mDataPtr, buffer, mDataParameterList[paramIndex].mDataSize);
	return true;
}

string txSerializedData::getValueString(int paramIndex)
{
	const DataParameter& dataParam = mDataParameterList[paramIndex];
	string dataString;
	if (dataParam.mDataType == mIntType)
	{
		dataString = StringUtility::intToString(*((int*)dataParam.mDataPtr));
	}
	else if (dataParam.mDataType == mShortType)
	{
		dataString = StringUtility::intToString((int)*((short*)dataParam.mDataPtr));
	}
	else if (dataParam.mDataType == mFloatType)
	{
		dataString = StringUtility::floatToStringExtra(*((float*)dataParam.mDataPtr), 2);
	}
	else if (dataParam.mDataType == mCharArrayType)
	{
		dataString = dataParam.mDataPtr;
	}
	else if (dataParam.mDataType == mIntArrayType)
	{
		int intCount = dataParam.mDataSize / sizeof(int);
		for (int i = 0; i < intCount; ++i)
		{
			dataString += StringUtility::intToString(*(int*)(dataParam.mDataPtr + i * sizeof(int)));
			if (i + 1 < intCount)
			{
				dataString += ";";
			}
		}
	}
	return dataString;
}

void txSerializedData::zeroParams()
{
	// 数据内容全部清空时,也一起计算数据大小
	mDataSize = 0;
	int parameterCount = mDataParameterList.size();
	for (int i = 0; i < parameterCount; ++i)
	{
		memset(mDataParameterList[i].mDataPtr, 0, mDataParameterList[i].mDataSize);
		mDataSize += mDataParameterList[i].mDataSize;
	}
}

bool txSerializedData::readStringList(const std::vector<string>& dataList)
{
	int curIndex = 0;
	int parameterCount = mDataParameterList.size();
	for (int i = 0; i < parameterCount; ++i)
	{
		if (curIndex >= (int)dataList.size())
		{
			return false;
		}
		const DataParameter& paramter = mDataParameterList[i];
		if (paramter.mDataType == mIntType)
		{
			*(int*)(paramter.mDataPtr) = StringUtility::stringToInt(dataList[curIndex]);
		}
		else if (paramter.mDataType == mShortType)
		{
			*(short*)(paramter.mDataPtr) = (short)StringUtility::stringToInt(dataList[curIndex]);
		}
		else if (paramter.mDataType == mFloatType)
		{
			*(float*)(paramter.mDataPtr) = StringUtility::stringToFloat(dataList[curIndex]);
		}
		else if (paramter.mDataType == mCharArrayType)
		{
			memset(mDataParameterList[i].mDataPtr, 0, mDataParameterList[i].mDataSize);
			int copySize = MathUtility::getMin((int)dataList[curIndex].length(), mDataParameterList[i].mDataSize - 1);
			memcpy(mDataParameterList[i].mDataPtr, dataList[curIndex].c_str(), copySize);
		}
		else if (paramter.mDataType == mIntArrayType)
		{
			myVector<string> breakVec;
			StringUtility::split(dataList[curIndex].c_str(), ";", breakVec);
			int size = breakVec.size();
			for (int j = 0; j < size; ++j)
			{
				((int*)(paramter.mDataPtr))[j] = StringUtility::stringToInt(breakVec[j]);
			}
		}
		++curIndex;
	}
	return true;
}