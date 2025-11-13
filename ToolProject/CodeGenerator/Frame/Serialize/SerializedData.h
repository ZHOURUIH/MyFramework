#ifndef _SERIALIZED_DATA_H_
#define _SERIALIZED_DATA_H_

#include "FrameDefine.h"
#include "SystemUtility.h"

struct DataParameter
{
	char* mDataPtr;
	uint mDataSize;		// 数组中最大的数据字节长度
	uint mRealSize;		// 数组中实际有效的数据字节长度
	uint mElementSize;	// 单个元素的字节长度
	uint mDataType;
	const char* mTypeName;
	bool mVariableLength;
	DataParameter()
	{
		mDataPtr = nullptr;
		mTypeName = nullptr;
		mDataSize = 0;
		mRealSize = 0;
		mElementSize = 0;
		mDataType = 0;
		mVariableLength = false;
	}
	DataParameter(char* ptr, uint elementSize, uint dataSize, uint typeHashCode, const char* typeName, bool variableLength)
	{
		mDataPtr = ptr;
		mElementSize = elementSize;
		mDataSize = dataSize;
		mRealSize = dataSize;
		mDataType = typeHashCode;
		mTypeName = typeName;
		mVariableLength = variableLength;
	}
};

class SerializedData : public SystemUtility
{
public:
	SerializedData()
	{
		mMaxDataSize = 0;
	}
	virtual ~SerializedData() { mDataParameterList.clear(); }
	virtual bool readFromBuffer(char* pBuffer, uint bufferSize);
	virtual bool writeToBuffer(char* pBuffer, uint bufferSize);
	virtual bool writeData(const string& dataString, int paramIndex);
	virtual bool writeData(char* buffer, uint bufferSize, int paramIndex);
	string getValueString(uint paramIndex);
	bool readStringList(const myVector<string>& dataList);
	uint getMaxSize() { return mMaxDataSize; }
	uint generateSize();
	uint getElementCount(void* paramPtr);
	void setElementCount(void* paramPtr, uint count);
	// 在子类构造中调用
	virtual void fillParams() = 0;
	void zeroParams();
	template<typename T>
	void pushParam(T& param)
	{
		mDataParameterList.push_back(DataParameter((char*)&param, sizeof(param), sizeof(param), typeid(T).hash_code(), typeid(T).name(), false));
	}
	template<typename T>
	void pushParam(const T* param, uint count, bool variableLength = false)
	{
		if (count > 0)
		{
			mDataParameterList.push_back(DataParameter((char*)param, sizeof(param[0]), sizeof(param[0]) * count, typeid(T*).hash_code(), typeid(T*).name(), variableLength));
		}
	}
	void setArrayByte(char* paramPtr, const char* str, uint length = 0);
	void setArrayByte(byte* paramPtr, const byte* valueList, uint count);
	void setArrayULLong(ullong* paramPtr, const ullong* valueList, uint count);
	void setArrayUShort(ushort* paramPtr, const ushort* valueList, uint count);
	void setArrayUInt(uint* paramPtr, const uint* valueList, uint count);
public:
	myVector<DataParameter> mDataParameterList;
	uint mMaxDataSize;
};

#endif