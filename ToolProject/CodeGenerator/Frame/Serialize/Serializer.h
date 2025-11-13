#ifndef _SERIALIZER_H_
#define _SERIALIZER_H_

#include "FrameDefine.h"
#include "Utility.h"

// 用于生成二进制文件的
class Serializer
{
public:
	Serializer(bool traceMemory = true);
	Serializer(char* buffer, uint bufferSize);
	virtual ~Serializer();
	template<typename T>
	void write(T value)
	{
		if (!writeCheck(sizeof(T)))
		{
			return;
		}
		BinaryUtility::write<T>(mBuffer, mBufferSize, mIndex, value);
	}
	template<typename T>
	void read(T& value)
	{
		if (!checkRead(sizeof(T)))
		{
			return;
		}
		value = BinaryUtility::read<T>(mBuffer, mBufferSize, mIndex);
	}
	void writeBuffer(char* buffer, uint bufferSize);
	void readBuffer(char* buffer, uint readLen);
	void writeString(const char* str);
	void readString(char* str);
	void writeToFile(const string& fullName);
	const char* getBuffer() const { return mBuffer; }
	uint getBufferSize() const { return mBufferSize; }
	uint getDataSize() const { return mIndex; }
	void setIndex(int index) { mIndex = index; }
	int getIndex() const { return mIndex; }
protected:
	void resizeBuffer(uint maxSize);
	void createBuffer(uint bufferSize);
	bool checkRead(uint readLen)
	{
		// 如果是只写的,则不能读取
		if (mWriteFlag)
		{
			ERROR("the buffer is write only, can not read!");
			return false;
		}
		if (mBuffer == NULL)
		{
			ERROR("buffer is NULL! can not read");
			return false;
		}
		if (mIndex + readLen > mBufferSize)
		{
			ERROR("read buffer out of range! cur index : " + StringUtility::intToString(mIndex) + ", buffer size : " + StringUtility::intToString(mBufferSize) + ", read length : " + StringUtility::intToString(readLen));
			return false;
		}
		return true;
	}
	bool writeCheck(uint writeLen)
	{
		// 如果是只读的,则不能写入
		if (!mWriteFlag)
		{
			ERROR("the buffer is read only, can not write!");
			return false;
		}
		// 如果缓冲区为空,则创建缓冲区
		if (mBuffer == NULL)
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
protected:
	char* mBuffer;
	uint mIndex;
	uint mBufferSize;
	bool mWriteFlag;	// 如果为真,则表示是只写的,为假则表示是只读的
	bool mTraceMemory;	// 为真则表示所有的内存申请释放操作都会被记录下来,为假则不会被记录,在记录内存跟踪信息时会用到
};

#endif