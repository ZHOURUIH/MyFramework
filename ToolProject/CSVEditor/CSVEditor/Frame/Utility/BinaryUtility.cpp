#include "FrameHeader.h"

namespace BinaryUtility
{
	const int mStringType = (int)typeid(string).hash_code();
	const int mBoolType = (int)typeid(bool).hash_code();
	const int mCharType = (int)typeid(char).hash_code();
	const int mWCharType = (int)typeid(wchar_t).hash_code();
	const int mByteType = (int)typeid(unsigned char).hash_code();
	const int mShortType = (int)typeid(short).hash_code();
	const int mUShortType = (int)typeid(ushort).hash_code();
	const int mIntType = (int)typeid(int).hash_code();
	const int mUIntType = (int)typeid(uint).hash_code();
	const int mFloatType = (int)typeid(float).hash_code();
	const int mLLongType = (int)typeid(llong).hash_code();
	const int mULLongType = (int)typeid(ullong).hash_code();
	const int mBoolsType = (int)typeid(bool*).hash_code();
	const int mCharsType = (int)typeid(char*).hash_code();
	const int mBytesType = (int)typeid(byte*).hash_code();
	const int mShortPtrType = (int)typeid(short*).hash_code();
	const int mUShortPtrType = (int)typeid(ushort*).hash_code();
	const int mIntPtrType = (int)typeid(int*).hash_code();
	const int mUIntPtrType = (int)typeid(uint*).hash_code();
	const int mFloatPtrType = (int)typeid(float*).hash_code();
	const int mLLongPtrType = (int)typeid(llong*).hash_code();
	const int mULLongPtrType = (int)typeid(ullong*).hash_code();
	const int mBoolListType = (int)typeid(Vector<bool>).hash_code();
	const int mCharListType = (int)typeid(Vector<char>).hash_code();
	const int mByteListType = (int)typeid(Vector<byte>).hash_code();
	const int mShortListType = (int)typeid(Vector<short>).hash_code();
	const int mUShortListType = (int)typeid(Vector<ushort>).hash_code();
	const int mIntListType = (int)typeid(Vector<int>).hash_code();
	const int mUIntListType = (int)typeid(Vector<uint>).hash_code();
	const int mFloatListType = (int)typeid(Vector<float>).hash_code();
	const int mLLongListType = (int)typeid(Vector<llong>).hash_code();
	const int mULLongListType = (int)typeid(Vector<ullong>).hash_code();
	const int mVector2Type = (int)typeid(Vector2).hash_code();
	const int mVector3Type = (int)typeid(Vector3).hash_code();
	const int mVector2IntType = (int)typeid(Vector2Int).hash_code();
	const int mVector3IntType = (int)typeid(Vector3Int).hash_code();
	const int mVector2UIntType = (int)typeid(Vector2UInt).hash_code();
	const int mVector2ShortType = (int)typeid(Vector2Short).hash_code();
	const int mVector2UShortType = (int)typeid(Vector2UShort).hash_code();
	const int mVector2ListType = (int)typeid(Vector<Vector2>).hash_code();
	const int mVector2IntListType = (int)typeid(Vector<Vector2Int>).hash_code();
	const int mVector3ListType = (int)typeid(Vector<Vector3>).hash_code();
	const int mVector3IntListType = (int)typeid(Vector<Vector3Int>).hash_code();
	byte mBitCountTable[65536];
	const ushort crc16_table[256]
	{
		0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
		0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
		0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
		0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
		0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
		0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
		0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
		0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
		0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
		0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
		0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
		0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
		0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
		0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
		0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
		0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
		0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
		0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
		0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
		0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
		0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
		0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
		0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
		0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
		0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
		0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
		0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
		0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
		0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
		0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
		0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
		0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
	};
	// 计算 16进制的c中1的个数,此处可使用查表法进行优化,但是由于几乎没有哪个地方在用,所以不做实际修改
	constexpr int crc_check(const char c)
	{
		int count = 0;
		constexpr int bitCount = sizeof(char) * 8;
		FOR(bitCount)
		{
			if ((c & (1 << i)) > 0)
			{
				++count;
			}
		}
		return count;
	}

