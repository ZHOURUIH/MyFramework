#include "FrameDefine.h"
#include "Utility.h"
#include "StreamBuffer.h"

StreamBuffer::StreamBuffer(uint bufferSize)
{
	mBufferSize = 0;
	mBuffer = nullptr;
	mDataLength = 0;
	mDataStart = 0;
	resizeBuffer(bufferSize);
	clear();
}
StreamBuffer::~StreamBuffer()
{
	ArrayPool::deleteArray(mBuffer);
}
bool StreamBuffer::isAvailable(uint count)
{
	return count <= mBufferSize - mDataLength;
}
void StreamBuffer::addData(const char* data, uint count, bool clearIfFull)
{
	// 缓冲区空闲部分足够放下数据时才处理
	if (count <= mBufferSize - (mDataStart + mDataLength))
	{
		memcpy(mBuffer + mDataStart + mDataLength, data, count);
		mDataLength += count;
	}
	// 空闲部分不够,查看所有空闲部分是否足够,如果足够,则调整缓冲区
	else if (count <= mBufferSize - mDataLength)
	{
		memmove(mBuffer, mBuffer + mDataStart, mDataLength);
		mDataStart = 0;
		addData(data, count);
	}
	// 整个缓冲区不足,根据参数清空缓冲区或者提示缓冲区太小
	else
	{
		if (clearIfFull && mBufferSize >= count)
		{
			clear();
			addData(data, count);
		}
		else
		{
			LOG("缓冲区太小");
		}
	}
}
void StreamBuffer::removeData(uint count)
{
	if (mDataLength >= count)
	{
		mDataStart += count;
		mDataLength -= count;
	}
	else
	{
		LOG("删除数据失败!数据量不足");
	}
}
void StreamBuffer::clear()
{
	mDataLength = 0;
	mDataStart = 0;
}

void StreamBuffer::resizeBuffer(uint size)
{
	if (mBufferSize >= size)
	{
		return;
	}
	mBufferSize = size;
	if (mBuffer != NULL)
	{
		// 创建新的缓冲区,将原来的数据拷贝到新缓冲区中,销毁原缓冲区,指向新缓冲区
		char* newBuffer = ArrayPool::newArray<char>(mBufferSize);
		if (mDataLength > 0)
		{
			memcpy(newBuffer, mBuffer + mDataStart, mDataLength);
		}
		ArrayPool::deleteArray(mBuffer);
		mBuffer = newBuffer;
		mDataStart = 0;
	}
	else
	{
		mBuffer = ArrayPool::newArray<char>(mBufferSize);
	}
}