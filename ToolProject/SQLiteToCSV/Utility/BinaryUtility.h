#ifndef _BINARY_UTILITY_H_
#define _BINARY_UTILITY_H_

#include "FrameDefine.h"

class BinaryUtility : public FrameDefine
{
public:
	// T0是否与T1的类型一致,T0可以带const,&,volatile,在判断时会忽略这些修饰符
	template<typename T0, typename T1>
	static constexpr bool isType()
	{
		return is_same<typename decay<T0>::type, T1>();
	}
	// 计算 16进制的c中1的个数
	static constexpr uint crc_check(char c);
	static ushort crc16(ushort crc, char* buffer, uint len, uint bufferOffset = 0);
	static ushort crc16_byte(ushort crc, byte data);
	static bool readBuffer(const char* buffer, uint bufferSize, uint& index, char* dest, uint destSize, uint readSize);
	static bool writeBuffer(char* buffer, uint bufferSize, uint& destOffset, char* source, uint writeSize);
	template<typename T>
	static constexpr bool read(char* buffer, uint bufferSize, uint& index, T& value)
	{
		constexpr uint valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return false;
		}
		value = *((T*)(buffer + index));
		index += valueSize;
		return true;
	}
	template<typename T>
	static constexpr T read(char* buffer, uint bufferSize, uint& index)
	{
		constexpr uint valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return 0;
		}
		T value = *((T*)(buffer + index));
		index += valueSize;
		return value;
	}
	template<typename T>
	static constexpr T readInverse(const char* buffer, uint bufferSize, uint& index)
	{
		constexpr uint valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return 0;
		}
		T finalValue;
		char* ptr = (char*)&finalValue;
		FOR_INVERSE_I(valueSize)
		{
			ptr[i] = buffer[index];
			++index;
		}
		return finalValue;
	}
	template<typename T>
	static constexpr void readArray(char* buffer, uint bufferSize, uint& index, T* dest, uint arrayLength)
	{
		FOR_I(arrayLength)
		{
			dest[i] = read<T>(buffer, bufferSize, index);
		}
	}
	template<typename T>
	static constexpr void readArrayInverse(char* buffer, uint bufferSize, uint& index, T* dest, uint arrayLength)
	{
		FOR_I(arrayLength)
		{
			dest[i] = readInverse<T>(buffer, bufferSize, index);
		}
	}
	template<typename T>
	static constexpr bool write(char* buffer, uint bufferSize, uint& index, T value)
	{
		constexpr uint writeSize = sizeof(T);
		if (bufferSize < index + writeSize)
		{
			return false;
		}
		*((T*)(buffer + index)) = value;
		index += writeSize;
		return true;
	}
	template<typename T>
	static constexpr bool writeInverse(char* buffer, uint bufferSize, uint& index, T value)
	{
		constexpr uint writeSize = sizeof(T);
		if (bufferSize < index + writeSize)
		{
			return false;
		}
		char* ptr = (char*)&value;
		FOR_INVERSE_I(writeSize)
		{
			buffer[index++] = ptr[i];
		}
		return true;
	}
	template<typename T>
	static constexpr bool writeArray(char* buffer, uint bufferSize, uint& index, T* valueArray, uint arrayLength)
	{
		FOR_I(arrayLength)
		{
			if (!write(buffer, bufferSize, index, valueArray[i]))
			{
				return false;
			}
		}
		return true;
	}
	template<typename T>
	static constexpr bool writeArrayInverse(char* buffer, uint bufferSize, uint& index, T* valueArray, uint arrayLength)
	{
		FOR_I(arrayLength)
		{
			if (!writeInverse(buffer, bufferSize, index, valueArray[i]))
			{
				return false;
			}
		}
		return true;
	}
	template<typename T>
	static constexpr void inverseByte(T& value)
	{
		uint typeSize = sizeof(T);
		uint halfSize = typeSize >> 1;
		FOR_I(halfSize)
		{
			swapByte(value, i, typeSize - 1 - i);
		}
	}
	template<typename T>
	static constexpr void swapByte(T& value, uint pos0, uint pos1)
	{
		char byte0 = getByte(value, pos0);
		char byte1 = getByte(value, pos1);
		setByte(value, byte0, pos1);
		setByte(value, byte1, pos0);
	}
	static void setString(char* buffer, uint bufferSize, const string& str)
	{
		uint length = (uint)str.length();
		MEMCPY(buffer, bufferSize, str.c_str(), length);
		buffer[length] = '\0';
	}
	// 设置value的指定位置pos的字节的值为byte,并且不影响其他字节
	template<typename T>
	static constexpr void setByte(T& value, byte b, int pos) { value = (value & ~(0xFFLL << (8 * pos))) | (b << (8 * pos)); }
	// 获得value的指定位置pos的字节的值
	template<typename T>
	static constexpr byte getByte(const T& value, int pos) { return ((value) & (0xFFLL << (8 * pos))) >> (8 * pos); }
	// 指定下标的位是否为1
	template<typename T>
	static constexpr bool hasBit(const T& value, int pos) { return (value & (1LL << pos)) != 0; }
	// 获取指定位的值
	template<typename T>
	static constexpr int getBit(const T& value, int pos) { return (value & (1LL << pos)) >> pos; }
	// 设置指定位的值
	template<typename T>
	static constexpr void setBit(T& value, int pos, bool bit)
	{
		if (bit)
		{
			value |= 1LL << pos;
		}
		else
		{
			value &= ~(1LL << pos);
		}
	}
	// 将指定下标的位设置为1
	template<typename T>
	static constexpr void setBitOne(T& value, int pos) { value |= 1LL << pos; }
	// 将指定下标的位设置为0
	template<typename T>
	static constexpr void setBitZero(T& value, int pos) { value &= ~(1LL << pos); }
	// 获取最高位
	template<typename T>
	static constexpr int getHighestBit(const T& value) { return (value & (1LL << (sizeof(T) * 8 - 1))) >> (sizeof(T) * 8 - 1); }
	// 设置最高位
	template<typename T>
	static constexpr void setHighestBit(T& value, bool bit) 
	{
		if (bit)
		{
			value |= 1LL << (sizeof(T) * 8 - 1);
		}
		else
		{
			value &= ~(1LL << (sizeof(T) * 8 - 1));
		}
	}
	// 获取最低位
	template<typename T>
	static constexpr int getLowestBit(const T& value) { return value & 1LL; }
	// 设置最低位
	template<typename T>
	static constexpr void setLowestBit(T& value, bool bit)
	{
		if (bit)
		{
			value |= 1LL;
		}
		else
		{
			value &= ~1LL;
		}
	}
public:
	static const ushort crc16_table[256];
	// 各个基础数据类型的类型hash值
	static const uint mStringType;
	static const uint mBoolType;
	static const uint mCharType;
	static const uint mWCharType;
	static const uint mByteType;
	static const uint mShortType;
	static const uint mUShortType;
	static const uint mIntType;
	static const uint mUIntType;
	static const uint mFloatType;
	static const uint mLLongType;
	static const uint mULLongType;
	static const uint mBoolsType;
	static const uint mCharsType;
	static const uint mBytesType;
	static const uint mShortsType;
	static const uint mUShortsType;
	static const uint mIntsType;
	static const uint mUIntsType;
	static const uint mFloatsType;
	static const uint mLLongsType;
	static const uint mULLongsType;
	static const uint mBoolListType;
	static const uint mCharListType;
	static const uint mByteListType;
	static const uint mShortListType;
	static const uint mUShortListType;
	static const uint mIntListType;
	static const uint mUIntListType;
	static const uint mFloatListType;
	static const uint mLLongListType;
	static const uint mULLongListType;
};

#endif