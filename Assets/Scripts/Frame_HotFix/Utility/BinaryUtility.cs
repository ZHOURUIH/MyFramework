using System;
using System.Text;

// 与二进制相关的工具函数
public class BinaryUtility
{
	protected static Encoding ENCODING_GB2312;
	protected static Encoding ENCODING_GBK;
	// CRC table for the CRC-16. The poly is 0x8005 (x^16 + x^15 + x^2 + 1)
	protected static ushort[] mCRC16Table;
	protected static ThreadLock mCRC16TableLock = new();
	public static Encoding getGB2312()
	{
		return ENCODING_GB2312 ??= Encoding.GetEncoding("gb2312");
	}
	public static Encoding getGBK()
	{
		return ENCODING_GBK ??= Encoding.GetEncoding("GBK");
	}
	// 计算 16进制的c中1的个数
	public static int bitCount1(byte c)
	{
		int count = 0;
		int bitCount = sizeof(char) * 8;
		for (int i = 0; i < bitCount; ++i)
		{
			if ((c & (1 << i)) > 0)
			{
				++count;
			}
		}
		return count;
	}
	// 字节数组buffer中是否包含字节数组key,类似于string.Contains
	public static bool contains(byte[] buffer, byte[] key)
	{
		if (buffer == null || key == null)
		{
			return false;
		}
		int keyLength = key.Length;
		int bufferLength = buffer.Length - keyLength + 1;
		for (int i = 0; i < bufferLength; ++i)
		{
			bool find = true;
			for (int j = 0; j < keyLength; ++j)
			{
				if (buffer[i + j] != key[j])
				{
					find = false;
					break;
				}
			}
			if (find)
			{
				return true;
			}
		}
		return false;
	}
	public static ushort crc16(ushort crc, byte[] buffer, int len, int bufferOffset = 0)
	{
		for (int i = 0; i < len; ++i)
		{
			crc = crc16_byte(crc, buffer[bufferOffset + i]);
		}
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0)
	{
		crc = crc16_byte(crc, byte0);
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0, byte byte1)
	{
		crc = crc16_byte(crc, byte0);
		crc = crc16_byte(crc, byte1);
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0, byte byte1, byte byte2, byte byte3)
	{
		crc = crc16_byte(crc, byte0);
		crc = crc16_byte(crc, byte1);
		crc = crc16_byte(crc, byte2);
		crc = crc16_byte(crc, byte3);
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7)
	{
		crc = crc16_byte(crc, byte0);
		crc = crc16_byte(crc, byte1);
		crc = crc16_byte(crc, byte2);
		crc = crc16_byte(crc, byte3);
		crc = crc16_byte(crc, byte4);
		crc = crc16_byte(crc, byte5);
		crc = crc16_byte(crc, byte6);
		crc = crc16_byte(crc, byte7);
		return crc;
	}
	public static ushort crc16_byte(ushort crc, byte data)
	{
		if (mCRC16Table == null)
		{
			initCRC16();
		}
		return (ushort)((crc >> 8) ^ mCRC16Table[(crc ^ data) & 0xFF]);
	}
	public static void memcpyObject<T>(T[] dest, T[] src, int destOffset, int srcOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[srcOffset + i];
		}
	}
	public static void memcpy<T>(T[] dest, Span<T> src, int destOffset, int srcOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[srcOffset + i];
		}
	}
	public static void memcpy<T>(T[] dest, Span<T> src, int destOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[i];
		}
	}
	public static void memcpy<T>(Span<T> dest, T[] src, int destOffset, int srcOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[srcOffset + i];
		}
	}
	public static void memcpy<T>(Span<T> dest, T[] src, int destOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[i];
		}
	}
	public static void memcpy<T>(T[] dest, T[] src, int destByteOffset, int srcByteOffset, int byteCount)
	{
		Buffer.BlockCopy(src, srcByteOffset, dest, destByteOffset, byteCount);
	}
	public static void memmove<T>(T[] data, int dest, int src, int count)
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
	public static void memset<T>(T[] p, T value, int startIndex, int length)
	{
		if (length == -1 || length > p.Length - startIndex)
		{
			length = p.Length - startIndex;
		}
		for (int i = 0; i < length; ++i)
		{
			p[startIndex + i] = value;
		}
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
		return str.startString(newLen);
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
	public static bool hasBit(byte value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(sbyte value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(short value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(ushort value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(int value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(uint value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(long value, int pos) { return (value & ((long)1 << pos)) != 0; }
	public static bool hasBit(ulong value, int pos) { return (value & ((ulong)1 << pos)) != 0; }
	public static int getLowestBit(byte value) { return value & 1; }
	public static int getLowestBit(short value) { return value & 1; }
	public static int getLowestBit(int value) { return value & 1; }
	public static int getHighestBit(byte value) { return (value & ~0x7F) >> (8 * sizeof(byte) - 1) & 1; }
	public static int getHighestBit(short value) { return (value & ~0x7FFF) >> (8 * sizeof(short) - 1) & 1; }
	public static int getHighestBit(int value) { return (value & ~0x7FFFFFFF) >> (8 * sizeof(int) - 1) & 1; }
	// 设置位的函数不能传负数,因为负数的存储是使用补码,所以不能直接进行按位或,需要转换位正数再操作
	public static void setBitOne(ref byte value, int pos) { value |= (byte)(1 << pos); }
	public static void setBitOne(ref sbyte value, int pos) { value |= (sbyte)(1 << pos); }
	public static void setBitOne(ref short value, int pos) { value |= (short)(1 << pos); }
	public static void setBitOne(ref ushort value, int pos) { value |= (ushort)(1 << pos); }
	public static void setBitOne(ref int value, int pos) { value |= 1 << pos; }
	public static void setBitOne(ref uint value, int pos) { value |= (uint)(1 << pos); }
	public static void setBitOne(ref long value, int pos) { value |= (long)1 << pos; }
	public static void setBitOne(ref ulong value, int pos) { value |= (ulong)1 << pos; }
	public static void setBitZero(ref byte value, int pos) { value &= (byte)~(1 << pos); }
	public static void setBitZero(ref sbyte value, int pos) { value &= (sbyte)~(1 << pos); }
	public static void setBitZero(ref short value, int pos) { value &= (short)~(1 << pos); }
	public static void setBitZero(ref ushort value, int pos) { value &= (ushort)~(1 << pos); }
	public static void setBitZero(ref int value, int pos) { value &= ~(1 << pos); }
	public static void setBitZero(ref uint value, int pos) { value &= (uint)~(1 << pos); }
	public static void setBitZero(ref long value, int pos) { value &= ~((long)1 << pos); }
	public static void setBitZero(ref ulong value, int pos) { value &= ~((ulong)1 << pos); }
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
	public static bool getBufferBit(byte[] buffer, int bitIndex) { return hasBit(buffer[bitIndex >> 3], bitIndex & 7); }
	public static void setBufferBitOne(byte[] buffer, int bitIndex) { setBitOne(ref buffer[bitIndex >> 3], bitIndex & 7); }
	//------------------------------------------------------------------------------------------------------------------------------
	// 直接写入一个无符号整数,不考虑任何情况
	protected static void initCRC16()
	{
		using (new ThreadLockScope(mCRC16TableLock))
		{
			mCRC16Table = new ushort[256]
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
	}
}