#include "Serializer.h"
#include "Utility.h"

Serializer::Serializer(bool traceMemory)
:
mBuffer(NULL),
mIndex(0),
mBufferSize(0),
mWriteFlag(true),
mTraceMemory(traceMemory)
{}

Serializer::Serializer(char* buffer, uint bufferSize)
:
mBuffer(buffer),
mIndex(0),
mBufferSize(bufferSize),
mWriteFlag(false),
mTraceMemory(true)
{}

Serializer::~Serializer()
{
	if (mWriteFlag)
	{
		if (mTraceMemory)
		{
			ArrayPool::deleteArray(mBuffer);
		}
		else
		{
			NORMAL_DELETE_ARRAY(mBuffer);
		}
	}
}

void Serializer::writeBuffer(char* buffer, uint bufferSize)
{
	if (!writeCheck(bufferSize))
	{
		return;
	}
	BinaryUtility::writeBuffer(mBuffer, mBufferSize, mIndex, buffer, bufferSize);
}

void Serializer::readBuffer(char* buffer, uint readLen)
{
	if (!checkRead(readLen))
	{
		return;
	}
	// 如果存放数据的空间大小不足以放入当前要读取的数据,则只拷贝能容纳的长度,但是下标应该正常跳转
	if (!BinaryUtility::readBuffer(mBuffer, mBufferSize, mIndex, buffer, readLen))
	{
		mIndex += readLen;
	}
}

void Serializer::writeString(const char* str)
{
	// 先写入字符串长度
	uint writeLen = (uint)strlen(str);
	write(writeLen);
	if (!writeCheck(writeLen))
	{
		return;
	}
	BinaryUtility::writeBuffer(mBuffer, mBufferSize, mIndex, (char*)str, writeLen);
}

void Serializer::readString(char* str)
{
	// 先读入字符串长度
	uint readLen = 0;
	read(readLen);
	if (!checkRead(readLen))
	{
		return;
	}
	BinaryUtility::readBuffer(mBuffer, mBufferSize, mIndex, str, readLen);
	str[readLen] = 0;
}

void Serializer::writeToFile(const string& fullName)
{
	// 确保是只写的,并且数据不为空
	if (!mWriteFlag || mBuffer == NULL || mIndex <= 0)
	{
		return;
	}
	FileUtility::writeFile(fullName, mBuffer, mIndex);
}

void Serializer::resizeBuffer(uint maxSize)
{
	uint newSize = maxSize > mBufferSize << 1 ? maxSize : mBufferSize << 1;
	char* newBuffer = NULL;
	if (mTraceMemory)
	{
		newBuffer = ArrayPool::newArray<char>(newSize);
	}
	else
	{
		newBuffer = NORMAL_NEW_ARRAY(char, newSize, newBuffer);
	}
	memcpy(newBuffer, mBuffer, mBufferSize);
	if (mTraceMemory)
	{
		ArrayPool::deleteArray(mBuffer);
	}
	else
	{
		NORMAL_DELETE_ARRAY(mBuffer);
	}
	mBuffer = newBuffer;
	mBufferSize = newSize;
}

void Serializer::createBuffer(uint bufferSize)
{
	if (mBuffer == NULL)
	{
		mBufferSize = bufferSize;
		if (mTraceMemory)
		{
			mBuffer = ArrayPool::newArray<char>(mBufferSize);
		}
		else
		{
			mBuffer = NORMAL_NEW_ARRAY(char, mBufferSize, mBuffer);
		}
	}
}
