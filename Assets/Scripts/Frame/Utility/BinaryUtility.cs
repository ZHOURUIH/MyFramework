using System;
using System.Text;

public class BinaryUtility
{
	private static ThreadLock mTempBytes4Lock = new ThreadLock();
	private static byte[] mTempBytes4 = new byte[4];
	protected static Encoding ENCODING_GB2312;
	protected static Encoding ENCODING_GBK;
	/** CRC table for the CRC-16. The poly is 0x8005 (x^16 + x^15 + x^2 + 1) */
	protected static ushort[] crc16_table;
	public static void initUtility()
	{
		ENCODING_GB2312 = Encoding.GetEncoding("gb2312");
		ENCODING_GBK = Encoding.GetEncoding("GBK");
		crc16_table = new ushort[256]
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
	}
	public static Encoding getGB2312() { return ENCODING_GB2312; }
	public static Encoding getGBK() { return ENCODING_GBK; }
	// 计算 16进制的c中1的个数
	public static int crc_check(byte c)
	{
		int count = 0;
		int bitCount = sizeof(char) * 8;
		for (int i = 0; i < bitCount; ++i)
		{
			if ((c & (0x01 << i)) > 0)
			{
				++count;
			}
		}
		return count;
	}
	public static ushort crc16(ushort crc, byte[] buffer, int len, int bufferOffset = 0)
	{
		for (int i = 0; i < len; ++i)
		{
			crc = crc16_byte(crc, buffer[bufferOffset + i]);
		}
		return crc;
	}
	public static ushort crc16_byte(ushort crc, byte data)
	{
		return (ushort)((crc >> 8) ^ crc16_table[(crc ^ data) & 0xFF]);
	}
	public static bool readBool(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(bool);
		if (!success)
		{
			return false;
		}
		return buffer[index++] != 0;
	}
	public static byte readByte(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(byte);
		if (!success)
		{
			return 0;
		}
		return buffer[index++];
	}
	public static short readShort(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(short);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(short);
		return bytesToShort(buffer[pre + 0], buffer[pre + 1]);
	}
	public static short readShortBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(short);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(short);
		return bytesToShortBigEndian(buffer[pre + 0], buffer[pre + 1]);
	}
	public static ushort readUShort(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(ushort);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ushort);
		return bytesToUShort(buffer[pre + 0], buffer[pre + 1]);
	}
	public static ushort readUShortBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(ushort);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ushort);
		return bytesToUShortBigEndian(buffer[pre + 0], buffer[pre + 1]);
	}
	public static int readInt(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(int);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(int);
		return bytesToInt(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static int readIntBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(int);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(int);
		return bytesToIntBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static uint readUInt(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(uint);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(uint);
		return bytesToUInt(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static uint readUIntBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(uint);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(uint);
		return bytesToUIntBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static long readLong(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(long);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(long);
		return bytesToLong(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3], buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static long readLongBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(long);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(long);
		return bytesToLongBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3], buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static ulong readULong(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(ulong);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ulong);
		return bytesToULong(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3], buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static ulong readULongBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(ulong);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ulong);
		return bytesToULongBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3], buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static float readFloat(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(float);
		if (!success)
		{
			return 0.0f;
		}
		int pre = index;
		index += sizeof(float);
		return bytesToFloat(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static float readFloatBigEndian(byte[] buffer, ref int index, out bool success)
	{
		success = buffer.Length >= index + sizeof(float);
		if (!success)
		{
			return 0.0f;
		}
		int pre = index;
		index += sizeof(float);
		return bytesToFloatBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	// readCount表示读取的bool的个数,小于0表示按照destBuffer数组长度读取
	public static bool readBools(byte[] buffer, ref int index, bool[] destBuffer, int readCount = -1)
	{
		int boolCount = readCount < 0 ? destBuffer.Length : readCount;
		if (boolCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < boolCount; ++i)
		{
			destBuffer[i] = readBool(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	public static bool readBytes(byte[] buffer, ref int index, byte[] destBuffer, int bufferSize = -1, int destBufferSize = -1, int readSize = -1)
	{
		if (bufferSize == -1)
		{
			bufferSize = buffer.Length;
		}
		if (destBufferSize == -1)
		{
			destBufferSize = destBuffer.Length;
		}
		if (readSize == -1)
		{
			readSize = destBuffer.Length;
		}
		if (destBufferSize < readSize || readSize + index > bufferSize)
		{
			return false;
		}
		readSize = MathUtility.getMin(readSize, destBufferSize);
		if (readSize > 0)
		{
			memcpy(destBuffer, buffer, 0, index, readSize);
			index += readSize;
		}
		return true;
	}
	// readCount表示要读取的short个数,小于0表示使用数组长度
	public static bool readShorts(byte[] buffer, ref int index, short[] destBuffer, int readCount = -1)
	{
		int shortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (shortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < shortCount; ++i)
		{
			destBuffer[i] = readShort(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的short个数,小于0表示使用数组长度
	public static bool readShortsBigEndian(byte[] buffer, ref int index, short[] destBuffer, int readCount = -1)
	{
		int shortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (shortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < shortCount; ++i)
		{
			destBuffer[i] = readShortBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ushort个数,小于0表示使用数组长度
	public static bool readUShorts(byte[] buffer, ref int index, ushort[] destBuffer, int readCount = -1)
	{
		int ushortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (ushortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < ushortCount; ++i)
		{
			destBuffer[i] = readUShort(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ushort个数,小于0表示使用数组长度
	public static bool readUShortsBigEndian(byte[] buffer, ref int index, ushort[] destBuffer, int readCount = -1)
	{
		int ushortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (ushortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < ushortCount; ++i)
		{
			destBuffer[i] = readUShortBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的int个数,小于0表示使用destBuffer数组长度
	public static bool readInts(byte[] buffer, ref int index, int[] destBuffer, int readCount = -1)
	{
		int intCount = readCount < 0 ? destBuffer.Length : readCount;
		if (intCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < intCount; ++i)
		{
			destBuffer[i] = readInt(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的int个数,小于0表示使用destBuffer数组长度
	public static bool readIntsBigEndian(byte[] buffer, ref int index, int[] destBuffer, int readCount = -1)
	{
		int intCount = readCount < 0 ? destBuffer.Length : readCount;
		if (intCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < intCount; ++i)
		{
			destBuffer[i] = readIntBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的uint个数,小于0表示使用数组长度
	public static bool readUInts(byte[] buffer, ref int index, uint[] destBuffer, int readCount = -1)
	{
		int uintCount = readCount < 0 ? destBuffer.Length : readCount;
		if (uintCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < uintCount; ++i)
		{
			destBuffer[i] = readUInt(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的uint个数,小于0表示使用数组长度
	public static bool readUIntsBigEndian(byte[] buffer, ref int index, uint[] destBuffer, int readCount = -1)
	{
		int uintCount = readCount < 0 ? destBuffer.Length : readCount;
		if (uintCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < uintCount; ++i)
		{
			destBuffer[i] = readUIntBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的long个数,小于0表示使用数组长度
	public static bool readLongs(byte[] buffer, ref int index, long[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readLong(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的long个数,小于0表示使用数组长度
	public static bool readLongsBigEndian(byte[] buffer, ref int index, long[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readLongBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ulong个数,小于0表示使用数组长度
	public static bool readULongs(byte[] buffer, ref int index, ulong[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readULong(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ulong个数,小于0表示使用数组长度
	public static bool readULongsBigEndian(byte[] buffer, ref int index, ulong[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readULongBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的float的数量,小于0表示使用数组的长度
	public static bool readFloats(byte[] buffer, ref int index, float[] destBuffer, int readCount = -1)
	{
		int floatCount = readCount < 0 ? destBuffer.Length : readCount;
		if (floatCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < floatCount; ++i)
		{
			destBuffer[i] = readFloat(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的float的数量,小于0表示使用数组的长度
	public static bool readFloatsBigEndian(byte[] buffer, ref int index, float[] destBuffer, int readCount = -1)
	{
		int floatCount = readCount < 0 ? destBuffer.Length : readCount;
		if (floatCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < floatCount; ++i)
		{
			destBuffer[i] = readFloatBigEndian(buffer, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeBool(byte[] buffer, ref int index, bool value)
	{
		if (buffer.Length < index + sizeof(bool))
		{
			return false;
		}
		buffer[index++] = (byte)(value ? 1 : 0);
		return true;
	}
	public static bool writeByte(byte[] buffer, ref int index, byte value)
	{
		if (buffer.Length < index + sizeof(byte))
		{
			return false;
		}
		buffer[index++] = value;
		return true;
	}
	public static bool writeShort(byte[] buffer, ref int index, short value)
	{
		if (buffer.Length < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		return true;
	}
	public static bool writeShortBigEndian(byte[] buffer, ref int index, short value)
	{
		if (buffer.Length < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeUShort(byte[] buffer, ref int index, ushort value)
	{
		if (buffer.Length < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		return true;
	}
	public static bool writeUShortBigEndian(byte[] buffer, ref int index, ushort value)
	{
		if (buffer.Length < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeInt(byte[] buffer, ref int index, int value)
	{
		if (buffer.Length < index + sizeof(int))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		return true;
	}
	public static bool writeIntBigEndian(byte[] buffer, ref int index, int value)
	{
		if (buffer.Length < index + sizeof(int))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeUInt(byte[] buffer, ref int index, uint value)
	{
		if (buffer.Length < index + sizeof(uint))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		return true;
	}
	public static bool writeUIntBigEndian(byte[] buffer, ref int index, uint value)
	{
		if (buffer.Length < index + sizeof(uint))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeLong(byte[] buffer, ref int index, long value)
	{
		if (buffer.Length < index + sizeof(long))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)(((0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)(((0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)(((0xFF << (8 * 7)) & value) >> (8 * 7));
		return true;
	}
	public static bool writeLongBigEndian(byte[] buffer, ref int index, long value)
	{
		if (buffer.Length < index + sizeof(long))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 7)) & value) >> (8 * 7));
		buffer[index++] = (byte)(((0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)(((0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)(((0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeULong(byte[] buffer, ref int index, ulong value)
	{
		if (buffer.Length < index + sizeof(ulong))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 7)) & value) >> (8 * 7));
		return true;
	}
	public static bool writeULongBigEndian(byte[] buffer, ref int index, ulong value)
	{
		if (buffer.Length < index + sizeof(ulong))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 7)) & value) >> (8 * 7));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeFloat(byte[] buffer, ref int index, float value)
	{
		if (buffer.Length < index + sizeof(float))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[0];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[3];
		return true;
	}
	public static bool writeFloatBigEndian(byte[] buffer, ref int index, float value)
	{
		if (buffer.Length < index + sizeof(float))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[3];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[0];
		return true;
	}
	// writeCount表示要写入的bool个数,小于0表示将整个数组全部写入
	public static bool writeBools(byte[] buffer, ref int index, bool[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int boolCount = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < boolCount; ++i)
		{
			ret = writeBool(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	public static bool writeBytes(byte[] buffer, ref int index, byte[] sourceBuffer, int bufferSize = -1, int sourceBufferSize = -1, int writeSize = -1)
	{
		if (bufferSize == -1)
		{
			bufferSize = buffer.Length;
		}
		if (sourceBufferSize == -1)
		{
			sourceBufferSize = sourceBuffer.Length;
		}
		if (writeSize == -1)
		{
			writeSize = sourceBuffer.Length;
		}
		if (writeSize > sourceBufferSize || writeSize + index > bufferSize)
		{
			return false;
		}
		memcpy(buffer, sourceBuffer, index, 0, writeSize);
		index += writeSize;
		return true;
	}
	// writeCount表示要写入的short个数,小于0表示写入整个数组
	public static bool writeShorts(byte[] buffer, ref int index, short[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeShort(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的short个数,小于0表示写入整个数组
	public static bool writeShortsBigEndian(byte[] buffer, ref int index, short[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeShortBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ushort个数,小于0表示写入整个数组
	public static bool writeUShorts(byte[] buffer, ref int index, ushort[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUShort(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ushort个数,小于0表示写入整个数组
	public static bool writeUShortsBigEndian(byte[] buffer, ref int index, ushort[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUShortBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的int个数,小于0表示写入整个数组
	public static bool writeInts(byte[] buffer, ref int index, int[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeInt(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的int个数,小于0表示写入整个数组
	public static bool writeIntsBigEndian(byte[] buffer, ref int index, int[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeIntBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的uint个数,小于0表示写入整个数组
	public static bool writeUInts(byte[] buffer, ref int index, uint[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUInt(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的uint个数,小于0表示写入整个数组
	public static bool writeUIntsBigEndian(byte[] buffer, ref int index, uint[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUIntBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的long的个数,小于0表示写入整个数组
	public static bool writeLongs(byte[] buffer, ref int index, long[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeLong(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的long的个数,小于0表示写入整个数组
	public static bool writeLongsBigEndian(byte[] buffer, ref int index, long[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeLongBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ulong的个数,小于0表示写入整个数组
	public static bool writeULongs(byte[] buffer, ref int index, ulong[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeULong(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ulong的个数,小于0表示写入整个数组
	public static bool writeULongsBigEndian(byte[] buffer, ref int index, ulong[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeULongBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的浮点数的数量,小于0表示写入整个数组
	public static bool writeFloats(byte[] buffer, ref int index, float[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeFloat(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的浮点数的数量,小于0表示写入整个数组
	public static bool writeFloatsBigEndian(byte[] buffer, ref int index, float[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeFloatBigEndian(buffer, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	public static void memcpyObject<T>(T[] dest, T[] src, int destOffset, int srcOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[srcOffset + i];
		}
	}
	public static void memcpy(Array dest, Array src, int destByteOffset, int srcByteOffset, int byteCount)
	{
		Buffer.BlockCopy(src, srcByteOffset, dest, destByteOffset, byteCount);
	}
	public static void memmove<T>(ref T[] data, int dest, int src, int count)
	{
		if (count <= 0)
		{
			return;
		}
		// 如果两个内存区有相交的部分,并且源地址在前面,则从后面往前拷贝字节
		if (src < dest && src + count > dest)
		{
			for (int i = 0; i < count; ++i)
			{
				data[count - i - 1 + dest] = data[count - i - 1 + src];
			}
		}
		else
		{
			for (int i = 0; i < count; ++i)
			{
				data[i + dest] = data[i + src];
			}
		}
	}
	// 将数组每个元素值设置为0
	public static void memset<T>(T[] p, T value, int length = -1)
	{
		if (length == -1)
		{
			length = p.Length;
		}
		// 有两种方法清零一个数组,遍历和Array.Clear(),但是两个效率在不同情况下是不一样的,数量小于77时,遍历会快一些,数量大于等于77时,Array.Clear()更快
		if (length < 77)
		{
			for (int i = 0; i < length; ++i)
			{
				p[i] = value;
			}
			return;
		}
		Array.Clear(p, 0, length);
	}
	public static void shortToBytes(short value, byte[] bytes)
	{
		if (bytes.Length != sizeof(short))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
	}
	public static void shortToBytesBigEndian(short value, byte[] bytes)
	{
		if (bytes.Length != sizeof(short))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[1] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
	}
	public static void intToBytes(int value, byte[] bytes)
	{
		if (bytes.Length != sizeof(int))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[2] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[3] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
	}
	public static void intToBytesBigEndian(int value, byte[] bytes)
	{
		if (bytes.Length != sizeof(int))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		bytes[1] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[2] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[3] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
	}
	public static void uintToBytes(uint value, byte[] bytes)
	{
		if (bytes.Length != sizeof(uint))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[2] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[3] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
	}
	public static void uintToBytesBigEndian(uint value, byte[] bytes)
	{
		if (bytes.Length != sizeof(uint))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		bytes[1] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[2] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[3] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
	}
	public static byte[] toBytes(float value)
	{
		return BitConverter.GetBytes(value);
	}
	public static byte bytesToByte(byte[] array)
	{
		return array[0];
	}
	public static short bytesToShort(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToShort(array[0], array[1]);
	}
	public static short bytesToShortBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToShortBigEndian(array[0], array[1]);
	}
	public static short bytesToShort(byte byte0, byte byte1) { return (short)((byte1 << (8 * 1)) | (byte0 << (8 * 0))); }
	public static short bytesToShortBigEndian(byte byte0, byte byte1) { return (short)((byte1 << (8 * 0)) | (byte0 << (8 * 1))); }
	public static ushort bytesToUShort(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUShort(array[0], array[1]);
	}
	public static ushort bytesToUShortBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUShortBigEndian(array[0], array[1]);
	}
	public static ushort bytesToUShort(byte b0, byte b1) { return (ushort)((b1 << (8 * 1)) | (b0 << (8 * 0))); }
	public static ushort bytesToUShortBigEndian(byte b0, byte b1) { return (ushort)((b1 << (8 * 0)) | (b0 << (8 * 1))); }
	public static int bytesToInt(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToInt(array[0], array[1], array[2], array[3]);
	}
	public static int bytesToIntBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToIntBigEndian(array[0], array[1], array[2], array[3]);
	}
	public static int bytesToInt(byte b0, byte b1, byte b2, byte b3) { return (b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0)); }
	public static int bytesToIntBigEndian(byte b0, byte b1, byte b2, byte b3) { return (b3 << (8 * 0)) | (b2 << (8 * 1)) | (b1 << (8 * 2)) | (b0 << (8 * 3)); }
	public static uint bytesToUInt(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUInt(array[0], array[1], array[2], array[3]);
	}
	public static uint bytesToUIntBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUIntBigEndian(array[0], array[1], array[2], array[3]);
	}
	public static uint bytesToUInt(byte b0, byte b1, byte b2, byte b3) { return (uint)((b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0))); }
	public static uint bytesToUIntBigEndian(byte b0, byte b1, byte b2, byte b3) { return (uint)((b3 << (8 * 0)) | (b2 << (8 * 1)) | (b1 << (8 * 2)) | (b0 << (8 * 3))); }
	public static long bytesToLong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 7) | (long)b6 << (8 * 6) | (long)b5 << (8 * 5) | (long)b4 << (8 * 4) | (long)b3 << (8 * 3) | (long)b2 << (8 * 2) | (long)b1 << (8 * 1) | (long)b0 << (8 * 0);
	}
	public static long bytesToLongBigEndian(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 0) | (long)b6 << (8 * 1) | (long)b5 << (8 * 2) | (long)b4 << (8 * 3) | (long)b3 << (8 * 4) | (long)b2 << (8 * 5) | (long)b1 << (8 * 6) | (long)b0 << (8 * 7);
	}
	public static ulong bytesToULong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 7) | (ulong)b6 << (8 * 6) | (ulong)b5 << (8 * 5) | (ulong)b4 << (8 * 4) | (ulong)b3 << (8 * 3) | (ulong)b2 << (8 * 2) | (ulong)b1 << (8 * 1) | (ulong)b0 << (8 * 0);
	}
	public static ulong bytesToULongBigEndian(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 0) | (ulong)b6 << (8 * 1) | (ulong)b5 << (8 * 2) | (ulong)b4 << (8 * 3) | (ulong)b3 << (8 * 4) | (ulong)b2 << (8 * 5) | (ulong)b1 << (8 * 6) | (ulong)b0 << (8 * 7);
	}
	public static float bytesToFloat(byte[] array)
	{
		if (array == null)
		{
			return 0.0f;
		}
		return BitConverter.ToSingle(array, 0);
	}
	public static float bytesToFloat(byte byte0, byte byte1, byte byte2, byte byte3)
	{
		mTempBytes4Lock.waitForUnlock();
		mTempBytes4[0] = byte0;
		mTempBytes4[1] = byte1;
		mTempBytes4[2] = byte2;
		mTempBytes4[3] = byte3;
		float value = BitConverter.ToSingle(mTempBytes4, 0);
		mTempBytes4Lock.unlock();
		return value;
	}
	public static float bytesToFloatBigEndian(byte byte0, byte byte1, byte byte2, byte byte3)
	{
		mTempBytes4Lock.waitForUnlock();
		mTempBytes4[0] = byte3;
		mTempBytes4[1] = byte2;
		mTempBytes4[2] = byte1;
		mTempBytes4[3] = byte0;
		float value = BitConverter.ToSingle(mTempBytes4, 0);
		mTempBytes4Lock.unlock();
		return value;
	}
	public static byte[] stringToBytes(string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		// 默认为UTF8
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return encoding.GetBytes(str);
	}
	public static string bytesToString(byte[] bytes, Encoding encoding = null)
	{
		// 默认为UTF8
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return removeLastZero(encoding.GetString(bytes));
	}
	public static string bytesToString(byte[] bytes, int startIndex, int count, Encoding encoding = null)
	{
		// 默认为UTF8
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return removeLastZero(encoding.GetString(bytes, startIndex, count));
	}
	public static string convertStringFormat(string str, Encoding source, Encoding target)
	{
		return bytesToString(stringToBytes(str, source), target);
	}
	public static string UTF8ToUnicode(string str)
	{
		return convertStringFormat(str, Encoding.UTF8, Encoding.Unicode);
	}
	public static string UTF8ToGB2312(string str)
	{
		return convertStringFormat(str, Encoding.UTF8, getGB2312());
	}
	public static string UnicodeToUTF8(string str)
	{
		return convertStringFormat(str, Encoding.Unicode, Encoding.UTF8);
	}
	public static string UnicodeToGB2312(string str)
	{
		return convertStringFormat(str, Encoding.Unicode, getGB2312());
	}
	public static string GB2312ToUTF8(string str)
	{
		return convertStringFormat(str, getGB2312(), Encoding.UTF8);
	}
	public static string GB2312ToUnicode(string str)
	{
		return convertStringFormat(str, getGB2312(), Encoding.Unicode);
	}
	// 字节数组转换为字符串时,末尾可能会带有数字0,此时在字符串比较时会出现错误,所以需要移除字符串末尾的0
	public static string removeLastZero(string str)
	{
		int strLen = str.Length;
		int newLen = strLen;
		for (int i = 0; i < strLen; ++i)
		{
			if (str[i] == 0)
			{
				newLen = i;
				break;
			}
		}
		str = str.Substring(0, newLen);
		return str;
	}
	public static bool isMemoryEqual(byte[] buffer0, byte[] buffer1, int length, int offset0 = 0, int offset1 = 0)
	{
		// 如果长度不足,则返回失败
		if (offset0 + length > buffer0.Length || offset1 + length > buffer1.Length)
		{
			return false;
		}
		for (int i = 0; i < length; ++i)
		{
			if (buffer0[i + offset0] != buffer1[i + offset1])
			{
				return false;
			}
		}
		return true;
	}
	public static int getBit(byte value, int pos) { return ((value & (1 << pos)) >> pos) & 1; }
	public static int getBit(ushort value, int pos) { return ((value & (1 << pos)) >> pos) & 1; }
	public static int getBit(int value, int pos) { return ((value & (1 << pos)) >> pos) & 1; }
	public static int getBit(uint value, int pos) { return (int)(((value & (1 << pos)) >> pos) & 1); }
	public static int getBit(long value, int pos) { return (int)(((value & (1 << pos)) >> pos) & 1); }
	public static int getBit(ulong value, int pos) { return (int)(((value & (ulong)(1 << pos)) >> pos) & 1); }
	public static int getLowestBit(byte value) { return value & 1; }
	public static int getLowestBit(short value) { return value & 1; }
	public static int getLowestBit(int value) { return value & 1; }
	public static int getHighestBit(byte value) { return (value & ~0x7F) >> (8 * sizeof(byte) - 1) & 1; }
	public static int getHighestBit(short value) { return (value & ~0x7FFF) >> (8 * sizeof(short) - 1) & 1; }
	public static int getHighestBit(int value) { return (value & ~0x7FFFFFFF) >> (8 * sizeof(int) - 1) & 1; }
	public static void setBit(ref byte value, int pos, int bit) { value = (byte)(value & ~(1 << pos) | (bit << pos)); }
	public static void setBit(ref short value, int pos, int bit) { value = (short)(value & ~(1 << pos) | (bit << pos)); }
	public static void setBit(ref ushort value, int pos, int bit) { value = (ushort)(value & ~(1 << pos) | (bit << pos)); }
	public static void setBit(ref int value, int pos, int bit) { value = value & ~(1 << pos) | (bit << pos); }
	public static void setBit(ref uint value, int pos, int bit) { value = (uint)(value & ~(1 << pos) | (uint)(bit << pos)); }
	public static void setBit(ref long value, int pos, int bit) { value = (value & ~(1 << pos) | (long)(bit << pos)); }
	public static void setBit(ref ulong value, int pos, int bit) { value = (ulong)((long)(value & (ulong)~(1 << pos)) | (long)(bit << pos)); }
	public static void setLowestBit(ref byte value, int bit)
	{
		if (bit == 0)
		{
			value = (byte)(value & 0xFE);
		}
		else
		{
			value = (byte)(value | 1);
		}
	}
	public static void setLowestBit(ref short value, int bit)
	{
		if (bit == 0)
		{
			value = (short)(value & 0xFFFE);
		}
		else
		{
			value = (short)(value | 1);
		}
	}
	public static void setLowestBit(ref int value, int bit)
	{
		if (bit == 0)
		{
			value >>= 1;
			value <<= 1;
		}
		else
		{
			value |= 1;
		}
	}
	public static void setHighestBit(ref byte value, int bit)
	{
		if (bit == 0)
		{
			value = (byte)(value & 0x7F);
		}
		else
		{
			value = (byte)(value | 0x80);
		}
	}
	public static void setHighestBit(ref short value, int bit)
	{
		if (bit == 0)
		{
			value = (short)(value & 0x7FFF);
		}
		else
		{
			value = (short)(value | 0x8000);
		}
	}
	public static void setHighestBit(ref int value, int bit)
	{
		if (bit == 0)
		{
			value &= 0x7FFFFFFF;
		}
		else
		{
			value = (int)(value | 0x80000000);
		}
	}
}