	ushort crc16(ushort crc, const char* buffer, const int len, const int bufferOffset)
	{
		FOR(len)
		{
			crc = crc16_byte(crc, buffer[bufferOffset + i]);
		}
		return crc;
	}

	bool readBuffer(const char* buffer, const int bufferSize, int& index, char* dest, const int destSize, const int readSize)
	{
		if (bufferSize < index + readSize)
		{
			return false;
		}
		if (readSize > 0)
		{
			MEMCPY(dest, destSize, buffer + index, readSize);
			index += readSize;
		}
		return true;
	}

	bool readFloatBit(const char* buffer, const int bufferSize, int& bitIndex, float& value, const int precision)
	{
		// 浮点数按照一定精度转换为整数再写入
		int intValue = 0;
		const bool result = readSignedIntegerBit(buffer, bufferSize, bitIndex, intValue);
		value = intValue * inversePow10(precision);
		return result;
	}

	bool readDoubleBit(const char* buffer, const int bufferSize, int& bitIndex, double& value, const int precision)
	{
		// 浮点数按照一定精度转换为整数再写入
		llong llongValue = 0;
		const bool result = readSignedIntegerBit(buffer, bufferSize, bitIndex, llongValue);
		value = llongValue * inversePow10LLong(precision);
		return result;
	}

