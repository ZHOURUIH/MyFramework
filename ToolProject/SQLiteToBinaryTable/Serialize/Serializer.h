#ifndef _SERIALIZER_H_
#define _SERIALIZER_H_

#include "FrameBase.h"

// 用于生成二进制文件的
class Serializer : public FrameBase
{
	BASE(FrameBase);
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
	template<typename T>
	void writeArray(const Vector<T>& value)
	{
		write((int)value.size());
		FOR_CONST(value)
		{
			write(value[i]);
		}
		END_CONST();
	}
	template<>
	void writeArray(const Vector<string>& value)
	{
		write((int)value.size());
		FOR_CONST(value)
		{
			writeString(value[i].c_str());
		}
		END_CONST();
	}
	void writeBuffer(char* buffer, uint bufferSize);
	void readBuffer(char* buffer, uint bufferSize, uint readLen);
	void writeString(const char* str);
	void readString(char* str, uint bufferSize);
	void writeToFile(const string& fullName);
	void setIndex(int index)		{ mIndex = index; }
	char* getWriteableBuffer() const{ return mBuffer; }
	const char* getBuffer() const	{ return mBuffer; }
	uint getBufferSize() const		{ return mBufferSize; }
	uint getDataSize() const		{ return mIndex; }
	int getIndex() const			{ return mIndex; }
protected:
	void resizeBuffer(uint maxSize);
	void createBuffer(uint bufferSize);
	bool checkRead(uint readLen);
	bool writeCheck(uint writeLen);
protected:
	char* mBuffer;
	uint mBufferSize;
	uint mIndex;
	bool mTraceMemory;	// 为真则表示所有的内存申请释放操作都会被记录下来,为假则不会被记录,在记录内存跟踪信息时会用到
	bool mWriteFlag;	// 如果为真,则表示是只写的,为假则表示是只读的
};

#endif