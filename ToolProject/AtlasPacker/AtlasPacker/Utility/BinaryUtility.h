#ifndef _BINARY_UTILITY_H_
#define _BINARY_UTILITY_H_

#include "ServerDefine.h"

class BinaryUtility
{
public:
	// 计算 16进制的c中1的个数
	static uint crc_check(char c);
	static ushort crc16(ushort crc, char* buffer, uint len, uint bufferOffset = 0);
	static ushort crc16_byte(ushort crc, byte data);
	static bool readBuffer(char* buffer, uint bufferSize, uint& index, char* dest, uint destSize, uint readSize);
	static bool writeBuffer(char* buffer, uint bufferSize, uint& destOffset, char* source, uint writeSize);
	template<typename T>
	static T read(char* buffer, uint bufferSize, uint& index, bool inverse = false)
	{
		uint valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return 0;
		}
		T finalValue;
		char* ptr = (char*)&finalValue;
		FOR_I(valueSize)
		{
			uint byteOffset = inverse ? (valueSize - 1 - i) : i;
			ptr[byteOffset] = buffer[index++];
		}
		return finalValue;
	}
	template<typename T>
	static void readArray(char* buffer, uint bufferSize, uint& index, T* dest, uint arrayLength, bool inverse = false)
	{
		FOR_I(arrayLength)
		{
			dest[i] = read<T>(buffer, bufferSize, index, inverse);
		}
	}
	template<typename T>
	static bool write(char* buffer, uint bufferSize, uint& index, T value, bool inverse = false)
	{
		uint writeSize = sizeof(T);
		if (bufferSize < index + writeSize)
		{
			return false;
		}
		char* ptr = (char*)&value;
		FOR_I(writeSize)
		{
			uint byteOffset = inverse ? (writeSize - 1 - i) : i;
			buffer[index++] = ptr[byteOffset];
		}
		return true;
	}
	static bool writeVector2(char* buffer, uint bufferSize, uint& index, Vector2& value, bool inverse = false);
	static bool writeVector3(char* buffer, uint bufferSize, uint& index, Vector3& value, bool inverse = false);
	static bool writeVector4(char* buffer, uint bufferSize, uint& index, Vector4& value, bool inverse = false);
	static void readVector2(char* buffer, uint bufferSize, uint& index, Vector2& value, bool inverse = false);
	static void readVector3(char* buffer, uint bufferSize, uint& index, Vector3& value, bool inverse = false);
	static void readVector4(char* buffer, uint bufferSize, uint& index, Vector4& value, bool inverse = false);
	template<typename T>
	static bool writeArray(char* buffer, uint bufferSize, uint& index, T* valueArray, uint arrayLength, bool inverse = false)
	{
		FOR_I(arrayLength)
		{
			if (!write(buffer, bufferSize, index, valueArray[i], inverse))
			{
				return false;
			}
		}
		return true;
	}
	template<typename T>
	static void inverseByte(T& value)
	{
		uint typeSize = sizeof(T);
		int halfSize = typeSize >> 1;
		FOR_I(halfSize)
		{
			swapByte(value, i, typeSize - i - 1);
		}
	}
	template<typename T>
	static void swapByte(T& value, uint pos0, uint pos1)
	{
		char byte0 = GET_BYTE(value, pos0);
		char byte1 = GET_BYTE(value, pos1);
		SET_BYTE(value, byte0, pos1);
		SET_BYTE(value, byte1, pos0);
	}
	template<size_t Length>
	static void setString(array<char, Length>& buffer, const char* str, uint strLength = 0)
	{
		if (strLength == 0)
		{
			strLength = (uint)strlen(str);
		}
		MEMCPY(buffer.data(), Length, str, strLength);
		buffer[strLength] = '\0';
	}
	template<size_t Length>
	static void setString(array<char, Length>& buffer, const string& str)
	{
		uint length = (uint)str.length();
		MEMCPY(buffer.data(), Length, str.c_str(), length);
		buffer[length] = '\0';
	}
	static void setString(char* buffer, uint bufferSize, const char* str, uint strLength = 0)
	{
		if (strLength == 0)
		{
			strLength = (uint)strlen(str);
		}
		MEMCPY(buffer, bufferSize, str, strLength);
		buffer[strLength] = '\0';
	}
	static void setString(char* buffer, uint bufferSize, const string& str)
	{
		uint length = (uint)str.length();
		MEMCPY(buffer, bufferSize, str.c_str(), length);
		buffer[length] = '\0';
	}
	static void setString(char* buffer, uint offset, uint bufferSize, const char* str, uint strLength = 0)
	{
		if (strLength == 0)
		{
			strLength = (uint)strlen(str);
		}
		MEMCPY(buffer + offset, bufferSize - offset, str, strLength);
		buffer[offset + strLength] = '\0';
	}
	static void setString(char* buffer, uint offset, uint bufferSize, const string& str)
	{
		uint length = (uint)str.length();
		MEMCPY(buffer + offset, bufferSize - offset, str.c_str(), length);
		buffer[offset + length] = '\0';
	}
	// 拷贝两个普通数组
	template<typename T>
	static void copyArray(T* dest, uint destCount, T* src, uint count)
	{
		MEMCPY(dest, destCount * sizeof(T), src, count * sizeof(T));
	}
	// 拷贝array数组
	template<size_t Length, typename T>
	static void copyArray(array<T, Length>& dest, const array<T, Length>& src)
	{
		MEMCPY(dest.data(), Length * sizeof(T), src.data(), Length * sizeof(T));
	}
	template<size_t Length, typename T>
	static void copyArray(array<T, Length>& dest, T* src, uint count)
	{
		MEMCPY(dest.data(), Length * sizeof(T), src, count * sizeof(T));
	}
	// 将数组的内容重置为0
	template<size_t Length, typename T>
	static void zeroArray(array<T, Length>& arr)
	{
		ZERO_MEMORY(arr.data(), sizeof(arr[0]) * arr.size());
	}
	template<size_t Length, typename T>
	static void fillArray(array<T, Length>& arr, T value)
	{
		FOR_I(Length)
		{
			arr[i] = value;
		}
	}
public:
	static const ushort crc16_table[256];
	// 各个基础数据类型的类型hash值
	static const uint mStringType;
	static const uint mBoolType;
	static const uint mCharType;
	static const uint mByteType;
	static const uint mShortType;
	static const uint mUShortType;
	static const uint mIntType;
	static const uint mUIntType;
	static const uint mFloatType;
	static const uint mULLongType;
	static const uint mBoolsType;
	static const uint mCharsType;
	static const uint mBytesType;
	static const uint mShortsType;
	static const uint mUShortsType;
	static const uint mIntsType;
	static const uint mUIntsType;
	static const uint mFloatsType;
	static const uint mULLongsType;
	static const uint mBoolListType;
	static const uint mCharListType;
	static const uint mByteListType;
	static const uint mShortListType;
	static const uint mUShortListType;
	static const uint mIntListType;
	static const uint mUIntListType;
	static const uint mFloatListType;
	static const uint mULLongListType;
	static const uint mVector2IntType;
	static const uint mVector2UShortType;
};

#endif