	bool readFloatListBit(const char* buffer, int bufferSize, int& bitIndex, Vector<float>& list, const int precision)
	{
		// 读取长度
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
		const float powValue = inversePow10(precision);
		// 使用统一的长度位
		if (lengthBitType)
		{
			// 读取位数量
			byte bitCount = 0;
			if (!readSignedLengthBit<int>(buffer, bufferSize, bitIndex, bitCount))
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

			// 读取所有的值
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(count)
			{
				// 读符号位
				const bool isNegative = getBufferBit(buffer, bitIndex++);

				// 读取值
				int intValue = 0;
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&(intValue))[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&(intValue))[byteCount - 1], bitCount - ((byteCount - 1) << 3));
				list[i] = isNegative ? -intValue * powValue : intValue * powValue;
			}
		}
		else
		{
			FOR(count)
			{
				int value = 0;
				readSignedIntegerBit(buffer, bufferSize, bitIndex, value);
				list[i] = value * powValue;
			}
		}
		return true;
	}

	bool readFloatListBit(const char* buffer, int bufferSize, int& bitIndex, float** list, const int count, const int precision)
	{
		if (count == 0)
		{
			return true;
		}

		// 先读取长度位是使用哪种方式写入的
		const bool lengthBitType = getBufferBit(buffer, bitIndex++);
		const float powValue = inversePow10(precision);
		// 使用统一的长度位
		if (lengthBitType)
		{
			// 读取位数量
			byte bitCount = 0;
			if (!readSignedLengthBit<int>(buffer, bufferSize, bitIndex, bitCount))
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

			// 读取所有的值
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(count)
			{
				// 读符号位
				const bool isNegative = getBufferBit(buffer, bitIndex++);

				// 读取值
				int intValue = 0;
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&(intValue))[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&(intValue))[byteCount - 1], bitCount - ((byteCount - 1) << 3));
				*list[i] = isNegative ? -intValue * powValue : intValue * powValue;
			}
		}
		else
		{
			FOR(count)
			{
				int value = 0;
				readSignedIntegerBit(buffer, bufferSize, bitIndex, value);
				*list[i] = value * powValue;
			}
		}
		return true;
	}

	bool readDoubleListBit(const char* buffer, const int bufferSize, int& bitIndex, Vector<double>& list, const int precision)
	{
		// 读取长度
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
		const double powValue = inversePow10LLong(precision);
		// 使用统一的长度位
		if (lengthBitType)
		{
			byte bitCount = 0;
			if (!readSignedLengthBit<llong>(buffer, bufferSize, bitIndex, bitCount))
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
			// 读取所有的值
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(count)
			{
				// 读符号位
				const bool isNegative = getBufferBit(buffer, bitIndex++);

				// 读取值
				llong llongValue = 0;
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&(llongValue))[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&(llongValue))[byteCount - 1], bitCount - ((byteCount - 1) << 3));
				list[i] = isNegative ? -llongValue * powValue : llongValue * powValue;
			}
		}
		else
		{
			FOR(count)
			{
				llong value = 0;
				readSignedIntegerBit(buffer, bufferSize, bitIndex, value);
				list[i] = value * powValue;
			}
		}
		return true;
	}

	bool readDoubleListBit(const char* buffer, const int bufferSize, int& bitIndex, double** list, const int count, const int precision)
	{
		if (count == 0)
		{
			return true;
		}

		// 先读取长度位是使用哪种方式写入的
		const bool lengthBitType = getBufferBit(buffer, bitIndex++);
		const double powValue = inversePow10LLong(precision);
		// 使用统一的长度位
		if (lengthBitType)
		{
			byte bitCount = 0;
			if (!readSignedLengthBit<llong>(buffer, bufferSize, bitIndex, bitCount))
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
			// 读取所有的值
			const int byteCount = bitCountToByteCount(bitCount);
			FOR(count)
			{
				// 读符号位
				const bool isNegative = getBufferBit(buffer, bitIndex++);

				// 读取值
				llong llongValue = 0;
				FOR_J(byteCount - 1)
				{
					readByteBits(buffer, bitIndex, ((byte*)&(llongValue))[j], 8);
				}
				readByteBits(buffer, bitIndex, ((byte*)&(llongValue))[byteCount - 1], bitCount - ((byteCount - 1) << 3));
				*list[i] = isNegative ? -llongValue * powValue : llongValue * powValue;
			}
		}
		else
		{
			FOR(count)
			{
				llong value = 0;
				readSignedIntegerBit(buffer, bufferSize, bitIndex, value);
				*list[i] = value * powValue;
			}
		}
		return true;
	}

	bool writeBuffer(char* buffer, const int bufferSize, int& destOffset, const char* source, const int writeSize)
	{
		if (writeSize == 0 || source == nullptr)
		{
			return true;
		}
		if (bufferSize < destOffset + writeSize)
		{
			return false;
		}
		if (writeSize > 0)
		{
			MEMCPY(buffer + destOffset, bufferSize - destOffset, source, writeSize);
			destOffset += writeSize;
		}
		return true;
	}

	bool readBufferBit(const char* buffer, const int bufferSize, int& bitIndex, char* dest, const int destSize, const int readSize)
	{
		// 如果不读取任何数据,则不做改变
		if (readSize == 0)
		{
			return true;
		}
		// buffer不能按照位进行读取,但是所以需要将位偏移调整到字节的开头
		int byteIndex = bitCountToByteCount(bitIndex);
		const bool result = readBuffer(buffer, bufferSize, byteIndex, dest, destSize, readSize);
		bitIndex = byteIndex << 3;
		return result;
	}

	bool writeBufferBit(char* buffer, const int bufferSize, int& bitIndex, const char* source, const int writeSize)
	{
		// 如果不写入任何数据,则不做改变
		if (writeSize == 0)
		{
			return true;
		}
		fillZeroToByteEnd(buffer, bitIndex);
		int byteIndex = bitIndex >> 3;
		const bool result = writeBuffer(buffer, bufferSize, byteIndex, source, writeSize);
		bitIndex = byteIndex << 3;
		return result;
	}

	void fillZeroToByteEnd(char* buffer, int& bitIndex)
	{
		const int byteIndex = bitCountToByteCount(bitIndex);
		const int targetBitIndex = byteIndex << 3;
		// 需要将中间空出来的位填充为0
		for (int i = bitIndex; i < targetBitIndex; ++i)
		{
			setBit(buffer[i >> 3], i & 7, 0);
		}
		bitIndex = byteIndex << 3;
	}

	// 将所有的浮点数都扩大到整数存储
	bool writeFloatListBit(char* buffer, const int bufferSize, int& bitIndex, const float* list, const int count, const bool needWriteCount, const int precision)
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
		const int powValue = pow10(precision);
		const byte maxBitCount = generateBitCount(MathUtility::round(getFloatListMaxAbs(list, count) * powValue));
		// 如果使用一个统一的位数来表示写入位个数所占用的总位数
		int bitCountUnity;
		if (maxBitCount > 0)
		{
			bitCountUnity = (maxBitCount + 1) * count + SIGNED_LENGTH_MAX_BIT[sizeof(float)];
		}
		else
		{
			bitCountUnity = SIGNED_LENGTH_MAX_BIT[sizeof(float)];
		}
		
		// 如果每个值都使用自己的实际位数来表示写入位个数所占用的总位数
		// 先加上每个元素的长度位的位数量
		int bitCountSingle = SIGNED_LENGTH_MAX_BIT[sizeof(float)] * count;
		// 写入独立长度所占空间小于统一个数时才会继续计算值所占的空间,如果长度部分已经大于了,则不需要再继续计算了
		if (bitCountSingle < bitCountUnity)
		{
			FOR(count)
			{
				const int thisAbsValue = MathUtility::round(MathUtility::abs(list[i]) * powValue);
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
			if (!writeSignedLengthBit<int>(buffer, bufferSize, bitIndex, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + (1 + maxBitCount) * count) > bufferSize)
			{
				return false;
			}
			const byte byteCount = bitCountToByteCount(maxBitCount);
			FOR(count)
			{
				int intValue = MathUtility::round(list[i] * powValue);
				// 写入符号位
				if (intValue < 0)
				{
					intValue = -intValue;
					setBufferBitOne(buffer, bitIndex);
				}
				++bitIndex;
				// 再写入值的所有位
				FOR_J(byteCount - 1)
				{
					writeByteBits(buffer, bitIndex, ((byte*)&intValue)[j], 8);
				}
				writeByteBits(buffer, bitIndex, ((byte*)&intValue)[byteCount - 1], maxBitCount - ((byteCount - 1) << 3));
			}
		}
		// 使用每个元素独立的长度位占用空间更小
		else
		{
			// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
			++bitIndex;
			FOR(count)
			{
				writeSignedIntegerBit(buffer, bufferSize, bitIndex, MathUtility::round(list[i] * powValue));
			}
		}
		return true;
	}

	bool writeDoubleListBit(char* buffer, const int bufferSize, int& bitIndex, const double* list, const int count, const bool needWriteCount, const int precision)
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
		const llong powValue = pow10LLong(precision);
		const byte maxBitCount = generateBitCount(roundDouble(getDoubleListMaxAbs(list, count) * powValue));
		// 如果使用一个统一的位数来表示写入位个数所占用的总位数
		int bitCountUnity;
		if (maxBitCount > 0)
		{
			bitCountUnity = (maxBitCount + 1) * count + SIGNED_LENGTH_MAX_BIT[sizeof(double)];
		}
		else
		{
			bitCountUnity = SIGNED_LENGTH_MAX_BIT[sizeof(double)];
		}
		// 如果每个值都使用自己的实际位数来表示写入位个数所占用的总位数
		// 先加上每个元素的长度位的位数量
		int bitCountSingle = SIGNED_LENGTH_MAX_BIT[sizeof(double)] * count;
		// 写入独立长度所占空间小于统一个数时才会继续计算值所占的空间,如果长度部分已经大于了,则不需要再继续计算了
		if (bitCountSingle < bitCountUnity)
		{
			FOR(count)
			{
				const llong thisAbsValue = roundDouble(MathUtility::abs(list[i]) * powValue);
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
			if (!writeSignedLengthBit<llong>(buffer, bufferSize, bitIndex, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			if (bitCountToByteCount(bitIndex + (1 + maxBitCount) * count) > bufferSize)
			{
				return false;
			}
			const byte byteCount = bitCountToByteCount(maxBitCount);
			FOR(count)
			{
				llong llongValue = roundDouble(list[i] * powValue);
				// 写入符号位
				if (llongValue < 0)
				{
					llongValue = -llongValue;
					setBufferBitOne(buffer, bitIndex);
				}
				++bitIndex;
				// 再写入值的所有位
				FOR_J(byteCount - 1)
				{
					writeByteBits(buffer, bitIndex, ((byte*)&llongValue)[j], 8);
				}
				writeByteBits(buffer, bitIndex, ((byte*)&llongValue)[byteCount - 1], maxBitCount - ((byteCount - 1) << 3));
			}
		}
		// 使用每个元素独立的长度位占用空间更小
		else
		{
			// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
			++bitIndex;
			FOR(count)
			{
				writeSignedIntegerBit(buffer, bufferSize, bitIndex, roundDouble(list[i] * powValue));
			}
		}
		return true;
	}

	bool writeVector2(char* buffer, const int bufferSize, int& index, const Vector2& value)
	{
		return write(buffer, bufferSize, index, value.x) && write(buffer, bufferSize, index, value.y);
	}

	bool writeVector2Inverse(char* buffer, const int bufferSize, int& index, const Vector2& value)
	{
		return writeInverse(buffer, bufferSize, index, value.x) && writeInverse(buffer, bufferSize, index, value.y);
	}

	bool writeVector2Int(char* buffer, const int bufferSize, int& index, const Vector2Int& value)
	{
		return write(buffer, bufferSize, index, value.x) && write(buffer, bufferSize, index, value.y);
	}

	bool writeVector2IntInverse(char* buffer, const int bufferSize, int& index, const Vector2Int& value)
	{
		return writeInverse(buffer, bufferSize, index, value.x) && writeInverse(buffer, bufferSize, index, value.y);
	}

	bool writeVector3(char* buffer, const int bufferSize, int& index, const Vector3& value)
	{
		return write(buffer, bufferSize, index, value.x) &&
			write(buffer, bufferSize, index, value.y) &&
			write(buffer, bufferSize, index, value.z);
	}

	bool writeVector3Inverse(char* buffer, const int bufferSize, int& index, const Vector3& value)
	{
		return writeInverse(buffer, bufferSize, index, value.x) &&
			writeInverse(buffer, bufferSize, index, value.y) &&
			writeInverse(buffer, bufferSize, index, value.z);
	}

	bool writeVector4(char* buffer, const int bufferSize, int& index, const Vector4& value)
	{
		return write(buffer, bufferSize, index, value.x) &&
			write(buffer, bufferSize, index, value.y) &&
			write(buffer, bufferSize, index, value.z) &&
			write(buffer, bufferSize, index, value.w);
	}

	bool writeVector4Inverse(char* buffer, const int bufferSize, int& index, const Vector4& value)
	{
		return writeInverse(buffer, bufferSize, index, value.x) &&
			writeInverse(buffer, bufferSize, index, value.y) &&
			writeInverse(buffer, bufferSize, index, value.z) &&
			writeInverse(buffer, bufferSize, index, value.w);
	}

	bool writeColor(char* buffer, const int bufferSize, int& index, const Color& value)
	{
		return write(buffer, bufferSize, index, value.r) &&
			write(buffer, bufferSize, index, value.g) &&
			write(buffer, bufferSize, index, value.b) &&
			write(buffer, bufferSize, index, value.a);
	}

	bool writeColorInverse(char* buffer, const int bufferSize, int& index, const Color& value)
	{
		return writeInverse(buffer, bufferSize, index, value.r) &&
			writeInverse(buffer, bufferSize, index, value.g) &&
			writeInverse(buffer, bufferSize, index, value.b) &&
			writeInverse(buffer, bufferSize, index, value.a);
	}

	bool readVector2(const char* buffer, const int bufferSize, int& index, Vector2& value)
	{
		return read(buffer, bufferSize, index, value.x) && read(buffer, bufferSize, index, value.y);
	}

	bool readVector2Inverse(const char* buffer, const int bufferSize, int& index, Vector2& value)
	{
		return readInverse(buffer, bufferSize, index, value.x) && readInverse(buffer, bufferSize, index, value.y);
	}

	bool readVector2Int(const char* buffer, const int bufferSize, int& index, Vector2Int& value)
	{
		return read(buffer, bufferSize, index, value.x) && read(buffer, bufferSize, index, value.y);
	}

	bool readVector2IntInverse(const char* buffer, const int bufferSize, int& index, Vector2Int& value)
	{
		return readInverse(buffer, bufferSize, index, value.x) && readInverse(buffer, bufferSize, index, value.y);
	}

	bool readVector3(const char* buffer, const int bufferSize, int& index, Vector3& value)
	{
		return read(buffer, bufferSize, index, value.x) &&
			read(buffer, bufferSize, index, value.y) &&
			read(buffer, bufferSize, index, value.z);
	}

	bool readVector3Inverse(const char* buffer, const int bufferSize, int& index, Vector3& value)
	{
		return readInverse(buffer, bufferSize, index, value.x) &&
			readInverse(buffer, bufferSize, index, value.y) &&
			readInverse(buffer, bufferSize, index, value.z);
	}

	bool readVector4(const char* buffer, const int bufferSize, int& index, Vector4& value)
	{
		return read(buffer, bufferSize, index, value.x) &&
			read(buffer, bufferSize, index, value.y) &&
			read(buffer, bufferSize, index, value.z) &&
			read(buffer, bufferSize, index, value.w);
	}

	bool readVector4Inverse(const char* buffer, const int bufferSize, int& index, Vector4& value)
	{
		return readInverse(buffer, bufferSize, index, value.x) &&
			readInverse(buffer, bufferSize, index, value.y) &&
			readInverse(buffer, bufferSize, index, value.z) &&
			readInverse(buffer, bufferSize, index, value.w);
	}

	bool readColor(const char* buffer, const int bufferSize, int& index, Color& value)
	{
		return read(buffer, bufferSize, index, value.r) &&
			read(buffer, bufferSize, index, value.g) &&
			read(buffer, bufferSize, index, value.b) &&
			read(buffer, bufferSize, index, value.a);
	}

	bool readColorInverse(const char* buffer, const int bufferSize, int& index, Color& value)
	{
		return readInverse(buffer, bufferSize, index, value.r) &&
			readInverse(buffer, bufferSize, index, value.g) &&
			readInverse(buffer, bufferSize, index, value.b) &&
			readInverse(buffer, bufferSize, index, value.a);
	}

	float getFloatListMaxAbs(const float* list, const int count)
	{
		if (count == 0)
		{
			return 0;
		}

		float maxValue = MathUtility::abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMinRef(maxValue, MathUtility::abs(list[i]));
		}
		return maxValue;
	}

	double getDoubleListMaxAbs(const double* list, const int count)
	{
		if (count == 0)
		{
			return 0;
		}

		double maxValue = MathUtility::abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMinRef(maxValue, MathUtility::abs(list[i]));
		}
		return maxValue;
	}

	void initBitCountTable()
	{
		if (mBitCountTable[1] != 0)
		{
			return;
		}
		FOR(65536)
		{
			mBitCountTable[i] = internalGenerateBitCount(i);
		}
	}

	byte internalGenerateBitCount(const ushort value)
	{
		if (value == 0)
		{
			return 0;
		}
		// 从高到低遍历每一位,找到最高位1的下标
		FOR(16)
		{
			if (getBit(value, 15 - i) == 1)
			{
				return 16 - i;
			}
		}
		return 0;
	}

	void logErrorInternal(string&& info) 
	{
		ERROR(info);
	}
}