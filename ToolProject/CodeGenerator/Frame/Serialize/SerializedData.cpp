#include "SerializedData.h"
#include "Utility.h"

bool SerializedData::readFromBuffer(char* pBuffer, uint bufferSize)
{
	uint bufferOffset = 0;
	bool ret = true;
	FOR_VECTOR(mDataParameterList)
	{
		auto& param = mDataParameterList[i];
		// 变长先读取数组数据实际字节数
		if (param.mVariableLength)
		{
			param.mRealSize = read<ushort>(pBuffer, bufferSize, bufferOffset);
		}
		else
		{
			param.mRealSize = param.mDataSize;
		}
		// 避免数据越界
		if (param.mRealSize > param.mDataSize)
		{
			ret = false;
			break;
		}
		// 读取实际的数组数据
		ret = ret && readBuffer(pBuffer, bufferSize, bufferOffset, param.mDataPtr, param.mRealSize);
	}
	return ret;
}

bool SerializedData::writeToBuffer(char* pBuffer, uint bufferSize)
{
	uint curWriteSize = 0;
	bool ret = true;
	FOR_VECTOR(mDataParameterList)
	{
		auto& param = mDataParameterList[i];
		// 变长数组先写入数组数据字节数
		if (param.mVariableLength)
		{
			ret = ret && write<ushort>(pBuffer, bufferSize, curWriteSize, param.mRealSize);
		}
		ret = ret && writeBuffer(pBuffer, bufferSize, curWriteSize, param.mDataPtr, param.mRealSize);
	}
	return ret;
}

bool SerializedData::writeData(const string& dataString, int paramIndex)
{
	if (paramIndex >= mDataParameterList.size())
	{
		return false;
	}
	const DataParameter& dataParam = mDataParameterList[paramIndex];
	uint paramType = dataParam.mDataType;
	if (paramType == mCharType)
	{
		dataParam.mDataPtr[0] = stringToInt(dataString);
	}
	else if (paramType == mByteType)
	{
		*(byte*)(dataParam.mDataPtr) = stringToInt(dataString);
	}
	else if (paramType == mIntType)
	{
		*(int*)(dataParam.mDataPtr) = stringToInt(dataString);
	}
	else if (paramType == mUIntType)
	{
		*(uint*)(dataParam.mDataPtr) = stringToInt(dataString);
	}
	else if (paramType == mShortType)
	{
		*(short*)(dataParam.mDataPtr) = stringToInt(dataString);
	}
	else if (paramType == mUShortType)
	{
		*(ushort*)(dataParam.mDataPtr) = stringToInt(dataString);
	}
	else if (paramType == mFloatType)
	{
		*(float*)(dataParam.mDataPtr) = stringToFloat(dataString);
	}
	else if (paramType == mCharArrayType)
	{
		memset(dataParam.mDataPtr, 0, dataParam.mDataSize);
		int copySize = getMin((int)dataString.length(), (int)dataParam.mDataSize - 1);
		memcpy(dataParam.mDataPtr, dataString.c_str(), copySize);
	}
	else if (paramType == mByteArrayType)
	{
		myVector<int> valueList;
		stringToIntArray(dataString, valueList, ";");
		FOR_VECTOR(valueList)
		{
			((byte*)(dataParam.mDataPtr))[i] = valueList[i];
		}
	}
	else if (paramType == mIntArrayType)
	{
		myVector<int> valueList;
		stringToIntArray(dataString, valueList, ";");
		FOR_VECTOR(valueList)
		{
			((int*)(dataParam.mDataPtr))[i] = valueList[i];
		}
	}
	else if (paramType == mUIntArrayType)
	{
		myVector<int> valueList;
		stringToIntArray(dataString, valueList, ";");
		FOR_VECTOR(valueList)
		{
			((uint*)(dataParam.mDataPtr))[i] = valueList[i];
		}
	}
	else if (paramType == mShortArrayType)
	{
		myVector<int> valueList;
		stringToIntArray(dataString, valueList, ";");
		FOR_VECTOR(valueList)
		{
			((short*)(dataParam.mDataPtr))[i] = valueList[i];
		}
	}
	else if (paramType == mUShortArrayType)
	{
		myVector<int> valueList;
		stringToIntArray(dataString, valueList, ";");
		FOR_VECTOR(valueList)
		{
			((ushort*)(dataParam.mDataPtr))[i] = valueList[i];
		}
	}
	else if (paramType == mFloatArrayType)
	{
		myVector<float> valueList;
		stringToFloatArray(dataString, valueList, ";");
		FOR_VECTOR(valueList)
		{
			((float*)(dataParam.mDataPtr))[i] = valueList[i];
		}
	}
	else
	{
		ERROR("error param type:" + string(dataParam.mTypeName));
	}
	return true;
}

