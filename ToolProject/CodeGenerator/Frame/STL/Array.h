#pragma once

#include "FrameDefine.h"

// 为了能够给T设置默认参数,只能将T放在最后
// 由于char数组是使用频率最高的类型,所以默认参数为char,也就是当使用Array<256>时就表示Array<256, char>
template<int Length, class T = char>
class Array
{
public:
	const T* data() const { return mValue; }
	T* data() { return mValue; }
	int eraseElement(const T& element, const int curCount)
	{
		int newCount = curCount;
		FOR_INVERSE_I(curCount)
		{
			if (mValue[i] == element)
			{
				eraseAt(curCount, i);
				--newCount;
			}
		}
		return newCount;
	}
	bool eraseLastElement(const T& element, const int curCount)
	{
		FOR_INVERSE_I(curCount)
		{
			if (mValue[i] == element)
			{
				eraseAt(curCount, i);
				return true;
			}
		}
		return false;
	}
	bool eraseFirstElement(const T& element, const int curCount)
	{
		FOR_I(curCount)
		{
			if (mValue[i] == element)
			{
				eraseAt(curCount, i);
				return true;
			}
		}
		return false;
	}
	void eraseAt(const int curCount, const int index)
	{
		if (index < curCount - 1)
		{
			memmove((void*)(mValue + index), (void*)(mValue + index + 1), sizeof(T) * (Length - index - 1));
		}
	}
	bool contains(const T& element, int length = -1) const
	{
		if (length < 0)
		{
			length = Length;
		}
		FOR_I(length)
		{
			if (mValue[i] == element)
			{
				return true;
			}
		}
		return false;
	}
	// 在拷贝过程中不会调用拷贝构造,所以不适用于T带拷贝构造的情况
	template<int SourceLength>
	void copy(const Array<SourceLength, T>& src)
	{
		if (SourceLength > Length)
		{
			LOG("拷贝数组太长");
			return;
		}
		MEMCPY(mValue, sizeof(T) * Length, src.data(), sizeof(T) * SourceLength);
	}
	template<int SourceLength>
	void copy(const int destOffset, const Array<SourceLength, T>& src)
	{
		if (destOffset + SourceLength > Length)
		{
			LOG("拷贝数组太长");
			return;
		}
		MEMCPY((char*)mValue + sizeof(T) * destOffset, sizeof(T) * (Length - destOffset), src.data(), sizeof(T) * SourceLength);
	}
	template<int SourceLength>
	void copy(const Array<SourceLength, T>& src, const int copyCount)
	{
		if (copyCount > Length)
		{
			LOG("拷贝数组太长");
			return;
		}
		MEMCPY(mValue, sizeof(T) * Length, src.data(), sizeof(T) * copyCount);
	}
	template<int SourceLength>
	void copy(const int destOffset, const Array<SourceLength, T>& src, const int copyCount)
	{
		if (destOffset + copyCount > Length)
		{
			LOG("拷贝数组太长");
			return;
		}
		MEMCPY((char*)mValue + sizeof(T) * destOffset, sizeof(T) * (Length - destOffset), src.data(), sizeof(T) * copyCount);
	}
	void copy(const T* src, const int copyCount)
	{
		if (copyCount > Length)
		{
			LOG("拷贝数组太长");
			return;
		}
		MEMCPY(mValue, sizeof(T) * Length, src, sizeof(T) * copyCount);
	}
	void copy(const int destOffset, const T* src, const int copyCount)
	{
		if (destOffset + copyCount > Length)
		{
			LOG("拷贝数组太长");
			return;
		}
		MEMCPY((char*)mValue + sizeof(T) * destOffset, sizeof(T) * (Length - destOffset), src, sizeof(T) * copyCount);
	}
	T* data(const int offset)
	{
		if (offset >= Length)
		{
			ERROR("数组越界");
			return nullptr;
		}
		return mValue + offset; 
	}
	const T* data(const int offset) const
	{
		if (offset >= Length)
		{
			ERROR("数组越界");
			return nullptr;
		}
		return mValue + offset;
	}
	constexpr int size() const { return Length; }
	constexpr const T& operator[](int index) const
	{
		if (index < 0 || index >= Length)
		{
			ERROR("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	constexpr T& operator[](int index)
	{
		if (index < 0 || index >= Length)
		{
			ERROR("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	// 将数组的内容重置为0,但是需要外部确保将所有字节设置为0不会引起问题,如果T类型是string或者其他的类则不允许使用此函数
	// 因为那会将虚表也重置为0,引起崩溃
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
	void fillArray(const int start, const T& value)
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
template<int Length>
class Array<Length, char>
{
public:
	// 为了方便查代码,将返回指针的函数命名为具体的名字
	const char* str() const			{ return mValue; }
	char* toBuffer()				{ return mValue; }
	// 仅为了在类型匹配时能够编译通过,尽量避免直接使用
	const char* data() const		{ return mValue; }
	char* data()					{ return mValue; }
	bool isEmpty() const			{ return mValue[0] == '\0'; }
	int length() const 
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
	template<int SourceLength>
	void copy(const Array<SourceLength>& src)
	{
		if (SourceLength > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.str(), SourceLength);
	}
	template<int SourceLength>
	void copy(const Array<SourceLength>& src, const int copyCount)
	{
		if (copyCount > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.str(), copyCount);
	}
	void copy(const char* src) const
	{
		const int length = (int)::strlen(src);
		if (length > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src, ::strlen(src));
	}
	void copy(const char* src, const int copyCount) const
	{
		if (copyCount > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src, copyCount);
	}
	void copy(const string& src, const int copyCount) const
	{
		if (copyCount > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.c_str(), copyCount);
	}
	void copy(const string& src, const int srcOffset, const int copyCount) const
	{
		if (copyCount > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.c_str() + srcOffset, copyCount);
	}
	template<int SourceLength>
	void copy(const int destOffset, const Array<SourceLength>& src)
	{
		if (SourceLength + destOffset > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.str(), SourceLength);
	}
	template<int SourceLength>
	void copy(const int destOffset, const Array<SourceLength>& src, const int copyCount)
	{
		if (copyCount + destOffset > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.str(), copyCount);
	}
	void copy(const int destOffset, const char* src, const int copyCount)
	{
		if (copyCount + destOffset > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src, copyCount);
	}
	void copy(const int destOffset, const string& src)
	{
		if (destOffset + src.length() > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.c_str(), src.length());
	}
	void copy(const int destOffset, const string& src, const int copyLength)
	{
		if (destOffset + copyLength > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.c_str(), copyLength);
	}
	constexpr int size() const { return Length; }
	constexpr const char& operator[](int index) const
	{
		if (index < 0 || index >= Length)
		{
			ERROR("数组越界");
			index = Length - 1;
		}
		return mValue[index];
	}
	constexpr char& operator[](int index)
	{
		if (index < 0 || index >= Length)
		{
			ERROR("数组越界");
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
	void fillArray(const char value)
	{
		memset(mValue, value, Length);
	}
	// 将数组的从start开始的所有元素都设置为value
	void fillArray(const int start, const char value)
	{
		memset(mValue + start, value, Length - start);
	}
	void setString(const char* str, int strLength = -1)
	{
		if (strLength == -1)
		{
			strLength = (int)::strlen(str);
		}
		copy(str, strLength);
		mValue[strLength] = '\0';
	}
	void setString(const string& str)
	{
		const int len = (int)str.length();
		copy(str.c_str(), len);
		mValue[len] = '\0';
	}
	void setString(const int offset, const char* str, int strLength = 0)
	{
		if (strLength == 0)
		{
			strLength = (int)::strlen(str);
		}
		copy(offset, str, strLength);
		mValue[offset + strLength] = '\0';
	}
	void setString(const int offset, const string& str)
	{
		copy(offset, str);
		mValue[offset + (int)str.length()] = '\0';
	}
public:
	char mValue[Length];
};