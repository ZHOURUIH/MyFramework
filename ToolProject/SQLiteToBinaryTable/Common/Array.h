#ifndef _ARRAY_H_
#define _ARRAY_H_

#include <string>
#include "IndependentLog.h"

#define FOR_I(count)			for (int i = 0; i < (int)count; ++i)
#define FOR_J(count)			for (int j = 0; j < (int)count; ++j)
#define FOR_K(count)			for (int k = 0; k < (int)count; ++k)
#define FOR_INVERSE_I(count)	for (int i = count - 1; i >= 0; --i)
#define FOR_INVERSE_J(count)	for (int j = count - 1; j >= 0; --j)
#define FOR_INVERSE_K(count)	for (int k = count - 1; k >= 0; --k)
#define FOR_ONCE				for(int tempI = 0; tempI < 1; ++tempI)
#define MEMCPY(dest, bufferSize, src, count)		memcpy_s((void*)(dest), bufferSize, (void*)(src), count)

typedef unsigned int uint;

// 为了能够给T设置默认参数,只能将T放在最后
// 由于char数组是使用频率最高的类型,所以默认参数为char,也就是当使用Array<256>时就表示Array<256, char>
template<size_t Length, class T = char>
class Array
{
public:
	const T* data() const { return mValue; }
	T* data() { return mValue; }
	uint eraseElement(const T& element, uint curCount)
	{
		uint newCount = curCount;
		FOR_INVERSE_I(curCount)
		{
			if (mValue[i] == element)
			{
				memmove((void*)(mValue + i), (void*)(mValue + i + 1), sizeof(T) * (Length - i - 1));
				--newCount;
			}
		}
		return newCount;
	}
	uint eraseLastElement(const T& element, uint curCount)
	{
		uint newCount = curCount;
		FOR_INVERSE_I(curCount)
		{
			if (mValue[i] == element)
			{
				memmove((void*)(mValue + i), (void*)(mValue + i + 1), sizeof(T) * (Length - i - 1));
				--newCount;
				break;
			}
		}
		return newCount;
	}
	uint eraseFirstElement(const T& element, uint curCount)
	{
		uint newCount = curCount;
		FOR_I(curCount)
		{
			if (mValue[i] == element)
			{
				memmove((void*)(mValue + i), (void*)(mValue + i + 1), sizeof(T) * (Length - i - 1));
				--newCount;
				break;
			}
		}
		return newCount;
	}
	bool contains(const T& element, int length = -1)
	{
		if (length < 0)
		{
			length = Length;
		}
		FOR_I((uint)length)
		{
			if (mValue[i] == element)
			{
				return true;
			}
		}
		return false;
	}
	template<size_t SourceLength>
	void copy(const Array<SourceLength, T>& src)
	{
		if (SourceLength > Length)
		{
			IndependentLog::directLog("拷贝数组太长");
			return;
		}
		MEMCPY(mValue, sizeof(T) * Length, src.data(), sizeof(T) * SourceLength);
	}
	template<size_t SourceLength>
	void copy(uint destOffset, const Array<SourceLength, T>& src)
	{
		if (destOffset + SourceLength > Length)
		{
			IndependentLog::directLog("拷贝数组太长");
			return;
		}
		MEMCPY(mValue + sizeof(T) * destOffset, sizeof(T) * (Length - destOffset), src.data(), sizeof(T) * SourceLength);
	}
	template<size_t SourceLength>
	void copy(const Array<SourceLength, T>& src, uint copyCount)
	{
		if (copyCount > Length)
		{
			IndependentLog::directLog("拷贝数组太长");
			return;
		}
		MEMCPY(mValue, sizeof(T) * Length, src.data(), sizeof(T) * copyCount);
	}
	template<size_t SourceLength>
	void copy(uint destOffset, const Array<SourceLength, T>& src, uint copyCount)
	{
		if (destOffset + copyCount > Length)
		{
			IndependentLog::directLog("拷贝数组太长");
			return;
		}
		MEMCPY(mValue + sizeof(T) * destOffset, sizeof(T) * (Length - destOffset), src.data(), sizeof(T) * copyCount);
	}
	void copy(const T* src, uint copyCount)
	{
		if (copyCount > Length)
		{
			IndependentLog::directLog("拷贝数组太长");
			return;
		}
		MEMCPY(mValue, sizeof(T) * Length, src, sizeof(T) * copyCount);
	}
	void copy(uint destOffset, const T* src, uint copyCount)
	{
		if (destOffset + copyCount > Length)
		{
			IndependentLog::directLog("拷贝数组太长");
			return;
		}
		MEMCPY(mValue + sizeof(T) * destOffset, sizeof(T) * (Length - destOffset), src, sizeof(T) * copyCount);
	}
	T* data(uint offset) 
	{
		if (offset >= Length)
		{
			IndependentLog::directError("数组越界");
			return nullptr;
		}
		return mValue + offset; 
	}
	const T* data(uint offset) const
	{
		if (offset >= Length)
		{
			IndependentLog::directError("数组越界");
			return nullptr;
		}
		return mValue + offset;
	}
	constexpr uint size() const { return Length; }
	constexpr const T& operator[](uint index) const
	{
		if (index >= Length)
		{
			IndependentLog::directError("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	constexpr T& operator[](uint index)
	{
		if (index >= Length)
		{
			IndependentLog::directError("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	// 将数组的内容重置为0
	void zero()
	{
		memset(mValue, 0, sizeof(T) * Length);
	}
	// 将数组的每一个元素都设置为value
	void fillArray(const T& value)
	{
		FOR_I(Length)
		{
			mValue[i] = value;
		}
	}
	void fillArray(int start, const T& value)
	{
		for (int i = start; i < Length; ++i)
		{
			mValue[i] = value;
		}
	}
public:
	T mValue[Length];
};

// 为长度为0特化的类型
template<typename T>
class Array<0, T>
{
public:
	T mValue[1];
};

// 为char特化的类型,因为常常会当作字符串使用
template<size_t Length>
class Array<Length, char>
{
public:
	// 为了方便查代码,将返回指针的函数命名为具体的名字
	const char* toString() const	{ return mValue; }
	char* toBuffer()				{ return mValue; }
	// 仅为了在类型匹配时能够编译通过,尽量避免直接使用
	const char* data() const		{ return mValue; }
	char* data()					{ return mValue; }
	uint length() const 
	{
		FOR_I(Length)
		{
			if (mValue[i] == '\0')
			{
				return i;
			}
		}
		return Length;
	}
	template<size_t SourceLength>
	void copy(const Array<SourceLength>& src)
	{
		if (SourceLength > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.toString(), SourceLength);
	}
	template<size_t SourceLength>
	void copy(const Array<SourceLength>& src, uint copyCount)
	{
		if (copyCount > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.toString(), copyCount);
	}
	void copy(const char* src)
	{
		uint length = (uint)strlen(src);
		if (length > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src, strlen(src));
	}
	void copy(const char* src, uint copyCount)
	{
		if (copyCount > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src, copyCount);
	}
	void copy(const string& src, uint copyCount)
	{
		if (copyCount > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.c_str(), copyCount);
	}
	void copy(const string& src, int srcOffset, uint copyCount)
	{
		if (copyCount > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.c_str() + srcOffset, copyCount);
	}
	template<size_t SourceLength>
	void copy(int destOffset, const Array<SourceLength>& src)
	{
		if (SourceLength + destOffset > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.toString(), SourceLength);
	}
	template<size_t SourceLength>
	void copy(int destOffset, const Array<SourceLength>& src, uint copyCount)
	{
		if (copyCount + destOffset > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.toString(), copyCount);
	}
	void copy(int destOffset, const char* src, uint copyCount)
	{
		if (copyCount + destOffset > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src, copyCount);
	}
	void copy(int destOffset, const string& src)
	{
		if (destOffset + src.length() > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.c_str(), src.length());
	}
	void copy(int destOffset, const string& src, uint copyLength)
	{
		if (destOffset + copyLength > Length)
		{
			IndependentLog::directLog("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.c_str(), copyLength);
	}
	constexpr uint size() const { return Length; }
	constexpr const char& operator[](uint index) const
	{
		if (index >= Length)
		{
			IndependentLog::directError("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	constexpr char& operator[](uint index)
	{
		if (index >= Length)
		{
			IndependentLog::directError("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	// 将数组的内容重置为0
	void zero()
	{
		memset(mValue, 0, sizeof(char) * Length);
	}
	// 将数组的每一个元素都设置为value
	void fillArray(char value)
	{
		memset(mValue, value, Length);
	}
	// 将数组的从start开始的所有元素都设置为value
	void fillArray(int start, char value)
	{
		memset(mValue + start, value, Length - start);
	}
	void setString(const char* str, uint strLength = 0)
	{
		if (strLength == 0)
		{
			strLength = (uint)strlen(str);
		}
		copy(str, strLength);
		mValue[strLength] = '\0';
	}
	void setString(const string& str)
	{
		uint len = (uint)str.length();
		copy(str.c_str(), len);
		mValue[len] = '\0';
	}
	void setString(uint offset, const char* str, uint strLength = 0)
	{
		if (strLength == 0)
		{
			strLength = (uint)strlen(str);
		}
		copy(offset, str, strLength);
		mValue[offset + strLength] = '\0';
	}
	void setString(uint offset, const string& str)
	{
		copy(offset, str);
		mValue[offset + (uint)str.length()] = '\0';
	}
public:
	char mValue[Length];
};

#endif