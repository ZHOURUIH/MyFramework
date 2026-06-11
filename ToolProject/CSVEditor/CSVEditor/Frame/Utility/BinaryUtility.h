#pragma once

#include "TypeUtility.h"
#include "Vector.h"

namespace BinaryUtility
{
	// constexpr的定义只能写在头文件中,否则在其他地方使用时会提示计算结果不是常量,无法使定义结果为constexpr
	// 不过将定义写到头文件中带来的影响就是每个cpp文件中的常量地址是不一样的

	// 下标是sizeof(T),value是此T类型的bit长度所需要的长度表示
	// 比如1个char的取值范围是-127~127,去除符号以后是0~127,127占7个bit,可以使用3个bit存储这个7,所以长度位就是用3个bit来表示,所以下标1的值是3
	constexpr byte SIGNED_LENGTH_MAX_BIT[9]{ 0, 3, 4, 0, 5, 0, 0, 0, 6 };
	// 比如1个byte的取值范围是8~255,255占8个bit,可以使用4个bit存储这个8,所以长度位就是用4个bit来表示,所以下标1的值是4
	// 其作用于mBitCountTable类似,只不过不需要计算
	constexpr byte UNSIGNED_LENGTH_MAX_BIT[9]{ 0, 4, 5, 0, 6, 0, 0, 0, 7 };
	// 下标i上的值表示有i个位为1时的数值,比如下标2的3表示0b11,下标3的7表示0b111
	constexpr byte BIT_ARRAY[9]{ 0, 1, 3, 7, 15, 31, 63, 127, 255 };
	// 0到65535的每个数中的最高位1的下标,也就是需要用几个bit来表示,1可以用1个bit表示,5可以使用3个bit表示,8可以使用5个bit表示
	extern byte mBitCountTable[65536];
	extern const ushort crc16_table[256];
	// 各个基础数据类型的类型hash值
	extern const int mStringType;
	extern const int mBoolType;
	extern const int mCharType;
	extern const int mWCharType;
	extern const int mByteType;
	extern const int mShortType;
	extern const int mUShortType;
	extern const int mIntType;
	extern const int mUIntType;
	extern const int mFloatType;
	extern const int mLLongType;
	extern const int mULLongType;
	extern const int mBoolPtrType;
	extern const int mCharPtrType;
	extern const int mBytePtrType;
	extern const int mShortPtrType;
	extern const int mUShortPtrType;
	extern const int mIntPtrType;
	extern const int mUIntPtrType;
	extern const int mFloatPtrType;
	extern const int mLLongPtrType;
	extern const int mULLongPtrType;
	extern const int mBoolListType;
	extern const int mCharListType;
	extern const int mByteListType;
	extern const int mShortListType;
	extern const int mUShortListType;
	extern const int mIntListType;
	extern const int mUIntListType;
	extern const int mFloatListType;
	extern const int mLLongListType;
	extern const int mULLongListType;
	extern const int mVector2Type;
	extern const int mVector2IntType;
	extern const int mVector2UIntType;
	extern const int mVector2ShortType;
	extern const int mVector2UShortType;
	extern const int mVector3Type;
	extern const int mVector3IntType;
	extern const int mVector2ListType;
	extern const int mVector2IntListType;
	extern const int mVector3ListType;
	extern const int mVector3IntListType;

