#ifndef _BINARY_UTILITY_H_
#define _BINARY_UTILITY_H_

#include "FrameDefine.h"
#include "Array.h"

class BinaryUtility : public FrameDefine
{
public:
	// 计算 16进制的c中1的个数
	static uint crc_check(char c);
	static ushort crc16(ushort crc, char* buffer, uint len, uint bufferOffset = 0);
	static ushort crc16_byte(ushort crc, byte data);
	static bool readBuffer(char* buffer, uint bufferSize, uint& index, char* dest, uint readSize);
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
	static bool writeColor(char* buffer, uint bufferSize, uint& index, Color& value, bool inverse = false);
	static void readVector2(char* buffer, uint bufferSize, uint& index, Vector2& value, bool inverse = false);
	static void readVector3(char* buffer, uint bufferSize, uint& index, Vector3& value, bool inverse = false);
	static void readVector4(char* buffer, uint bufferSize, uint& index, Vector4& value, bool inverse = false);
	static void readColor(char* buffer, uint bufferSize, uint& index, Color& value, bool inverse = false);
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
	static void setString(char* buffer, const char* str)
	{
		memcpy(buffer, str, strlen(str));
	}
	static void setString(char* buffer, const string& str)
	{
		memcpy(buffer, str.c_str(), str.length());
	}
	template<typename T>
	static void copyArray(T* dest, T* src, uint count)
	{
		memcpy(dest, src, count * sizeof(T));
	}
public:
	static const ushort crc16_table[256];
	// 各个基础数据类型的类型hash值
	static uint mStringType;
	static uint mBoolType;
	static uint mCharType;
	static uint mByteType;
	static uint mShortType;
	static uint mUShortType;
	static uint mIntType;
	static uint mUIntType;
	static uint mFloatType;
	static uint mLLongType;
	static uint mULLongType;
	static uint mBoolArrayType;
	static uint mCharArrayType;
	static uint mByteArrayType;
	static uint mShortArrayType;
	static uint mUShortArrayType;
	static uint mIntArrayType;
	static uint mUIntArrayType;
	static uint mFloatArrayType;
	static uint mULLongArrayType;
	static uint mBoolListType;
	static uint mCharListType;
	static uint mByteListType;
	static uint mShortListType;
	static uint mUShortListType;
	static uint mIntListType;
	static uint mUIntListType;
	static uint mFloatListType;
	static uint mULLongListType;
	static uint mVector2IntType;
	static uint mVector2UShortType;
	static uint mVector2ShortType;
};

#endif