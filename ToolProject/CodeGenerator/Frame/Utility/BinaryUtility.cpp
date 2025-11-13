#include "BinaryUtility.h"

const ushort BinaryUtility::crc16_table[256]
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

uint BinaryUtility::mStringType = (uint)typeid(string).hash_code();
uint BinaryUtility::mBoolType = (uint)typeid(bool).hash_code();
uint BinaryUtility::mCharType = (uint)typeid(char).hash_code();
uint BinaryUtility::mByteType = (uint)typeid(unsigned char).hash_code();
uint BinaryUtility::mShortType = (uint)typeid(short).hash_code();
uint BinaryUtility::mUShortType = (uint)typeid(ushort).hash_code();
uint BinaryUtility::mIntType = (uint)typeid(int).hash_code();
uint BinaryUtility::mUIntType = (uint)typeid(uint).hash_code();
uint BinaryUtility::mFloatType = (uint)typeid(float).hash_code();
uint BinaryUtility::mLLongType = (uint)typeid(llong).hash_code();
uint BinaryUtility::mULLongType = (uint)typeid(ullong).hash_code();
uint BinaryUtility::mBoolArrayType = (uint)typeid(bool*).hash_code();
uint BinaryUtility::mCharArrayType = (uint)typeid(char*).hash_code();
uint BinaryUtility::mByteArrayType = (uint)typeid(byte*).hash_code();
uint BinaryUtility::mShortArrayType = (uint)typeid(short*).hash_code();
uint BinaryUtility::mUShortArrayType = (uint)typeid(ushort*).hash_code();
uint BinaryUtility::mIntArrayType = (uint)typeid(int*).hash_code();
uint BinaryUtility::mUIntArrayType = (uint)typeid(uint*).hash_code();
uint BinaryUtility::mFloatArrayType = (uint)typeid(float*).hash_code();
uint BinaryUtility::mULLongArrayType = (uint)typeid(ullong*).hash_code();
uint BinaryUtility::mBoolListType = (uint)typeid(myVector<bool>).hash_code();
uint BinaryUtility::mCharListType = (uint)typeid(myVector<char>).hash_code();
uint BinaryUtility::mByteListType = (uint)typeid(myVector<byte>).hash_code();
uint BinaryUtility::mShortListType = (uint)typeid(myVector<short>).hash_code();
uint BinaryUtility::mUShortListType = (uint)typeid(myVector<ushort>).hash_code();
uint BinaryUtility::mIntListType = (uint)typeid(myVector<int>).hash_code();
uint BinaryUtility::mUIntListType = (uint)typeid(myVector<uint>).hash_code();
uint BinaryUtility::mFloatListType = (uint)typeid(myVector<float>).hash_code();
uint BinaryUtility::mULLongListType = (uint)typeid(myVector<ullong>).hash_code();
uint BinaryUtility::mVector2IntType = (uint)typeid(Vector2Int).hash_code();
uint BinaryUtility::mVector2UShortType = (uint)typeid(Vector2UShort).hash_code();
uint BinaryUtility::mVector2ShortType = (uint)typeid(Vector2Short).hash_code();

// 计算 16进制的c中1的个数
uint BinaryUtility::crc_check(char c)
{
	uint count = 0;
	uint bitCount = sizeof(char) * 8;
	FOR_I(bitCount)
	{
		if ((c & (0x01 << i)) > 0)
		{
			++count;
		}
	}
	return count;
}
ushort BinaryUtility::crc16(ushort crc, char* buffer, uint len, uint bufferOffset)
{
	FOR_I(len)
	{
		crc = crc16_byte(crc, buffer[bufferOffset + i]);
	}
	return crc;
}
ushort BinaryUtility::crc16_byte(ushort crc, byte data)
{
	return (ushort)((crc >> 8) ^ crc16_table[(crc ^ data) & 0xFF]);
}
bool BinaryUtility::readBuffer(char* buffer, uint bufferSize, uint& index, char* dest, uint readSize)
{
	if (bufferSize < index + readSize)
	{
		return false;
	}
	if (readSize > 0)
	{
		memcpy(dest, buffer + index, readSize);
		index += readSize;
	}
	return true;
}
bool BinaryUtility::writeBuffer(char* buffer, uint bufferSize, uint& destOffset, char* source, uint writeSize)
{
	if (bufferSize < destOffset + writeSize)
	{
		return false;
	}
	if (writeSize > 0)
	{
		memcpy(buffer + destOffset, source, writeSize);
		destOffset += writeSize;
	}
	return true;
}

bool BinaryUtility::writeVector2(char* buffer, uint bufferSize, uint& index, Vector2& value, bool inverse)
{
	bool ret = write(buffer, bufferSize, index, value.x, inverse);
	ret = write(buffer, bufferSize, index, value.y, inverse) && ret;
	return ret;
}

bool BinaryUtility::writeVector3(char* buffer, uint bufferSize, uint& index, Vector3& value, bool inverse)
{
	bool ret = write(buffer, bufferSize, index, value.x, inverse);
	ret = write(buffer, bufferSize, index, value.y, inverse) && ret;
	ret = write(buffer, bufferSize, index, value.z, inverse) && ret;
	return ret;
}

bool BinaryUtility::writeVector4(char* buffer, uint bufferSize, uint& index, Vector4& value, bool inverse)
{
	bool ret = write(buffer, bufferSize, index, value.x, inverse);
	ret = write(buffer, bufferSize, index, value.y, inverse) && ret;
	ret = write(buffer, bufferSize, index, value.z, inverse) && ret;
	ret = write(buffer, bufferSize, index, value.w, inverse) && ret;
	return ret;
}

bool BinaryUtility::writeColor(char* buffer, uint bufferSize, uint& index, Color& value, bool inverse)
{
	bool ret = write(buffer, bufferSize, index, value.r, inverse);
	ret = write(buffer, bufferSize, index, value.g, inverse) && ret;
	ret = write(buffer, bufferSize, index, value.b, inverse) && ret;
	ret = write(buffer, bufferSize, index, value.a, inverse) && ret;
	return ret;
}

void BinaryUtility::readVector2(char* buffer, uint bufferSize, uint& index, Vector2& value, bool inverse)
{
	value.x = read<float>(buffer, bufferSize, index, inverse);
	value.y = read<float>(buffer, bufferSize, index, inverse);
}

void BinaryUtility::readVector3(char* buffer, uint bufferSize, uint& index, Vector3& value, bool inverse)
{
	value.x = read<float>(buffer, bufferSize, index, inverse);
	value.y = read<float>(buffer, bufferSize, index, inverse);
	value.z = read<float>(buffer, bufferSize, index, inverse);
}

void BinaryUtility::readVector4(char* buffer, uint bufferSize, uint& index, Vector4& value, bool inverse)
{
	value.x = read<float>(buffer, bufferSize, index, inverse);
	value.y = read<float>(buffer, bufferSize, index, inverse);
	value.z = read<float>(buffer, bufferSize, index, inverse);
	value.w = read<float>(buffer, bufferSize, index, inverse);
}

void BinaryUtility::readColor(char* buffer, uint bufferSize, uint& index, Color& value, bool inverse)
{
	value.r = read<byte>(buffer, bufferSize, index, inverse);
	value.g = read<byte>(buffer, bufferSize, index, inverse);
	value.b = read<byte>(buffer, bufferSize, index, inverse);
	value.a = read<byte>(buffer, bufferSize, index, inverse);
}