	//------------------------------------------------------------------------------------------------------------------------------
	// protected
	void initBitCountTable();
	byte internalGenerateBitCount(ushort value);
	//------------------------------------------------------------------------------------------------------------------------------
	// public
	// 因为当前类中需要使用过,如果写到FrameUtility中,会有头文件互相包含的问题,所以即使下面的函数于二进制操作无关,也写到这里
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline T getListMax(const Vector<T>& list)
	{
		const int count = list.size();
		if (count == 0)
		{
			return 0;
		}

		T maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			if (maxValue < list[i])
			{
				maxValue = list[i];
			}
		}
		return maxValue;
	}
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline T getListMax(const T* list, const int count)
	{
		if (count == 0)
		{
			return 0;
		}

		T maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			if (maxValue < list[i])
			{
				maxValue = list[i];
			}
		}
		return maxValue;
	}
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline T getListMaxAbs(const Vector<T>& list)
	{
		const int count = list.size();
		if (count == 0)
		{
			return 0;
		}

		T maxAbsValue = list[0] > 0 ? list[0] : -list[0];
		for (int i = 1; i < count; ++i)
		{
			T absValue = list[i] > 0 ? list[i] : -list[i];
			if (maxAbsValue < absValue)
			{
				maxAbsValue = absValue;
			}
		}
		return maxAbsValue;
	}
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline T getListMaxAbs(const T* list, const int count)
	{
		if (count == 0)
		{
			return 0;
		}

		T maxAbsValue = list[0] > 0 ? list[0] : -list[0];
		for (int i = 1; i < count; ++i)
		{
			T absValue = list[i] > 0 ? list[i] : -list[i];
			if (maxAbsValue < absValue)
			{
				maxAbsValue = absValue;
			}
		}
		return maxAbsValue;
	}
	float getFloatListMaxAbs(const float* list, int count);
	double getDoubleListMaxAbs(const double* list, int count);
	// 设置value的指定位置pos的字节的值为byte,并且不影响其他字节
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr void setByte(T& value, const byte b, const int pos) { value = (value & ~(0xFFLL << (8 * pos))) | (b << (8 * pos)); }
	// 获得value的指定位置pos的字节的值
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr byte getByte(T value, const int pos) { return ((value) & (0xFFLL << (8 * pos))) >> (8 * pos); }
	// 指定下标的位是否为1
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr bool hasBit(T value, const int pos) { return (value & (1LL << pos)) != 0; }
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr bool hasFlag(T value, const T flag) { return (value & flag) != 0; }
	// 获取指定位的值
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr int getBit(T value, const int pos) { return (value & (1LL << pos)) >> pos; }
	// 设置指定位的值
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr void setBit(T& value, const int pos, const int bit)
	{
		if (bit != 0)
		{
			value |= 1LL << pos;
		}
		else
		{
			value &= ~(1LL << pos);
		}
	}
	// 注意,负数由于存储是使用补码存储的,所以不能直接按位或
	// 将指定下标的位设置为1
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr void setBitOne(T& value, const int pos) { value |= 1LL << pos; }
	// 将指定下标的位设置为0
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr void setBitZero(T& value, const int pos) { value &= ~(1LL << pos); }
	// 获取最高位
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr int getHighestBit(T value) { return (value & (1LL << (sizeof(T) * 8 - 1))) >> (sizeof(T) * 8 - 1); }
	// 设置最高位
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr void setHighestBit(T& value, const bool bit)
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
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr int getLowestBit(T value) { return value & 1LL; }
	// 设置最低位
	template<typename T, typename TypeCheck = typename IsPodIntegerType<T>::mType>
	inline constexpr void setLowestBit(T& value, const bool bit)
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
	// 将value中截取从start开始的count个位的值
	inline constexpr byte getBits(const byte value, const byte start, const byte count) { return (value & (BIT_ARRAY[count] << start)) >> start; }
	// 将位的数量转换为字节的数量,比如9个bit就是占2个字节
	inline constexpr int bitCountToByteCount(const int bitCount) { return (bitCount & 7) != 0 ? (bitCount >> 3) + 1 : bitCount >> 3; }
	inline void setBufferBitOne(char* buffer, const int bitIndex) { setBitOne(buffer[bitIndex >> 3], bitIndex & 7); }
	inline bool getBufferBit(const char* buffer, const int bitIndex) { return hasBit(buffer[bitIndex >> 3], bitIndex & 7); }
	// 将sourceValue中sourceStart开始的writeBitCount个位写入到destValue从destStart开始的位下标上
	inline void copyBits(byte& destValue, const byte destStart, const byte sourceValue, const byte sourceStart, const byte writeBitCount)
	{
		destValue |= (getBits(sourceValue, sourceStart, writeBitCount) << destStart);
	}
	inline void copyBits(char& destValue, const byte destStart, const byte sourceValue, const byte sourceStart, const byte writeBitCount)
	{
		destValue |= (getBits(sourceValue, sourceStart, writeBitCount) << destStart);
	}
	inline constexpr void copyBits(byte& destValue, const byte destStart, const byte sourceValue) { destValue |= (sourceValue << destStart); }
	inline constexpr void copyBits(char& destValue, const byte destStart, const byte sourceValue) { destValue |= (sourceValue << destStart); }
	void logErrorInternal(string&& info);
	// 计算有效位数,也就是最高位1的下标+1,比如输入8,返回值就是4,意义是可以使用4个bit就可以表示数字8
	inline byte generateBitCount(char value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			logErrorInternal("无法计算负数的位数量");
			return 0;
		}
		return mBitCountTable[(byte)value];
	}
	inline byte generateBitCount(byte value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		return mBitCountTable[value];
	}
	inline byte generateBitCount(short value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			logErrorInternal("无法计算负数的位数量");
			return 0;
		}
		return mBitCountTable[value];
	}
	inline byte generateBitCount(ushort value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		return mBitCountTable[value];
	}
	inline byte generateBitCount(int value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			logErrorInternal("无法计算负数的位数量");
			return 0;
		}
		const ushort part1 = (ushort)((value & 0xFFFF0000) >> 16);
		return part1 > 0 ? mBitCountTable[part1] + 16 : mBitCountTable[value & 0x0000FFFF];
	}
	inline byte generateBitCount(uint value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		const ushort part1 = (ushort)((value & 0xFFFF0000) >> 16);
		return part1 > 0 ? mBitCountTable[part1] + 16 : mBitCountTable[value & 0x0000FFFF];
	}
	inline byte generateBitCount(llong value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			logErrorInternal("无法计算负数的位数量");
			return 0;
		}
		if ((value & 0xFFFFFFFF00000000) > 0)
		{
			const ushort part3 = (ushort)((value & 0xFFFF000000000000) >> 48);
			if (part3 > 0)
			{
				return mBitCountTable[part3] + 16 * 3;
			}
			return mBitCountTable[(ushort)((value & 0x0000FFFF00000000) >> 32)] + 16 * 2;
		}
		else
		{
			const ushort part1 = (ushort)((value & 0x00000000FFFF0000) >> 16);
			return part1 > 0 ? mBitCountTable[part1] + 16 * 1 : mBitCountTable[value & 0x000000000000FFFF];
		}
	}
	inline byte generateBitCount(ullong value)
	{
		if (mBitCountTable[1] == 0)
		{
			initBitCountTable();
		}
		if ((value & 0xFFFFFFFF00000000) > 0)
		{
			const ushort part3 = (ushort)((value & 0xFFFF000000000000) >> 48);
			if (part3 > 0)
			{
				return mBitCountTable[part3] + 16 * 3;
			}
			return mBitCountTable[(ushort)((value & 0x0000FFFF00000000) >> 32)] + 16 * 2;
		}
		else
		{
			const ushort part1 = (ushort)((value & 0x00000000FFFF0000) >> 16);
			return part1 > 0 ? mBitCountTable[part1] + 16 * 1 : mBitCountTable[value & 0x000000000000FFFF];
		}
	}
	// 从buffer的bitIndex的位下标处读取writeBitCount个位,位数最多不超过8位
	inline void readByteBits(const char* buffer, int& bitIndex, byte& value, const byte readBitCount)
	{
		value = ((*((short*)(buffer + (bitIndex >> 3)))) >> (bitIndex & 7)) & ((1 << readBitCount) - 1);
		bitIndex += readBitCount;
	}
	// 读取长度位
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool readUnsignedLengthBit(const char* buffer, const int bufferSize, int& bitIndex, byte& bitCount)
	{
		constexpr byte TYPE_LENGTH_MAX_BIT = UNSIGNED_LENGTH_MAX_BIT[sizeof(T)];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		bitCount = 0;
		readByteBits(buffer, bitIndex, bitCount, TYPE_LENGTH_MAX_BIT);
		return true;
	}
	// 读取长度位
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool readSignedLengthBit(const char* buffer, const int bufferSize, int& bitIndex, byte& bitCount)
	{
		constexpr byte TYPE_LENGTH_MAX_BIT = SIGNED_LENGTH_MAX_BIT[sizeof(T)];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		bitCount = 0;
		readByteBits(buffer, bitIndex, bitCount, TYPE_LENGTH_MAX_BIT);
		return true;
	}
	// 将一个字节的前writeBitCount个位写入到buffer的bitIndex的位下标处
	inline void writeByteBits(char* buffer, int& bitIndex, const byte value, const byte writeBitCount)
	{
		*((short*)(buffer + (bitIndex >> 3))) |= ((short)value & ((1 << writeBitCount) - 1)) << (bitIndex & 7);
		bitIndex += writeBitCount;
	}
	// 写入长度位
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool writeUnsignedLengthBit(char* buffer, const int bufferSize, int& bitIndex, const byte bitCount)
	{
		constexpr byte TYPE_LENGTH_MAX_BIT = UNSIGNED_LENGTH_MAX_BIT[sizeof(T)];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		// 先写入TYPE_LENGTH_MAX_BIT个bit
		writeByteBits(buffer, bitIndex, bitCount, TYPE_LENGTH_MAX_BIT);
		return true;
	}
	// 写入长度位
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool writeSignedLengthBit(char* buffer, const int bufferSize, int& bitIndex, const byte bitCount)
	{
		constexpr byte TYPE_LENGTH_MAX_BIT = SIGNED_LENGTH_MAX_BIT[sizeof(T)];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		// 先写入TYPE_LENGTH_MAX_BIT个bit
		writeByteBits(buffer, bitIndex, bitCount, TYPE_LENGTH_MAX_BIT);
		return true;
	}
	// 计算 16进制的c中1的个数
	constexpr int crc_check(const char c);
	ushort crc16(ushort crc, const char* buffer, int len, int bufferOffset = 0);
	inline ushort crc16_byte(ushort crc, byte data) { return (ushort)((crc >> 8) ^ crc16_table[(crc ^ data) & 0xFF]); }
	template<typename T>
	inline void zeroArray(T* ptr, int size) { memset(ptr, 0, sizeof(T) * size); }
	bool readBuffer(const char* buffer, int bufferSize, int& index, char* dest, int destSize, int readSize);
	bool writeBuffer(char* buffer, int bufferSize, int& destOffset, const char* source, int writeSize);
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr bool read(const char* buffer, const int bufferSize, int& index, T& value)
	{
		constexpr int valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return false;
		}
		value = *((T*)(buffer + index));
		index += valueSize;
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr T read(const char* buffer, const int bufferSize, int& index)
	{
		constexpr int valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return 0;
		}
		const T value = *((T*)(buffer + index));
		index += valueSize;
		return value;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr T readInverse(const char* buffer, const int bufferSize, int& index)
	{
		constexpr int valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return 0;
		}
		T finalValue = (T)0;
		char* ptr = (char*)&finalValue;
		FOR_INVERSE_I(valueSize)
		{
			ptr[i] = buffer[index++];
		}
		return finalValue;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr bool readInverse(const char* buffer, const int bufferSize, int& index, T& value)
	{
		constexpr int valueSize = sizeof(T);
		if (bufferSize < index + valueSize)
		{
			return 0;
		}
		char* ptr = (char*)&value;
		FOR_INVERSE_I(valueSize)
		{
			ptr[i] = buffer[index++];
		}
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr void readArray(const char* buffer, const int bufferSize, int& index, T* dest, const int arrayLength)
	{
		FOR(arrayLength)
		{
			dest[i] = read<T>(buffer, bufferSize, index);
		}
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr void readArrayInverse(const char* buffer, const int bufferSize, int& index, T* dest, const int arrayLength)
	{
		FOR(arrayLength)
		{
			dest[i] = readInverse<T>(buffer, bufferSize, index);
		}
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr bool write(char* buffer, const int bufferSize, int& index, const T value)
	{
		constexpr int writeSize = sizeof(T);
		if (bufferSize < index + writeSize)
		{
			return false;
		}
		*((T*)(buffer + index)) = value;
		index += writeSize;
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr bool writeInverse(char* buffer, const int bufferSize, int& index, const T value)
	{
		constexpr int writeSize = sizeof(T);
		if (bufferSize < index + writeSize)
		{
			return false;
		}
		const char* ptr = (char*)&value;
		FOR_INVERSE_I(writeSize)
		{
			buffer[index++] = ptr[i];
		}
		return true;
	}
	inline bool readBoolBit(const char* buffer, const int bufferSize, int& bitIndex, bool& value)
	{
		// bool固定读取1位
		if (bitCountToByteCount(bitIndex + 1) > bufferSize)
		{
			return false;
		}
		value = getBufferBit(buffer, bitIndex++);
		return true;
	}
	bool readBufferBit(const char* buffer, int bufferSize, int& bitIndex, char* dest, int destSize, int readSize);
	// 可以按位读取带符号的整数
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool readSignedIntegerBit(const char* buffer, int bufferSize, int& bitIndex, T& value)
	{
		value = 0;
		// 读取长度位
		byte bitCount = 0;
		if (!readSignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
		{
			return false;
		}
		if (bitCount == 0)
		{
			return true;
		}
		// 因为写入的时候,最高位的1是不需要写入的,所以实际读取的位数会少一位
		--bitCount;
		if (bitCountToByteCount(bitIndex + 1 + bitCount) > bufferSize)
		{
			return false;
		}

		// 读符号位
		const bool isNegative = getBufferBit(buffer, bitIndex++);

		// 读取值
		if (bitCount > 0)
		{
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(byteCount - 1)
			{
				readByteBits(buffer, bitIndex, ((byte*)&value)[i], 8);
			}
			readByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
			// 注意,负数由于存储是使用补码存储的,所以不能直接按位或
			// 读取完所有位再将最高位的1加上
			setBitOne(value, bitCount);
			if (isNegative)
			{
				value = -value;
			}
		}
		else
		{
			value = isNegative ? -1 : 1;
		}
		return true;
	}
	// 可以按位读取无符号的整数
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool readUnsignedIntegerBit(const char* buffer, const int bufferSize, int& bitIndex, T& value)
	{
		value = 0;
		// 读取长度位
		byte bitCount = 0;
		if (!readUnsignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
		{
			return false;
		}
		if (bitCount == 0)
		{
			return true;
		}
		// 因为写入的时候,最高位的1是不需要写入的,所以实际读取的位数会少一位
		--bitCount;
		if (bitCountToByteCount(bitIndex + bitCount) > bufferSize)
		{
			return false;
		}

		// 读取值
		if (bitCount > 0)
		{
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(byteCount - 1)
			{
				readByteBits(buffer, bitIndex, ((byte*)&value)[i], 8);
			}
			readByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
			// 读取完所有位再将最高位的1加上
			setBitOne(value, bitCount);
		}
		else
		{
			value = 1;
		}
		return true;
	}
	bool readFloatBit(const char* buffer, int bufferSize, int& bitIndex, float& value, const int precision = 3);
	bool readDoubleBit(const char* buffer, int bufferSize, int& bitIndex, double& value, const int precision = 4);
	template<typename T, int Count, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool readSignedIntegerListBit(const char* buffer, const int bufferSize, int& bitIndex, Array<Count, T*>& list)
	{
		// 先读取长度位是使用哪种方式写入的
		const bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 使用统一的长度位
		if (lengthBitType)
		{
			// 读取长度位
			byte bitCount = 0;
			if (!readSignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
			{
				return false;
			}
			if (bitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + (1 + bitCount) * Count) > bufferSize)
			{
				return false;
			}
			// 读取所有元素位
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(Count)
			{
				T& value = *list[i];
				value = 0;
				// 读符号位
				const bool isNegative = getBufferBit(buffer, bitIndex++);
				// 读取值
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&value)[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
				if (isNegative)
				{
					value = -value;
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			FOR(Count)
			{
				readSignedIntegerBit(buffer, bufferSize, bitIndex, *list[i]);
			}
		}
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool readSignedIntegerListBit(const char* buffer, const int bufferSize, int& bitIndex, Vector<T>& list)
	{
		// 读元素个数位
		int count = 0;
		if (!readSignedIntegerBit(buffer, bufferSize, bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		list.clear();
		list.resize(count);

		// 先读取长度位是使用哪种方式写入的
		const bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 使用统一的长度位
		if (lengthBitType)
		{
			// 读取长度位
			byte bitCount = 0;
			if (!readSignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
			{
				return false;
			}
			if (bitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + (1 + bitCount) * count) > bufferSize)
			{
				return false;
			}
			// 读取所有元素位
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(count)
			{
				T& value = list[i];
				value = 0;
				// 读符号位
				const bool isNegative = getBufferBit(buffer, bitIndex++);
				// 读取值
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&value)[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
				if (isNegative)
				{
					value = -value;
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			FOR(count)
			{
				readSignedIntegerBit(buffer, bufferSize, bitIndex, list[i]);
			}
		}
		return true;
	}
	template<typename T, int Count, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool readUnsignedIntegerListBit(const char* buffer, const int bufferSize, int& bitIndex, Array<Count, T*>& list)
	{
		// 先读取长度位是使用哪种方式写入的
		const bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 使用统一的长度位
		if (lengthBitType)
		{
			// 读取长度位
			byte bitCount = 0;
			if (!readUnsignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
			{
				return false;
			}
			if (bitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + bitCount * Count) > bufferSize)
			{
				return false;
			}
			// 读取所有元素位
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(Count)
			{
				T& value = *list[i];
				value = 0;
				// 读取值
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&value)[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			FOR(Count)
			{
				readUnsignedIntegerBit(buffer, bufferSize, bitIndex, *list[i]);
			}
		}
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool readUnsignedIntegerListBit(const char* buffer, const int bufferSize, int& bitIndex, Vector<T>& list)
	{
		// 读元素个数位
		int count = 0;
		if (!readSignedIntegerBit(buffer, bufferSize, bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		list.clear();
		list.resize(count);

		// 先读取长度位是使用哪种方式写入的
		const bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 使用统一的长度位
		if (lengthBitType)
		{
			// 读取长度位
			byte bitCount = 0;
			if (!readUnsignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
			{
				return false;
			}
			if (bitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + bitCount * count) > bufferSize)
			{
				return false;
			}
			// 读取所有元素位
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(count)
			{
				T& value = list[i];
				value = 0;
				// 读取值
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&value)[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			FOR(count)
			{
				readUnsignedIntegerBit(buffer, bufferSize, bitIndex, list[i]);
			}
		}
		return true;
	}
	bool readFloatListBit(const char* buffer, int bufferSize, int& bitIndex, Vector<float>& list, int precision = 3);
	bool readFloatListBit(const char* buffer, int bufferSize, int& bitIndex, float** list, int count, int precision = 3);
	template<int Count>
	inline bool readFloatListBit(const char* buffer, int bufferSize, int& bitIndex, Array<Count, float*>& list, int precision = 3)
	{
		return readFloatListBit(buffer, bufferSize, bitIndex, list.data(), Count, precision);
	}
	bool readDoubleListBit(const char* buffer, int bufferSize, int& bitIndex, Vector<double>& list, int precision = 4);
	bool readDoubleListBit(const char* buffer, int bufferSize, int& bitIndex, double** list, int count, int precision = 4);
	template<int Count>
	inline bool readDoubleListBit(const char* buffer, int bufferSize, int& bitIndex, Array<Count, double*>& list, int precision = 4)
	{
		return readDoubleListBit(buffer, bufferSize, bitIndex, list.data(), Count, precision);
	}
	inline bool writeBoolBit(char* buffer, const int bufferSize, int& bitIndex, bool value)
	{
		if (bitCountToByteCount(bitIndex + 1) > bufferSize)
		{
			return false;
		}
		// 固定只写入1位
		if (value)
		{
			setBufferBitOne(buffer, bitIndex);
		}
		++bitIndex;
		return true;
	}
	void fillZeroToByteEnd(char* buffer, int& bitIndex);
	bool writeBufferBit(char* buffer, int bufferSize, int& bitIndex, const char* source, int writeSize);
	// 可以按位写入带符号的整数
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool writeSignedIntegerBit(char* buffer, const int bufferSize, int& bitIndex, T value)
	{
		const bool isNegative = value < 0;
		if (isNegative)
		{
			value = -value;
		}
		// 需要多少位来存储这个值
		byte bitCount = generateBitCount(value);
		if ((value != 0 && bitCount == 0) || bitCount > sizeof(value) * 8)
		{
			return false;
		}
		if (!writeSignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
		{
			return false;
		}
		if (value == 0)
		{
			return true;
		}
		if (bitCountToByteCount(bitIndex + 1 + bitCount - 1) > bufferSize)
		{
			return false;
		}

		// 写入符号位
		if (isNegative)
		{
			setBufferBitOne(buffer, bitIndex);
		}
		++bitIndex;
		// 将最高位的1去掉,不需要写入
		setBitZero(value, --bitCount);
		if (bitCount > 0)
		{
			// 再写入值的所有位
			// 计算出要写多少个字节,按每个字节去写入
			const byte byteCount = bitCountToByteCount(bitCount);
			FOR(byteCount - 1)
			{
				writeByteBits(buffer, bitIndex, ((byte*)&value)[i], 8);
			}
			writeByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
		}
		return true;
	}
	// 可以按位写入无符号的整数
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool writeUnsignedIntegerBit(char* buffer, const int bufferSize, int& bitIndex, T value)
	{
		byte bitCount = generateBitCount(value);
		if ((value != 0 && bitCount == 0) || bitCount > sizeof(value) * 8)
		{
			return false;
		}
		if (!writeUnsignedLengthBit<T>(buffer, bufferSize, bitIndex, bitCount))
		{
			return false;
		}
		if (value == 0)
		{
			return true;
		}
		if (bitCountToByteCount(bitIndex + bitCount - 1) > bufferSize)
		{
			return false;
		}
		// 将最高位的1去掉,不需要写入
		setBitZero(value, --bitCount);
		if (bitCount > 0)
		{
			// 再写入值的所有位
			// 计算出要写多少个字节,按每个字节去写入
			const byte byteCount = bitCountToByteCount(bitCount);
			FOR(byteCount - 1)
			{
				writeByteBits(buffer, bitIndex, ((byte*)&value)[i], 8);
			}
			writeByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], bitCount - ((byteCount - 1) << 3));
		}
		return true;
	}
	bool writeFloatListBit(char* buffer, int bufferSize, int& bitIndex, const float* list, int count, bool needWriteCount, int precision = 3);
	template<int Count>
	inline bool writeFloatListBit(char* buffer, int bufferSize, int& bitIndex, const Array<Count, float>& list, int precision = 3)
	{
		return writeFloatListBit(buffer, bufferSize, bitIndex, list.data(), Count, false, precision);
	}
	inline bool writeFloatListBit(char* buffer, int bufferSize, int& bitIndex, const Vector<float>& list, int precision = 3)
	{
		return writeFloatListBit(buffer, bufferSize, bitIndex, list.data(), list.size(), true, precision);
	}
	bool writeDoubleListBit(char* buffer, int bufferSize, int& bitIndex, const double* list, int count, bool needWriteCount, int precision = 4);
	template<int Count>
	inline bool writeDoubleListBit(char* buffer, int bufferSize, int& bitIndex, const Array<Count, double>& list, int precision = 4)
	{
		return writeDoubleListBit(buffer, bufferSize, bitIndex, list.data(), Count, false, precision);
	}
	inline bool writeDoubleListBit(char* buffer, int bufferSize, int& bitIndex, const Vector<double>& list, int precision = 4)
	{
		return writeDoubleListBit(buffer, bufferSize, bitIndex, list.data(), list.size(), true, precision);
	}
	// 可以按位写入带符号的整数的列表
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool writeSignedIntegerListBit(char* buffer, const int bufferSize, int& bitIndex, const T* list, const int count, const bool needWriteCount)
	{
		// 写入长度
		if (needWriteCount && !writeSignedIntegerBit(buffer, bufferSize, bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 计算出最大值的位数量,所有的值都使用这个位数量来存储
		const byte maxBitCount = generateBitCount(getListMaxAbs(list, count));
		// 如果使用一个统一的位数来表示写入位个数所占用的总位数
		int bitCountUnity;
		if (maxBitCount > 0)
		{
			bitCountUnity = (maxBitCount + 1) * count + SIGNED_LENGTH_MAX_BIT[sizeof(T)];
		}
		else
		{
			bitCountUnity = SIGNED_LENGTH_MAX_BIT[sizeof(T)];
		}
		// 如果每个值都使用自己的实际位数来表示写入位个数所占用的总位数
		// 先加上每个元素的长度位的位数量
		int bitCountSingle = SIGNED_LENGTH_MAX_BIT[sizeof(T)] * count;
		// 写入独立长度所占空间小于统一个数时才会继续计算值所占的空间,如果长度部分已经大于了,则不需要再继续计算了
		if (bitCountSingle < bitCountUnity)
		{
			FOR(count)
			{
				T thisAbsValue = abs(list[i]);
				// 值为0时不会写入符号位和数据,不为0时才会写入
				if (thisAbsValue > 0)
				{
					// 每个元素绝对值所占用的位数,最高位固定是1,所以减去1位,然后还要加上一个符号位
					bitCountSingle += generateBitCount(thisAbsValue) - 1 + 1;
					if (bitCountSingle > bitCountUnity)
					{
						break;
					}
				}
			}
		}

		// 使用统一的长度位占空间更小
		if (bitCountSingle >= bitCountUnity)
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 先写入长度位
			if (!writeSignedLengthBit<T>(buffer, bufferSize, bitIndex, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + (maxBitCount + 1) * count) > bufferSize)
			{
				return false;
			}
			const byte byteCount = bitCountToByteCount(maxBitCount);
			// 再写入所有值的每一位
			FOR(count)
			{
				T value = list[i];
				// 写入符号位
				if (value < 0)
				{
					value = -value;
					setBufferBitOne(buffer, bitIndex);
				}
				++bitIndex;
				// 再写入值的所有位
				FOR_J(byteCount - 1)
				{
					writeByteBits(buffer, bitIndex, ((byte*)&value)[j], 8);
				}
				writeByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], maxBitCount - ((byteCount - 1) << 3));
			}
		}
		// 使用每个元素独立的长度位占用空间更小
		else
		{
			// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
			++bitIndex;
			FOR(count)
			{
				writeSignedIntegerBit(buffer, bufferSize, bitIndex, list[i]);
			}
		}
		return true;
	}
	// 可以按位写入带符号的整数的列表,Array类型
	template<typename T, int Count, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool writeSignedIntegerListBit(char* buffer, const int bufferSize, int& bitIndex, const Array<Count, T>& list)
	{
		return writeSignedIntegerListBit(buffer, bufferSize, bitIndex, list.data(), list.size(), false);
	}
	// 可以按位写入带符号的整数的列表,Vector类型
	template<typename T, typename TypeCheck = typename IsPodSignedIntegerType<T>::mType>
	inline bool writeSignedIntegerListBit(char* buffer, const int bufferSize, int& bitIndex, const Vector<T>& list)
	{
		return writeSignedIntegerListBit(buffer, bufferSize, bitIndex, list.data(), list.size(), true);
	}
	// 可以按位写入无符号的整数的列表
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool writeUnsignedIntegerListBit(char* buffer, const int bufferSize, int& bitIndex, const T* list, const int count, const bool needWriteCount)
	{
		// 写入长度
		if (needWriteCount && !writeSignedIntegerBit(buffer, bufferSize, bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 计算出最大值的位数量,所有的值都使用这个位数量来存储
		const byte maxBitCount = generateBitCount(getListMax(list, count));
		// 如果使用一个统一的位数来表示写入位个数所占用的总位数
		const int bitCountUnity = maxBitCount * count + UNSIGNED_LENGTH_MAX_BIT[sizeof(T)];
		// 如果每个值都使用自己的实际位数来表示写入位个数所占用的总位数
		// 先加上每个元素的长度位的位数量
		int bitCountSingle = UNSIGNED_LENGTH_MAX_BIT[sizeof(T)] * count;
		// 写入独立长度所占空间小于统一个数时才会继续计算值所占的空间,如果长度部分已经大于了,则不需要再继续计算了
		if (bitCountSingle < bitCountUnity)
		{
			// 每个元素绝对值所占用的位数,最高位固定是1,所以减去1位
			FOR(count)
			{
				T thisValue = list[i];
				if (thisValue > 0)
				{
					bitCountSingle += generateBitCount(thisValue) - 1;
					if (bitCountSingle > bitCountUnity)
					{
						break;
					}
				}
			}
		}

		// 使用统一的长度位占空间更小
		if (bitCountSingle >= bitCountUnity)
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 先写入长度位
			if (!writeUnsignedLengthBit<T>(buffer, bufferSize, bitIndex, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + maxBitCount * count) > bufferSize)
			{
				return false;
			}
			const byte byteCount = bitCountToByteCount(maxBitCount);
			FOR(count)
			{
				const T& value = list[i];
				// 写入值的所有位
				FOR_J(byteCount - 1)
				{
					writeByteBits(buffer, bitIndex, ((byte*)&value)[j], 8);
				}
				writeByteBits(buffer, bitIndex, ((byte*)&value)[byteCount - 1], maxBitCount - ((byteCount - 1) << 3));
			}
		}
		// 使用每个元素独立的长度位占用空间更小
		else
		{
			// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
			++bitIndex;
			FOR(count)
			{
				writeUnsignedIntegerBit(buffer, bufferSize, bitIndex, list[i]);
			}
		}
		return true;
	}
	// 可以按位写入无符号的整数的列表,Array类型
	template<typename T, int Count, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool writeUnsignedIntegerListBit(char* buffer, const int bufferSize, int& bitIndex, const Array<Count, T>& list)
	{
		return writeUnsignedIntegerListBit(buffer, bufferSize, bitIndex, list.data(), list.size(), false);
	}
	// 可以按位写入无符号的整数的列表,Vector类型
	template<typename T, typename TypeCheck = typename IsPodUnsignedIntegerType<T>::mType>
	inline bool writeUnsignedIntegerListBit(char* buffer, const int bufferSize, int& bitIndex, const Vector<T>& list)
	{
		return writeUnsignedIntegerListBit(buffer, bufferSize, bitIndex, list.data(), list.size(), true);
	}
	bool writeVector2(char* buffer, int bufferSize, int& index, const Vector2& value);
	bool writeVector2Inverse(char* buffer, int bufferSize, int& index, const Vector2& value);
	bool writeVector2Int(char* buffer, int bufferSize, int& index, const Vector2Int& value);
	bool writeVector2IntInverse(char* buffer, int bufferSize, int& index, const Vector2Int& value);
	bool writeVector3(char* buffer, int bufferSize, int& index, const Vector3& value);
	bool writeVector3Inverse(char* buffer, int bufferSize, int& index, const Vector3& value);
	bool writeVector4(char* buffer, int bufferSize, int& index, const Vector4& value);
	bool writeVector4Inverse(char* buffer, int bufferSize, int& index, const Vector4& value);
	bool writeColor(char* buffer, int bufferSize, int& index, const Color& value);
	bool writeColorInverse(char* buffer, int bufferSize, int& index, const Color& value);
	bool readVector2(const char* buffer, int bufferSize, int& index, Vector2& value);
	bool readVector2Inverse(const char* buffer, int bufferSize, int& index, Vector2& value);
	bool readVector2Int(const char* buffer, int bufferSize, int& index, Vector2Int& value);
	bool readVector2IntInverse(const char* buffer, int bufferSize, int& index, Vector2Int& value);
	bool readVector3(const char* buffer, int bufferSize, int& index, Vector3& value);
	bool readVector3Inverse(const char* buffer, int bufferSize, int& index, Vector3& value);
	bool readVector4(const char* buffer, int bufferSize, int& index, Vector4& value);
	bool readVector4Inverse(const char* buffer, int bufferSize, int& index, Vector4& value);
	bool readColor(const char* buffer, int bufferSize, int& index, Color& value);
	bool readColorInverse(const char* buffer, int bufferSize, int& index, Color& value);
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr bool writeArray(char* buffer, const int bufferSize, int& index, T* valueArray, const int arrayLength)
	{
		FOR(arrayLength)
		{
			if (!write(buffer, bufferSize, index, valueArray[i]))
			{
				return false;
			}
		}
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr bool writeArrayInverse(char* buffer, const int bufferSize, int& index, T* valueArray, const int arrayLength)
	{
		FOR(arrayLength)
		{
			if (!writeInverse(buffer, bufferSize, index, valueArray[i]))
			{
				return false;
			}
		}
		return true;
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr void inverseByte(T& value)
	{
		constexpr int typeSize = sizeof(T);
		constexpr int halfSize = typeSize >> 1;
		FOR(halfSize)
		{
			swapByte(value, i, typeSize - 1 - i);
		}
	}
	template<typename T, typename TypeCheck = typename IsPodType<T>::mType>
	inline constexpr void swapByte(T& value, const int pos0, const int pos1)
	{
		const char byte0 = getByte(value, pos0);
		const char byte1 = getByte(value, pos1);
		setByte(value, byte0, pos1);
		setByte(value, byte1, pos0);
	}
	inline void setString(char* buffer, const int bufferSize, const string& str)
	{
		const int length = (int)str.length();
		MEMCPY(buffer, bufferSize, str.c_str(), length);
		buffer[length] = '\0';
	}
}

using BinaryUtility::hasBit;
using BinaryUtility::hasFlag;
using BinaryUtility::setBitOne;
using BinaryUtility::setBitZero;
using BinaryUtility::getByte;
using BinaryUtility::bitCountToByteCount;
using BinaryUtility::setBit;
using BinaryUtility::getBit;
using BinaryUtility::setString;
using BinaryUtility::crc16;
using BinaryUtility::writeBuffer;
using BinaryUtility::readBuffer;
using BinaryUtility::writeVector2;
using BinaryUtility::writeVector3;
using BinaryUtility::writeVector4;
using BinaryUtility::writeBoolBit;
using BinaryUtility::writeSignedIntegerBit;
using BinaryUtility::writeUnsignedIntegerBit;
using BinaryUtility::writeBufferBit;
using BinaryUtility::writeSignedIntegerListBit;
using BinaryUtility::writeUnsignedIntegerListBit;
using BinaryUtility::writeFloatListBit;
using BinaryUtility::writeDoubleListBit;
using BinaryUtility::readBoolBit;
using BinaryUtility::readBufferBit;
using BinaryUtility::readSignedIntegerBit;
using BinaryUtility::readUnsignedIntegerBit;
using BinaryUtility::readSignedIntegerListBit;
using BinaryUtility::readUnsignedIntegerListBit;
using BinaryUtility::readFloatBit;
using BinaryUtility::readDoubleBit;
using BinaryUtility::readFloatListBit;
using BinaryUtility::readDoubleListBit;
using BinaryUtility::mStringType;
using BinaryUtility::mBoolType;
using BinaryUtility::mCharType;
using BinaryUtility::mWCharType;
using BinaryUtility::mByteType;
using BinaryUtility::mShortType;
using BinaryUtility::mUShortType;
using BinaryUtility::mIntType;
using BinaryUtility::mUIntType;
using BinaryUtility::mFloatType;
using BinaryUtility::mLLongType;
using BinaryUtility::mULLongType;
using BinaryUtility::mBoolPtrType;
using BinaryUtility::mCharPtrType;
using BinaryUtility::mBytePtrType;
using BinaryUtility::mShortPtrType;
using BinaryUtility::mUShortPtrType;
using BinaryUtility::mIntPtrType;
using BinaryUtility::mUIntPtrType;
using BinaryUtility::mFloatPtrType;
using BinaryUtility::mLLongPtrType;
using BinaryUtility::mULLongPtrType;
using BinaryUtility::mBoolListType;
using BinaryUtility::mCharListType;
using BinaryUtility::mByteListType;
using BinaryUtility::mShortListType;
using BinaryUtility::mUShortListType;
using BinaryUtility::mIntListType;
using BinaryUtility::mUIntListType;
using BinaryUtility::mFloatListType;
using BinaryUtility::mLLongListType;
using BinaryUtility::mULLongListType;
using BinaryUtility::mVector2Type;
using BinaryUtility::mVector2IntType;
using BinaryUtility::mVector2UIntType;
using BinaryUtility::mVector2ShortType;
using BinaryUtility::mVector2UShortType;
using BinaryUtility::mVector3Type;
using BinaryUtility::mVector3IntType;
using BinaryUtility::mVector2ListType;
using BinaryUtility::mVector2IntListType;
using BinaryUtility::mVector3ListType;
using BinaryUtility::mVector3IntListType;