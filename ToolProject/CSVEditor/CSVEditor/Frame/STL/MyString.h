#pragma once

#include "UsingSTD.h"

// char的数组,相当于Array<Length, char>,只是经常使用,所以单独写成一个类
template<int Length>
class MyString
{
public:
	// 不使用MyString() = default; 因为这样仍然会允许使用初始化列表
	MyString() {}
	// 为了方便查代码,将返回指针的函数命名为具体的名字
	const char* str() const				{ return mValue; }
	char* toBuffer()					{ return mValue; }
	// 仅为了在类型匹配时能够编译通过,尽量避免直接使用
	const char* data() const			{ return mValue; }
	char* data()						{ return mValue; }
	bool isEmpty() const				{ return mValue[0] == '\0'; }
	constexpr int getMaxLength() const	{ return Length; }
	void clear()						{ mValue[0] = '\0'; }
	int length() const 
	{
		FOR(Length)
		{
			if (mValue[i] == '\0')
			{
				return i;
			}
		}
		return Length;
	}
	template<int SourceLength>
	void copy(const MyString<SourceLength>& src)
	{
		if (SourceLength > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue, Length, src.str(), SourceLength);
	}
	template<int SourceLength>
	void copy(const MyString<SourceLength>& src, const int copyCount)
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
	void copy(const int destOffset, const MyString<SourceLength>& src)
	{
		if (SourceLength + destOffset > Length)
		{
			LOG("拷贝字符串太长");
			return;
		}
		MEMCPY(mValue + destOffset, Length - destOffset, src.str(), SourceLength);
	}
	template<int SourceLength>
	void copy(const int destOffset, const MyString<SourceLength>& src, const int copyCount)
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
	char mValue[Length]{};
};