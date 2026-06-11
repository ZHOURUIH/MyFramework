#include "FrameHeader.h"

Serializer::Serializer(bool traceMemory):
	mBuffer(nullptr),
	mBufferSize(0),
	mIndex(0),
	mTraceMemory(traceMemory),
	mWriteFlag(true)
{}

Serializer::Serializer(char* buffer, uint bufferSize):
	mBuffer(buffer),
	mBufferSize(bufferSize),
	mIndex(0),
	mTraceMemory(true),
	mWriteFlag(false)
{}

Serializer::~Serializer()
{
	if (mWriteFlag)
	{
		if (mTraceMemory)
		{
			deleteCharArray(mBuffer);
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

void Serializer::readBuffer(char* buffer, uint bufferSize, uint readLen)
{
	if (!checkRead(readLen))
	{
		return;
	}
	// 如果存放数据的空间大小不足以放入当前要读取的数据,则只拷贝能容纳的长度,但是下标应该正常跳转
	if (!BinaryUtility::readBuffer(mBuffer, mBufferSize, mIndex, buffer, bufferSize, readLen))
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

void Serializer::readString(char* str, uint bufferSize)
{
	// 先读入字符串长度
	uint readLen = 0;
	read(readLen);
	if (!checkRead(readLen))
	{
		return;
	}
	BinaryUtility::readBuffer(mBuffer, mBufferSize, mIndex, str, bufferSize, readLen);
	str[readLen] = 0;
}

void Serializer::writeToFile(const string& fullName)
{
	// 确保是只写的,并且数据不为空
	if (!mWriteFlag)
	{
		return;
	}
	if (mBuffer == nullptr || mIndex == 0)
	{
		writeEmptyFile(fullName);
		return;
	}
	writeFile(fullName, mBuffer, mIndex);
}

void Serializer::resizeBuffer(uint maxSize)
{
	uint newSize = maxSize > mBufferSize << 1 ? maxSize : mBufferSize << 1;
	char* newBuffer = nullptr;
	if (mTraceMemory)
	{
		newBuffer = newCharArray(newSize);
	}
	else
	{
		newBuffer = NORMAL_NEW_ARRAY(char, newSize, newBuffer);
	}
	MEMCPY(newBuffer, newSize, mBuffer, mBufferSize);
	if (mTraceMemory)
	{
		deleteCharArray(mBuffer);
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
	if (mBuffer == nullptr)
	{
		mBufferSize = bufferSize;
		if (mTraceMemory)
		{
			mBuffer = newCharArray(mBufferSize);
		}
		else
		{
			mBuffer = NORMAL_NEW_ARRAY(char, mBufferSize, mBuffer);
		}
	}
}

bool Serializer::checkRead(uint readLen)
{
	// 如果是只写的,则不能读取
	if (mWriteFlag)
	{
		ERROR("the buffer is write only, can not read!");
		return false;
	}
	if (mBuffer == nullptr)
	{
		ERROR("buffer is nullptr! can not read");
		return false;
	}
	if (mIndex + readLen > mBufferSize)
	{
		ERROR("read buffer out of range! cur index : " + intToString(mIndex) + ", buffer size : " + intToString(mBufferSize) + ", read length : " + intToString(readLen));
		return false;
	}
	return true;
}

bool Serializer::writeCheck(uint writeLen)
{
	// 如果是只读的,则不能写入
	if (!mWriteFlag)
	{
		ERROR("the buffer is read only, can not write!");
		return false;
	}
	// 如果缓冲区为空,则创建缓冲区
	if (mBuffer == nullptr)
	{
		createBuffer(writeLen);
	}
	// 如果缓冲区已经不够用了,则重新扩展缓冲区
	else if (writeLen + mIndex > mBufferSize)
	{
		resizeBuffer(writeLen + mIndex);
	}
	return true;
}