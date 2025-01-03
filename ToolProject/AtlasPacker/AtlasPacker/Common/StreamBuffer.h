#ifndef _STREAM_BUFFER_H_
#define _STREAM_BUFFER_H_

#include "ServerDefine.h"
#include "txMemoryTrace.h"

class StreamBuffer
{
public:
	int mBufferSize;
	char* mBuffer;
	int mDataLength;
public:
	StreamBuffer(int bufferSize)
	{
		resizeBuffer(bufferSize);
	}
	~StreamBuffer()
	{
		TRACE_DELETE_ARRAY(mBuffer);
	}
	bool isAvailable(int count)
	{
		return count <= mBufferSize - mDataLength;
	}
	void addDataToInputBuffer(const char* data, int count)
	{
		// 缓冲区足够放下数据时才处理
		if (count <= mBufferSize - mDataLength)
		{
			memcpy(mBuffer + mDataLength, data, count);
			mDataLength += count;
		}
	}
	void removeDataFromInputBuffer(int start, int count)
	{
		if (mDataLength >= start + count)
		{
			memmove(mBuffer + start, mBuffer + start + count, mDataLength - start - count);
			mDataLength -= count;
		}
	}
	void clearInputBuffer()
	{
		mDataLength = 0;
	}
protected:
	//-------------------------------------------------------------------------------------------------------------
	void resizeBuffer(int size)
	{
		if (mBufferSize >= size)
		{
			return;
		}
		mBufferSize = size;
		if (mBuffer != NULL)
		{
			// 创建新的缓冲区,将原来的数据拷贝到新缓冲区中,销毁原缓冲区,指向新缓冲区
			char* newBuffer = TRACE_NEW_ARRAY(char, mBufferSize, newBuffer);
			if (mDataLength > 0)
			{
				memcpy(newBuffer, mBuffer, mDataLength);
			}
			mBuffer = newBuffer;
		}
		else
		{
			mBuffer = TRACE_NEW_ARRAY(char, mBufferSize, mBuffer);
		}
	}
};

#endif