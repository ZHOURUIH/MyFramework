#pragma once

#include "FrameDefine.h"

// 是否为大于等于0的数
template<int N>
struct IsPositive
{
	static constexpr bool mValue = N > 0;
	using mType = enable_if_t<N >= 0>;
};

// 判断是否为基础数据类型
template<typename T>
struct IsPod { static constexpr bool mValue = false; };

template<>
struct IsPod<bool> { static constexpr bool mValue = true; };

template<>
struct IsPod<char> { static constexpr bool mValue = true; };

template<>
struct IsPod<byte> { static constexpr bool mValue = true; };

template<>
struct IsPod<short> { static constexpr bool mValue = true; };

template<>
struct IsPod<ushort> { static constexpr bool mValue = true; };

template<>
struct IsPod<int> { static constexpr bool mValue = true; };

template<>
struct IsPod<uint> { static constexpr bool mValue = true; };

template<>
struct IsPod<long> { static constexpr bool mValue = true; };

template<>
struct IsPod<ulong> { static constexpr bool mValue = true; };

template<>
struct IsPod<llong> { static constexpr bool mValue = true; };

template<>
struct IsPod<ullong> { static constexpr bool mValue = true; };

template<>
struct IsPod<float> { static constexpr bool mValue = true; };

template<>
struct IsPod<double> { static constexpr bool mValue = true; };

template<typename T>
struct IsPodType { using mType = enable_if_t<IsPod<typename std::remove_cv<T>::type>::mValue>; };

// 是否为指针类型
template<typename T>
struct IsPointer
{
	static constexpr bool mValue = false;
};

template<typename T>
struct IsPointer<T*>
{
	static constexpr bool mValue = true;
};

// 是否为指针或者基础数据类型
template<typename T>
struct IsPodOrPointerType 
{
	static constexpr bool mValue = IsPod<T>::mValue || IsPointer<T>::mValue;
	using mType = enable_if_t<mValue>; 
};

// 是否不是指针和基础数据类型
template<typename T>
struct IsNotPodAndPointerType
{
	static constexpr bool mValue = !IsPod<T>::mValue && !IsPointer<T>::mValue;
	using mType = enable_if_t<mValue>;
};

// 是否为整型
template<typename T>
struct IsPodInteger { static constexpr bool mValue = false; };

template<>
struct IsPodInteger<bool> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<char> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<byte> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<short> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<ushort> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<int> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<uint> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<long> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<ulong> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<llong> { static constexpr bool mValue = true; };

template<>
struct IsPodInteger<ullong> { static constexpr bool mValue = true; };

template<typename T>
struct IsPodIntegerType { using mType = enable_if_t<IsPodInteger<T>::mValue>; };

// 是否为带符号整型
template<typename T>
struct IsPodSignedInteger { static constexpr bool mValue = false; };

template<>
struct IsPodSignedInteger<char> { static constexpr bool mValue = true; };

template<>
struct IsPodSignedInteger<short> { static constexpr bool mValue = true; };

template<>
struct IsPodSignedInteger<int> { static constexpr bool mValue = true; };

template<>
struct IsPodSignedInteger<long> { static constexpr bool mValue = true; };

template<>
struct IsPodSignedInteger<llong> { static constexpr bool mValue = true; };

template<typename T>
struct IsPodSignedIntegerType { using mType = enable_if_t<IsPodSignedInteger<T>::mValue>; };

// 是否为无符号整型
template<typename T>
struct IsPodUnsignedInteger { static constexpr bool mValue = false; };

template<>
struct IsPodUnsignedInteger<byte> { static constexpr bool mValue = true; };

template<>
struct IsPodUnsignedInteger<ushort> { static constexpr bool mValue = true; };

template<>
struct IsPodUnsignedInteger<uint> { static constexpr bool mValue = true; };

template<>
struct IsPodUnsignedInteger<ulong> { static constexpr bool mValue = true; };

template<>
struct IsPodUnsignedInteger<ullong> { static constexpr bool mValue = true; };

template<typename T>
struct IsPodUnsignedIntegerType { using mType = enable_if_t<IsPodUnsignedInteger<T>::mValue>; };