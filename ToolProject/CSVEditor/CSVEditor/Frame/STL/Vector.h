#pragma once

#include "Array.h"
#include "ArrayList.h"
#include "UsingSTD.h"
#include "IsPODType.h"

template<typename T, typename TypeCheck = typename IsNotPodAndPointerType<T>::mType>
static void copyVector(const vector<T>& source, const int sourceCount, vector<T>& target, const int targetOldCount)
{
	target.reserve(target.size() + sourceCount);
	FOR(sourceCount)
	{
		target.emplace_back(source[i]);
	}
}

template<typename T, typename TypeCheck = typename IsNotPodAndPointerType<T>::mType>
static void copyVector(const T* source, const int sourceCount, vector<T>& target, const int targetOldCount)
{
	target.reserve(targetOldCount + sourceCount);
	FOR(sourceCount)
	{
		target.emplace_back(source[i]);
	}
}

template<typename T, typename TypeCheck = typename IsPodOrPointerType<T>::mType>
static void memcpyVector(const vector<T>& source, const int sourceCount, vector<T>& target, const int targetOldCount)
{
	const int newSize = targetOldCount + sourceCount;
	target.resize(newSize);
	MEMCPY((char*)target.data() + targetOldCount * sizeof(T), newSize * sizeof(T), source.data(), sizeof(T) * sourceCount);
}

template<typename T, typename TypeCheck = typename IsPodOrPointerType<T>::mType>
static void memcpyVector(const T* source, const int sourceCount, vector<T>& target, const int targetOldCount)
{
	const int newSize = targetOldCount + sourceCount;
	target.resize(newSize);
	MEMCPY((char*)target.data() + targetOldCount * sizeof(T), newSize * sizeof(T), source, sizeof(T) * sourceCount);
}

