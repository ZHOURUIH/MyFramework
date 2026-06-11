#pragma once

#include "UsingSTD.h"

// 为了能够给T设置默认参数,只能将T放在最后
// 由于char数组是使用频率最高的类型,所以默认参数为char,也就是当使用MyString<256>时就表示Array<256, char>
template<int Length, class T>
class Array
{
public:
	T* begin() { return mValue; }
	T* end() { return mValue + Length; }
	const T* begin() const { return mValue; }
	const T* end() const { return mValue + Length; }
	const T* cbegin() const { return mValue; }
	const T* cend() const { return mValue + Length; }
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
		FOR(curCount)
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
		FOR(length)
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
			ERROR("数组越界,index:" + to_string(offset) + ", Length:" + to_string(Length));
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
			ERROR("数组越界,index:" + to_string(index) + ", Length:" + to_string(Length));
			index = Length - 1;
		}
		return mValue[index];
	}
	constexpr T& operator[](int index)
	{
		if (index < 0 || index >= Length)
		{
			ERROR("数组越界,index:" + to_string(index) + ", Length:" + to_string(Length));
			index = Length - 1;
		}
		return mValue[index];
	}
	// 将数组的内容重置为0,但是需要外部确保将所有字节设置为0不会引起问题,如果T类型是string或者其他的类则不允许使用此函数
	// 因为那会将虚表也重置为0,引起崩溃
	void zero()
	{
		static_assert(!std::is_polymorphic<T>::value, "Type T cannot have virtual functions");
		memset(mValue, 0, sizeof(T) * Length);
	}
	// 将数组的每一个元素都设置为value
	void fillArray(const T& value)
	{
		FOR(Length)
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
	T mValue[Length]{};
};

// 为长度为0特化的类型
template<typename T>
class Array<0, T>
{
public:
	T mValue[1];
};