bool SerializedData::writeData(char* buffer, uint bufferSize, int paramIndex)
{
	if (buffer == NULL || paramIndex >= mDataParameterList.size())
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

string SerializedData::getValueString(uint paramIndex)
{
	const DataParameter& dataParam = mDataParameterList[paramIndex];
	string dataString;
	uint dataType = dataParam.mDataType;
	if (dataType == mCharType)
	{
		dataString = intToString(dataParam.mDataPtr[0]);
	}
	else if (dataType == mByteType)
	{
		dataString = intToString(*((byte*)dataParam.mDataPtr));
	}
	else if (dataType == mIntType)
	{
		dataString = intToString(*((int*)dataParam.mDataPtr));
	}
	else if (dataType == mUIntType)
	{
		dataString = intToString(*((uint*)dataParam.mDataPtr));
	}
	else if (dataType == mShortType)
	{
		dataString = intToString(*((short*)dataParam.mDataPtr));
	}
	else if (dataType == mUShortType)
	{
		dataString = intToString(*((ushort*)dataParam.mDataPtr));
	}
	else if (dataType == mFloatType)
	{
		dataString = floatToString(*((float*)dataParam.mDataPtr));
	}
	else if (dataType == mCharArrayType)
	{
		dataString = dataParam.mDataPtr;
	}
	else if (dataType == mByteArrayType)
	{
		const int count = (int)dataParam.mDataSize / sizeof(byte);
		FOR_I(count)
		{
			dataString += intToString(*(byte*)(dataParam.mDataPtr + i * sizeof(byte)));
			if (i + 1 < count)
			{
				dataString += ";";
			}
		}
	}
	else if (dataType == mIntArrayType)
	{
		dataString += intArrayToString((int*)dataParam.mDataPtr, dataParam.mDataSize / sizeof(int), 0, ";");
	}
	else if (dataType == mUIntArrayType)
	{
		const int count = (int)dataParam.mDataSize / sizeof(uint);
		FOR_I(count)
		{
			dataString += intToString(*(uint*)(dataParam.mDataPtr + i * sizeof(uint)));
			if (i + 1 < count)
			{
				dataString += ";";
			}
		}
	}
	else if (dataType == mShortArrayType)
	{
		const int count = (int)dataParam.mDataSize / sizeof(short);
		FOR_I(count)
		{
			dataString += intToString(*(short*)(dataParam.mDataPtr + i * sizeof(short)));
			if (i + 1 < count)
			{
				dataString += ";";
			}
		}
	}
	else if (dataType == mUShortArrayType)
	{
		const int count = (int)dataParam.mDataSize / sizeof(ushort);
		FOR_I(count)
		{
			dataString += intToString(*(ushort*)(dataParam.mDataPtr + i * sizeof(ushort)));
			if (i + 1 < count)
			{
				dataString += ";";
			}
		}
	}
	else if (dataType == mFloatArrayType)
	{
		dataString += floatArrayToString((float*)dataParam.mDataPtr, dataParam.mDataSize / sizeof(float), ";");
	}
	else
	{
		ERROR("error param type:" + string(dataParam.mTypeName));
	}
	return dataString;
}

void SerializedData::zeroParams()
{
	// 数据内容全部清空时,也一起计算数据大小
	mMaxDataSize = 0;
	FOR_VECTOR(mDataParameterList)
	{
		auto& elem = mDataParameterList[i];
		memset(elem.mDataPtr, 0, elem.mDataSize);
		elem.mRealSize = elem.mDataSize;
		mMaxDataSize += elem.mDataSize;
	}
}

bool SerializedData::readStringList(const myVector<string>& dataList)
{
	bool result = true;
	int curIndex = 0;
	FOR_VECTOR(mDataParameterList)
	{
		if (curIndex >= dataList.size())
		{
			result = false;
			break;
		}
		const DataParameter& paramter = mDataParameterList[i];
		uint dataType = paramter.mDataType;
		if (dataType == mCharType)
		{
			paramter.mDataPtr[0] = stringToInt(dataList[curIndex]);
		}
		else if (dataType == mByteType)
		{
			*(byte*)(paramter.mDataPtr) = stringToInt(dataList[curIndex]);
		}
		else if (dataType == mIntType)
		{
			*(int*)(paramter.mDataPtr) = stringToInt(dataList[curIndex]);
		}
		else if (dataType == mShortType)
		{
			*(short*)(paramter.mDataPtr) = (short)stringToInt(dataList[curIndex]);
		}
		else if (dataType == mFloatType)
		{
			*(float*)(paramter.mDataPtr) = stringToFloat(dataList[curIndex]);
		}
		else if (dataType == mCharArrayType)
		{
			memset(mDataParameterList[i].mDataPtr, 0, mDataParameterList[i].mDataSize);
			int copySize = getMin((int)dataList[curIndex].length(), (int)mDataParameterList[i].mDataSize - 1);
			memcpy(mDataParameterList[i].mDataPtr, dataList[curIndex].c_str(), copySize);
		}
		else if (dataType == mIntArrayType)
		{
			myVector<string> breakVec;
			split(dataList[curIndex].c_str(), ";", breakVec);
			uint size = breakVec.size();
			FOR_J(size)
			{
				((int*)(paramter.mDataPtr))[j] = stringToInt(breakVec[j]);
			}
		}
		else
		{
			ERROR("error param type:" + string(paramter.mTypeName));
		}
		++curIndex;
	}
	return result;
}

uint SerializedData::generateSize()
{
	uint size = 0;
	FOR_VECTOR(mDataParameterList)
	{
		if (mDataParameterList[i].mVariableLength)
		{
			size += sizeof(ushort);
		}
		size += mDataParameterList[i].mRealSize;
	}
	return size;
}

uint SerializedData::getElementCount(void* paramPtr)
{
	uint count = 0;
	FOR_VECTOR(mDataParameterList)
	{
		auto& elem = mDataParameterList[i];
		if (elem.mDataPtr == paramPtr)
		{
			count = elem.mRealSize / elem.mElementSize;
			break;
		}
	}
	return count;
}

void SerializedData::setElementCount(void* paramPtr, uint count)
{
	FOR_VECTOR(mDataParameterList)
	{
		auto& elem = mDataParameterList[i];
		if (elem.mDataPtr == paramPtr)
		{
			if (!elem.mVariableLength)
			{
				ERROR("mVariableLength需要为true才能设置变长长度");
				break;
			}
			if (elem.mElementSize * count > elem.mDataSize)
			{
				ERROR("变长数据长度不能超过最大长度");
				break;
			}
			elem.mRealSize = elem.mElementSize * count;
			break;
		}
	}
}

void SerializedData::setArrayByte(char* paramPtr, const char* str, uint length)
{
	setString(paramPtr, str);
	setElementCount(paramPtr, length > 0 ? length : (uint)strlen(str));
}

void SerializedData::setArrayByte(byte* paramPtr, const byte* valueList, uint count)
{
	FOR_I(count)
	{
		paramPtr[i] = valueList[i];
	}
	setElementCount(paramPtr, count);
}

void SerializedData::setArrayULLong(ullong* paramPtr, const ullong* valueList, uint count)
{
	FOR_I(count)
	{
		paramPtr[i] = valueList[i];
	}
	setElementCount(paramPtr, count);
}

void SerializedData::setArrayUShort(ushort* paramPtr, const ushort* valueList, uint count)
{
	FOR_I(count)
	{
		paramPtr[i] = valueList[i];
	}
	setElementCount(paramPtr, count);
}

void SerializedData::setArrayUInt(uint* paramPtr, const uint* valueList, uint count)
{
	FOR_I(count)
	{
		paramPtr[i] = valueList[i];
	}
	setElementCount(paramPtr, count);
}