// 对于bool类型可能不太适用,在std::vector内部有对bool类型的特例化,可能某些函数无法正常编译通过
template<typename T>
class Vector
{
public:
	typedef typename vector<T>::iterator iterator;
	typedef typename vector<T>::reverse_iterator reverse_iterator;
	typedef typename vector<T>::const_iterator const_iterator;
public:
	Vector()
	{
		mVector.reserve(4);
	}
	explicit Vector(const int reserveSize)
	{
		mVector.reserve(reserveSize);
	}
	Vector(const Vector<T>& other):
		mVector(other.mVector)
	{
		mSize = other.size();
#ifdef WIN32
		if (mSize > 0)
		{
			LOG("Vector请尝试使用移动构造,减少拷贝构造");
		}
#endif
	}
	Vector(Vector<T>&& other) noexcept:
		mVector(move(other.mVector))
	{
		mSize = other.size();
		other.mSize = 0;
	}
	explicit Vector(initializer_list<T> _Ilist)
	{
		mVector.insert(mVector.begin(), _Ilist);
		mSize = (int)mVector.size();
	}
	Vector<T>& operator=(Vector<T>&& other) noexcept
	{
		mVector = move(other.mVector);
		mSize = other.size();
		other.mSize = 0;
		return *this;
	}
	Vector<T>& operator=(const Vector<T>& other)
	{
		mVector = other.mVector;
		mSize = other.mSize;
#ifdef WIN32
		LOG("Vector请尝试使用移动赋值,减少拷贝赋值");
#endif
		return *this;
	}
	bool operator==(const Vector<llong>& other)
	{
		if (mSize != other.size())
		{
			return false;
		}
		FOR(mSize)
		{
			if (mVector[i] != other[i])
			{
				return false;
			}
		}
		return true;
	}
	bool operator!=(const Vector<llong>& other)
	{
		if (mSize != other.size())
		{
			return true;
		}
		FOR(mSize)
		{
			if (mVector[i] != other[i])
			{
				return true;
			}
		}
		return false;
	}
	virtual ~Vector()				{ mVector.clear(); }
	T* data() const					{ return (T*)mVector.data(); }
	int size() const				{ return mSize; }
	bool isEmpty() const			{ return mSize <= 0; }
	iterator begin()				{ return mVector.begin(); }
	iterator end()					{ return mVector.end(); }
	const_iterator begin() const	{ return mVector.begin(); }
	const_iterator end()  const		{ return mVector.end(); }
	reverse_iterator rbegin() const { return mVector.rbegin(); }
	reverse_iterator rend() const	{ return mVector.rend(); }
	const_iterator cbegin() const	{ return mVector.cbegin(); }
	const_iterator cend() const		{ return mVector.cend(); }
	// 返回最后一个元素,并且将该元素移除,如果当前列表为空,则返回defaultValue
	T popBack(const T& defaultValue)
	{
		if (mSize == 0)
		{
			return defaultValue;
		}
		const T value = mVector[--mSize];
		mVector.pop_back();
		return value;
	}
	// 通过逐个拷贝的方式去添加,仅限非基础数据类型
	void addRangeCopy(const T* values, const int count)
	{
		if (values == nullptr || count == 0)
		{
			return;
		}
		copyVector(values, count, mVector, mSize);
		mSize += count;
	}
	// 通过逐个拷贝的方式去添加,仅限非基础数据类型
	void setRangeCopy(const T* values, const int count)
	{
		clear();
		if (values == nullptr || count == 0)
		{
			return;
		}
		copyVector(values, count, mVector, mSize);
		mSize += count;
	}
	// 通过逐个拷贝的方式去添加,仅限非基础数据类型
	void addRangeCopy(const Vector<T>& values)
	{
		const int sourceCount = values.size();
		if (sourceCount == 0)
		{
			return;
		}
		copyVector(values.mVector, sourceCount, mVector, mSize);
		mSize += sourceCount;
	}
	// 通过逐个拷贝的方式去添加,仅限非基础数据类型
	void setRangeCopy(const Vector<T>& values)
	{
		clear();
		const int sourceCount = values.size();
		if (sourceCount == 0)
		{
			return;
		}
		copyVector(values.mVector, sourceCount, mVector, mSize);
		mSize += sourceCount;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	void addRange(const T* values, const int count)
	{
		if (count == 0)
		{
			return;
		}
		memcpyVector(values, count, mVector, mSize);
		mSize += count;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	void setRange(const T* values, const int count)
	{
		clear();
		if (count == 0)
		{
			return;
		}
		memcpyVector(values, count, mVector, mSize);
		mSize += count;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	template<int Length>
	void addRange(const ArrayList<Length, T>& values)
	{
		const int count = values.size();
		if (count == 0)
		{
			return;
		}
		memcpyVector(values.data(), count, mVector, mSize);
		mSize += count;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	template<int Length>
	void setRange(const ArrayList<Length, T>& values)
	{
		clear();
		const int count = values.size();
		if (count == 0)
		{
			return;
		}
		memcpyVector(values.data(), count, mVector, mSize);
		mSize += count;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	template<int Length>
	void addRange(const Array<Length, T>& values, const int count)
	{
		if (count == 0)
		{
			return;
		}
		memcpyVector(values.data(), count, mVector, mSize);
		mSize += count;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	void addRange(const Vector<T>& values)
	{
		const int sourceCount = values.size();
		if (sourceCount == 0)
		{
			return;
		}
		memcpyVector(values.mVector, sourceCount, mVector, mSize);
		mSize += sourceCount;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型
	void setRange(const Vector<T>& values)
	{
		clear();
		const int sourceCount = values.size();
		if (sourceCount == 0)
		{
			return;
		}
		memcpyVector(values.mVector, sourceCount, mVector, mSize);
		mSize += sourceCount;
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型,虽然这里仍然使用的是拷贝
	void addRange(Vector<T>&& values)
	{
		const int sourceCount = values.size();
		if (sourceCount == 0)
		{
			return;
		}
		memcpyVector(values.mVector, sourceCount, mVector, mSize);
		mSize += sourceCount;
		values.clear();
	}
	// 通过直接内存拷贝的方式进行添加,仅限基础数据类型,虽然这里仍然使用的是拷贝
	void setRange(Vector<T>&& values)
	{
		clear();
		const int sourceCount = values.size();
		if (sourceCount == 0)
		{
			return;
		}
		memcpyVector(values.mVector, sourceCount, mVector, mSize);
		mSize += sourceCount;
		values.clear();
	}
	void setData(const T* values, const int count)
	{
		mVector.clear();
		mSize = count;
		mVector.reserve(mSize);
		FOR(mSize)
		{
			mVector.emplace_back(values[i]);
		}
	}
	void push_back()
	{
		++mSize;
		mVector.emplace_back(mDefaultValue);
	}
	bool addUnique(const T& elem)
	{
		if (!contains(elem))
		{
			++mSize;
			mVector.emplace_back(elem);
			return true;
		}
		return false;
	}
	template <class... ParamList>
	void emplace_back(ParamList&&... _Val)
	{
		++mSize;
		mVector.emplace_back(forward<ParamList>(_Val)...);
	}
	// 一般都是基础数据类型或者指针才会使用此方法,因此不使用引用
	void addNotEqual(T elem, const T other)
	{
		if (elem == other)
		{
			return;
		}
		++mSize;
		mVector.emplace_back(elem);
	}
	void push_back(const T& elem)
	{
		++mSize;
		mVector.emplace_back(elem);
	}
	void push_back(T&& elem)
	{
		++mSize;
		mVector.emplace_back(move(elem));
	}
	void push_back(const T& elem0, const T& elem1)
	{
		mSize += 2;
		mVector.reserve(mSize);
		mVector.emplace_back(elem0);
		mVector.emplace_back(elem1);
	}
	void push_back(const T& elem0, const T& elem1, const T& elem2)
	{
		mSize += 3;
		mVector.reserve(mSize);
		mVector.emplace_back(elem0);
		mVector.emplace_back(elem1);
		mVector.emplace_back(elem2);
	}
	void push_back(const T& elem0, const T& elem1, const T& elem2, const T& elem3)
	{
		mSize += 4;
		mVector.reserve(mSize);
		mVector.emplace_back(elem0);
		mVector.emplace_back(elem1);
		mVector.emplace_back(elem2);
		mVector.emplace_back(elem3);
	}
	void push_back(const T& elem0, const T& elem1, const T& elem2, const T& elem3, const T& elem4)
	{
		mSize += 5;
		mVector.reserve(mSize);
		mVector.emplace_back(elem0);
		mVector.emplace_back(elem1);
		mVector.emplace_back(elem2);
		mVector.emplace_back(elem3);
		mVector.emplace_back(elem4);
	}
	void push_back(const T& elem0, const T& elem1, const T& elem2, const T& elem3, const T& elem4, const T& elem5)
	{
		mSize += 6;
		mVector.reserve(mSize);
		mVector.emplace_back(elem0);
		mVector.emplace_back(elem1);
		mVector.emplace_back(elem2);
		mVector.emplace_back(elem3);
		mVector.emplace_back(elem4);
		mVector.emplace_back(elem5);
	}
	iterator erase(const iterator& iter)
	{
		iterator retIter = mVector.erase(iter);
		mSize = (int)mVector.size();
		return retIter;
	}
	iterator erase(const iterator& iter, const iterator& end)
	{
		iterator retIter = mVector.erase(iter, end);
		mSize = (int)mVector.size();
		return retIter;
	}
	iterator eraseAt(const int index)
	{
		if (index < 0 || index >= mSize)
		{
			return mVector.end();
		}
		iterator iter = mVector.erase(mVector.begin() + index);
		mSize = (int)mVector.size();
		return iter;
	}
	iterator eraseAt(const int index, const int count)
	{
		if (index < 0 || index >= mSize)
		{
			return mVector.end();
		}
		iterator iter = mVector.erase(mVector.begin() + index, mVector.begin() + (index + count));
		mSize = (int)mVector.size();
		return iter;
	}
	const T& eraseAtAndGet(const int index)
	{
		if (index < 0 || index >= mSize)
		{
			return mDefaultValue;
		}
		const T& value = mVector[index];
		mVector.erase(mVector.begin() + index);
		mSize = (int)mVector.size();
		return value;
	}
	void eraseLast()
	{
		if (mSize == 0)
		{
			return;
		}
		--mSize;
		mVector.pop_back();
	}
	bool eraseElement(const T& value)
	{
		FOR(mSize)
		{
			if (mVector[i] == value)
			{
				mVector.erase(mVector.begin() + i);
				--mSize;
				return true;
			}
		}
		return false;
	}
	int eraseAllElement(const T& value)
	{
		int eraseCount = 0;
		for (int i = mSize - 1; i >= 0; --i)
		{
			if (mVector[i] == value)
			{
				mVector.erase(mVector.begin() + i);
				++eraseCount;
			}
		}
		mSize -= eraseCount;
		return eraseCount;
	}
	// 将指定值的元素替换为默认值
	bool resetElement(const T& value)
	{
		FOR(mSize)
		{
			if (mVector[i] == value)
			{
				mVector[i]= mDefaultValue;
				return true;
			}
		}
		return false;
	}
	bool resetElementAt(const int index)
	{
		if (index < 0 || index >= mSize)
		{
			return false;
		}
		mVector[index] = mDefaultValue;
		return true;
	}
	void clearDefaultElement(int count)
	{
		for (int i = mSize - 1; i >= 0; --i)
		{
			if (mVector[i] == mDefaultValue)
			{
				mVector.erase(mVector.begin() + i);
				--mSize;
				if (--count == 0)
				{
					return;
				}
			}
		}
	}
	void clear(bool disposeMemory = false)
	{
		if (mSize > 0)
		{
			mSize = 0;
			mVector.clear();
		}
		if (disposeMemory)
		{
			dispose();
		}
	}
	void clearAndReserve(const int capacity)
	{
		clear();
		reserve(capacity);
	}
	void insert(const int index, const T& elem)
	{
		mVector.emplace(mVector.begin() + index, elem);
		++mSize;
	}
	void insert(const int index, T&& elem)
	{
		mVector.emplace(mVector.begin() + index, move(elem));
		++mSize;
	}
	void insert(const iterator& iter, const T& elem)
	{
		mVector.emplace(iter, elem);
		++mSize;
	}
	const T& operator[](const int i) const
	{
		if (i < 0 || i >= mSize)
		{
			ERROR("vector index out of range! index:" + to_string(i) + ", vector size:" + to_string(mSize));
		}
		return mVector[i];
	}
	// 根据下标获取元素,如果下标不合法则返回默认值
	const T& get(const int index) const
	{
		if (index < 0 || index >= mSize)
		{
			return mDefaultValue;
		}
		return mVector[index];
	}
	const T& get(const int index, const T& defaultValue) const
	{
		if (index < 0 || index >= mSize)
		{
			return defaultValue;
		}
		return mVector[index];
	}
	T& operator[](const int i)
	{
		if (i < 0 || i >= mSize)
		{
			ERROR("vector index out of range! index:" + to_string(i) + ", vector size:" + to_string(mSize));
		}
		return mVector[i];
	}
	// 同时修改capacity和size
	void resize(const int size)
	{
		mVector.resize(size, mDefaultValue);
		mSize = size;
	}
	// 只增加capacity,不改变当前size,如果当前的最大容量已经大于等于要设置的容量则不作任何事情
	void reserve(const int capacity)
	{
		if (capacity <= 0)
		{
			return;
		}
		mVector.reserve(capacity);
	}
	bool contains(const T& value) const
	{
		return mSize > 0 && std::find(mVector.begin(), mVector.end(), value) != mVector.end();
	}
	int findFirstIndex(const T& value, const int startIndex = 0) const
	{
		for(int i = startIndex; i < mSize; ++i)
		{
			if (mVector[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
	int findElementCount(const T& value) const
	{
		int elementCount = 0;
		for (int i = 0; i < mSize; ++i)
		{
			if (mVector[i] == value)
			{
				++elementCount;
			}
		}
		return elementCount;
	}
	void swapIndex(int index0, int index1)
	{
		T temp = move(mVector[index0]);
		mVector[index0] = move(mVector[index1]);
		mVector[index1] = move(temp);
	}
	// 添加克隆函数的目的是为了显式调用拷贝,而非自动调用拷贝,可以避免可以使用移动构造而没有使用的情况
	void clone(Vector<T>& target) const
	{
		target.mVector = mVector;
		target.mSize = mSize;
	}
	void shrink_to_fit()
	{
		mVector.shrink_to_fit();
	}
	void dispose()
	{
		vector<T> temp;
		mVector.swap(temp);
	}
public:
	vector<T> mVector;
	static const Vector<T> mDefaultList;
protected:
	int mSize = 0;			// 因为vector的size()非常耗时,所以单独使用一个变量记录元素数量,不过实测好像没有快多少
private:
	static const T mDefaultValue;
};

template<typename T>
const T Vector<T>::mDefaultValue = T();

template<typename T>
const Vector<T> Vector<T>::mDefaultList;