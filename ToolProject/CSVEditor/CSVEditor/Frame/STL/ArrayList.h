#pragma once

#include "UsingSTD.h"

template<int Length, class T>
class ArrayList
{
public:
	// 不使用ArrayList() = default; 因为这样仍然会允许使用初始化列表
	ArrayList() {}
	const T* data() const { return mValue; }
	T* data() { return mValue; }
	int size() const { return mSize; }
	// 如果设置的size比当前size小,则不会生效
	void setSize(int size) 
	{
		FOR(size - mSize)
		{
			mValue[mSize++] = mDefault;
		}
	}
	constexpr int maxSize() const { return Length; }
	void resize(int size) { mSize = size; }
	bool add(const T& value0, const T& value1, const T& value2, const T& value3, const T& value4, const T& value5)
	{
		if (mSize + 5 >= Length)
		{
			return false;
		}
		mValue[mSize] = value0;
		mValue[mSize + 1] = value1;
		mValue[mSize + 2] = value2;
		mValue[mSize + 3] = value3;
		mValue[mSize + 4] = value4;
		mValue[mSize + 5] = value5;
		mSize += 6;
		return true;
	}
	bool add(const T& value0, const T& value1, const T& value2, const T& value3, const T& value4)
	{
		if (mSize + 4 >= Length)
		{
			return false;
		}
		mValue[mSize] = value0;
		mValue[mSize + 1] = value1;
		mValue[mSize + 2] = value2;
		mValue[mSize + 3] = value3;
		mValue[mSize + 4] = value4;
		mSize += 5;
		return true;
	}
	bool add(const T& value0, const T& value1, const T& value2, const T& value3)
	{
		if (mSize + 3 >= Length)
		{
			return false;
		}
		mValue[mSize] = value0;
		mValue[mSize + 1] = value1;
		mValue[mSize + 2] = value2;
		mValue[mSize + 3] = value3;
		mSize += 4;
		return true;
	}
	bool add(const T& value0, const T& value1, const T& value2)
	{
		if (mSize + 2 >= Length)
		{
			return false;
		}
		mValue[mSize] = value0;
		mValue[mSize + 1] = value1;
		mValue[mSize + 2] = value2;
		mSize += 3;
		return true;
	}
	bool add(const T& value0, const T& value1)
	{
		if (mSize + 1 >= Length)
		{
			return false;
		}
		mValue[mSize] = value0;
		mValue[mSize + 1] = value1;
		mSize += 2;
		return true;
	}
	bool add(const T& value)
	{
		if (mSize >= Length)
		{
			return false;
		}
		mValue[mSize++] = value;
		return true;
	}
	bool addNotEqual(const T& value, const T& other)
	{
		if (mSize >= Length)
		{
			return false;
		}
		if (value == other)
		{
			return false;
		}
		mValue[mSize++] = value;
		return true;
	}
	bool addUnique(const T& value)
	{
		if (mSize >= Length)
		{
			return false;
		}
		if (contains(value))
		{
			return false;
		}
		mValue[mSize++] = value;
		return true;
	}
	bool add(T&& value)
	{
		if (mSize >= Length)
		{
			return false;
		}
		mValue[mSize++] = move(value);
		return true;
	}
	void eraseElement(const T& element)
	{
		int curCount = mSize;
		FOR_INVERSE_I(curCount)
		{
			if (mValue[i] == element)
			{
				eraseAt(i);
			}
		}
	}
	bool eraseLastElement(const T& element)
	{
		FOR_INVERSE_I(mSize)
		{
			if (mValue[i] == element)
			{
				eraseAt(i);
				return true;
			}
		}
		return false;
	}
	bool eraseFirstElement(const T& element)
	{
		FOR(mSize)
		{
			if (mValue[i] == element)
			{
				eraseAt(i);
				return true;
			}
		}
		return false;
	}
	void eraseAt(const int index)
	{
		if (index < mSize - 1)
		{
			memmove((void*)(mValue + index), (void*)(mValue + index + 1), sizeof(T) * (mSize - index - 1));
		}
		--mSize;
	}
	bool contains(const T& element) const
	{
		FOR(mSize)
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
	bool addRange(const ArrayList<SourceLength, T>& src)
	{
		static_assert(!std::is_polymorphic<T>::value, "Type T cannot have virtual functions");
		const int copyLength = src.size();
		if (copyLength + mSize > Length)
		{
			LOG("拷贝数组太长");
			return false;
		}
		MEMCPY((char*)mValue + sizeof(T) * mSize, sizeof(T) * (Length - mSize), src.data(), sizeof(T) * copyLength);
		mSize += src.size();
		return true;
	}
	bool addRange(const T* src, const int copyCount)
	{
		static_assert(!std::is_polymorphic<T>::value, "Type T cannot have virtual functions");
		if (copyCount + mSize > Length)
		{
			LOG("拷贝数组太长");
			return false;
		}
		MEMCPY((char*)mValue + sizeof(T) * mSize, sizeof(T) * (Length - mSize), src, sizeof(T) * copyCount);
		mSize += copyCount;
		return true;
	}
	T* data(const int offset)
	{
		if (offset >= mSize)
		{
			ERROR("数组越界");
			return nullptr;
		}
		return mValue + offset; 
	}
	const T* data(const int offset) const
	{
		if (offset >= mSize)
		{
			ERROR("数组越界");
			return nullptr;
		}
		return mValue + offset;
	}
	constexpr const T& operator[](int index) const
	{
		if (index < 0 || index >= mSize)
		{
			ERROR("数组越界");
			index = mSize - 1;
		}
		return mValue[index];
	}
	constexpr T& operator[](int index)
	{
		if (index < 0 || index >= mSize)
		{
			ERROR("数组越界");
			index = mSize - 1;
		}
		return mValue[index];
	}
	constexpr const T& get(int index) const
	{
		if (index < 0 || index >= mSize)
		{
			return mDefault;
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
	void clear()
	{
		mSize = 0;
	}
public:
	T mValue[Length]{};
	int mSize = 0;
	static const ArrayList<Length, T> mDefaultList;
	static const T mDefault;
};

template<int Length, typename T>
const T ArrayList<Length, T>::mDefault = T();
template<int Length, typename T>
const ArrayList<Length, T> ArrayList<Length, T>::mDefaultList;