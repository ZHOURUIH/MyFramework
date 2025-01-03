#include "txSerializer.h"

txSerializer::txSerializer(bool traceMemery)
:
mBuffer(NULL),
mIndex(0),
mBufferSize(0),
mWriteFlag(true),
mTraceMemery(traceMemery)
{}

txSerializer::txSerializer(char* buffer, int bufferSize)
:
mBuffer(buffer),
mIndex(0),
mBufferSize(bufferSize),
mWriteFlag(false),
mTraceMemery(true)
{}

txSerializer::~txSerializer()
{
	if (mWriteFlag)
	{
		if (mTraceMemery)
		{
			DELETE_ARRAY(mBuffer);
		}
		else
		{
			NORMAL_DELETE_ARRAY(mBuffer);
		}
	}
}

void txSerializer::writeBuffer(char* buffer, int bufferSize)
{
	if (!writeCheck(bufferSize))
	{
		return;
	}
	BinaryUtility::writeBuffer(mBuffer, mBufferSize, mIndex, buffer, bufferSize);
}

void txSerializer::readBuffer(char* buffer, int readLen)
{
	if (!checkRead(readLen))
	{
		return;
	}
	BinaryUtility::readBuffer(mBuffer, mBufferSize, mIndex, buffer, readLen, readLen);
	//// 如果存放数据的空间大小不足以放入当前要读取的数据,则只拷贝能容纳的长度,但是下标应该正常跳转
	//if (bufferSize <= readLen)
	//{
	//	memcpy(buffer, mBuffer + mIndex, bufferSize);
	//	mIndex += readLen;
	//}
	//else
	//{
	//	memcpy(buffer, mBuffer + mIndex, readLen);
	//	mIndex += readLen;
	//}
}

void txSerializer::writeString(const char* str)
{
	// 先写入字符串长度
	int writeLen = strlen(str);
	write(writeLen);
	if (!writeCheck(writeLen))
	{
		return;
	}
	BinaryUtility::writeBuffer(mBuffer, mBufferSize, mIndex, (char*)str, writeLen);
}

void txSerializer::readString(char* str)
{
	// 先读入字符串长度
	int readLen = 0;
	read(readLen);
	if (!checkRead(readLen))
	{
		return;
	}
	BinaryUtility::readBuffer(mBuffer, mBufferSize, mIndex, str, readLen, readLen);
	str[readLen] = 0;
}

void txSerializer::writeToFile(const string& fullName)
{
	// 确保是只写的,并且数据不为空
	if (!mWriteFlag || mBuffer == NULL || mIndex <= 0)
	{
		return;
	}
	FileUtility::writeFile(fullName, mBuffer, mIndex);
}

void txSerializer::resizeBuffer(int maxSize)
{
	int newSize = maxSize > mBufferSize * 2 ? maxSize : mBufferSize * 2;
	char* newBuffer = NULL;
	if (mTraceMemery)
	{
		newBuffer = NEW_ARRAY(char, newSize, newBuffer);
	}
	else
	{
		newBuffer = NORMAL_NEW_ARRAY(char, newSize, newBuffer);
	}
	memcpy(newBuffer, mBuffer, mBufferSize);
	if (mTraceMemery)
	{
		DELETE_ARRAY(mBuffer);
	}
	else
	{
		NORMAL_DELETE_ARRAY(mBuffer);
	}
	mBuffer = newBuffer;
	mBufferSize = newSize;
}

void txSerializer::createBuffer(int bufferSize)
{
	if (mBuffer == NULL)
	{
		mBufferSize = bufferSize;
		if (mTraceMemery)
		{
			mBuffer = NEW_ARRAY(char, mBufferSize, mBuffer);
		}
		else
		{
			mBuffer = NORMAL_NEW_ARRAY(char, mBufferSize, mBuffer);
		}
	}
}
