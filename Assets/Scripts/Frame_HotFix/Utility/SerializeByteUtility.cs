using System;
using UnityEngine;
using static BinaryUtility;
using static MathUtility;

// 按字节序列化的工具函数
public class SerializeByteUtility
{
	public static bool readBool(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(bool);
		if (!success)
		{
			return false;
		}
		return buffer[index++] != 0;
	}
	public static sbyte readSByte(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(sbyte);
		if (!success)
		{
			return 0;
		}
		return (sbyte)buffer[index++];
	}
	public static byte readByte(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(byte);
		if (!success)
		{
			return 0;
		}
		return buffer[index++];
	}
	public static short readShort(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(short);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(short);
		return bytesToShort(buffer[pre + 0], buffer[pre + 1]);
	}
	public static short readShortBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(short);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(short);
		return bytesToShortBigEndian(buffer[pre + 0], buffer[pre + 1]);
	}
	public static ushort readUShort(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ushort);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ushort);
		return bytesToUShort(buffer[pre + 0], buffer[pre + 1]);
	}
	public static ushort readUShortBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ushort);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ushort);
		return bytesToUShortBigEndian(buffer[pre + 0], buffer[pre + 1]);
	}
	public static int readInt(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(int);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(int);
		return bytesToInt(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static int readIntBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(int);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(int);
		return bytesToIntBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static uint readUInt(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(uint);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(uint);
		return bytesToUInt(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static uint readUIntBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(uint);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(uint);
		return bytesToUIntBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static long readLong(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(long);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(long);
		return bytesToLong(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
							buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static long readLongBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(long);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(long);
		return bytesToLongBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
									buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static ulong readULong(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ulong);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ulong);
		return bytesToULong(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
							buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static ulong readULongBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ulong);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ulong);
		return bytesToULongBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
									buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static float readFloat(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(float);
		if (!success)
		{
			return 0.0f;
		}
		byte[] floatBuffer = helpBuffer;
		floatBuffer ??= new byte[sizeof(float)];
		floatBuffer[0] = buffer[index + 0];
		floatBuffer[1] = buffer[index + 1];
		floatBuffer[2] = buffer[index + 2];
		floatBuffer[3] = buffer[index + 3];
		index += sizeof(float);
		return bytesToFloat(floatBuffer);
	}
	public static float readFloatBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(float);
		if (!success)
		{
			return 0.0f;
		}
		byte[] floatBuffer = helpBuffer;
		floatBuffer ??= new byte[sizeof(float)];
		floatBuffer[0] = buffer[index + 3];
		floatBuffer[1] = buffer[index + 2];
		floatBuffer[2] = buffer[index + 1];
		floatBuffer[3] = buffer[index + 0];
		index += sizeof(float);
		return bytesToFloat(floatBuffer);
	}
	public static double readDouble(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(double);
		if (!success)
		{
			return 0.0f;
		}
		byte[] doubleBuffer = helpBuffer;
		doubleBuffer ??= new byte[sizeof(double)];
		doubleBuffer[0] = buffer[index + 0];
		doubleBuffer[1] = buffer[index + 1];
		doubleBuffer[2] = buffer[index + 2];
		doubleBuffer[3] = buffer[index + 3];
		doubleBuffer[4] = buffer[index + 4];
		doubleBuffer[5] = buffer[index + 5];
		doubleBuffer[6] = buffer[index + 6];
		doubleBuffer[7] = buffer[index + 7];
		index += sizeof(double);
		return bytesToDouble(doubleBuffer);
	}
	public static double readDoubleBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(double);
		if (!success)
		{
			return 0.0f;
		}
		byte[] doubleBuffer = helpBuffer;
		doubleBuffer ??= new byte[sizeof(double)];
		doubleBuffer[0] = buffer[index + 7];
		doubleBuffer[1] = buffer[index + 6];
		doubleBuffer[2] = buffer[index + 5];
		doubleBuffer[3] = buffer[index + 4];
		doubleBuffer[4] = buffer[index + 3];
		doubleBuffer[5] = buffer[index + 2];
		doubleBuffer[6] = buffer[index + 1];
		doubleBuffer[7] = buffer[index + 0];
		index += sizeof(double);
		return bytesToDouble(doubleBuffer);
	}
	// readCount表示读取的bool的个数,小于0表示按照destBuffer数组长度读取
	public static bool readBools(byte[] buffer, int bufferSize, ref int index, bool[] destBuffer, int readCount = -1)
	{
		int boolCount = readCount < 0 ? destBuffer.Length : readCount;
		if (boolCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < boolCount; ++i)
		{
			destBuffer[i] = readBool(buffer, bufferSize, ref index, out bool success);
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
		readSize = getMin(readSize, destBufferSize);
		if (readSize > 0)
		{
			memcpy(destBuffer, buffer, 0, index, readSize);
			index += readSize;
		}
		return true;
	}
	// readCount表示要读取的short个数,小于0表示使用数组长度
	public static bool readShorts(byte[] buffer, int bufferSize, ref int index, short[] destBuffer, int readCount = -1)
	{
		int shortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (shortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < shortCount; ++i)
		{
			destBuffer[i] = readShort(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的short个数,小于0表示使用数组长度
	public static bool readShortsBigEndian(byte[] buffer, int bufferSize, ref int index, short[] destBuffer, int readCount = -1)
	{
		int shortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (shortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < shortCount; ++i)
		{
			destBuffer[i] = readShortBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ushort个数,小于0表示使用数组长度
	public static bool readUShorts(byte[] buffer, int bufferSize, ref int index, ushort[] destBuffer, int readCount = -1)
	{
		int ushortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (ushortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < ushortCount; ++i)
		{
			destBuffer[i] = readUShort(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ushort个数,小于0表示使用数组长度
	public static bool readUShortsBigEndian(byte[] buffer, int bufferSize, ref int index, ushort[] destBuffer, int readCount = -1)
	{
		int ushortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (ushortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < ushortCount; ++i)
		{
			destBuffer[i] = readUShortBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的int个数,小于0表示使用destBuffer数组长度
	public static bool readInts(byte[] buffer, int bufferSize, ref int index, int[] destBuffer, int readCount = -1)
	{
		int intCount = readCount < 0 ? destBuffer.Length : readCount;
		if (intCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < intCount; ++i)
		{
			destBuffer[i] = readInt(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的int个数,小于0表示使用destBuffer数组长度
	public static bool readIntsBigEndian(byte[] buffer, int bufferSize, ref int index, int[] destBuffer, int readCount = -1)
	{
		int intCount = readCount < 0 ? destBuffer.Length : readCount;
		if (intCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < intCount; ++i)
		{
			destBuffer[i] = readIntBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的uint个数,小于0表示使用数组长度
	public static bool readUInts(byte[] buffer, int bufferSize, ref int index, uint[] destBuffer, int readCount = -1)
	{
		int uintCount = readCount < 0 ? destBuffer.Length : readCount;
		if (uintCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < uintCount; ++i)
		{
			destBuffer[i] = readUInt(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的uint个数,小于0表示使用数组长度
	public static bool readUIntsBigEndian(byte[] buffer, int bufferSize, ref int index, uint[] destBuffer, int readCount = -1)
	{
		int uintCount = readCount < 0 ? destBuffer.Length : readCount;
		if (uintCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < uintCount; ++i)
		{
			destBuffer[i] = readUIntBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的long个数,小于0表示使用数组长度
	public static bool readLongs(byte[] buffer, int bufferSize, ref int index, long[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readLong(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的long个数,小于0表示使用数组长度
	public static bool readLongsBigEndian(byte[] buffer, int bufferSize, ref int index, long[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readLongBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ulong个数,小于0表示使用数组长度
	public static bool readULongs(byte[] buffer, int bufferSize, ref int index, ulong[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readULong(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ulong个数,小于0表示使用数组长度
	public static bool readULongsBigEndian(byte[] buffer, int bufferSize, ref int index, ulong[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readULongBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的float的数量,小于0表示使用数组的长度
	public static bool readFloats(byte[] buffer, int bufferSize, ref int index, float[] destBuffer, int readCount = -1)
	{
		int floatCount = readCount < 0 ? destBuffer.Length : readCount;
		if (floatCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < floatCount; ++i)
		{
			destBuffer[i] = readFloat(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的float的数量,小于0表示使用数组的长度
	public static bool readFloatsBigEndian(byte[] buffer, int bufferSize, ref int index, float[] destBuffer, int readCount = -1)
	{
		int floatCount = readCount < 0 ? destBuffer.Length : readCount;
		if (floatCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < floatCount; ++i)
		{
			destBuffer[i] = readFloatBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的double的数量,小于0表示使用数组的长度
	public static bool readDoubles(byte[] buffer, int bufferSize, ref int index, double[] destBuffer, int readCount = -1)
	{
		int doubleCount = readCount < 0 ? destBuffer.Length : readCount;
		if (doubleCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < doubleCount; ++i)
		{
			destBuffer[i] = readDouble(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的double的数量,小于0表示使用数组的长度
	public static bool readDoublesBigEndian(byte[] buffer, int bufferSize, ref int index, double[] destBuffer, int readCount = -1)
	{
		int doubleCount = readCount < 0 ? destBuffer.Length : readCount;
		if (doubleCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < doubleCount; ++i)
		{
			destBuffer[i] = readDoubleBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeBool(byte[] buffer, int bufferSize, ref int index, bool value)
	{
		if (bufferSize < index + sizeof(bool))
		{
			return false;
		}
		buffer[index++] = (byte)(value ? 1 : 0);
		return true;
	}
	public static bool writeByte(byte[] buffer, int bufferSize, ref int index, byte value)
	{
		if (bufferSize < index + sizeof(byte))
		{
			return false;
		}
		buffer[index++] = value;
		return true;
	}
	public static bool writeShort(byte[] buffer, int bufferSize, ref int index, short value)
	{
		if (bufferSize < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index + 1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		index += 2;
		return true;
	}
	public static bool writeShortBigEndian(byte[] buffer, int bufferSize, ref int index, short value)
	{
		if (bufferSize < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index + 1] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		index += 2;
		return true;
	}
	public static bool writeUShort(byte[] buffer, int bufferSize, ref int index, ushort value)
	{
		if (bufferSize < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index + 1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		index += 2;
		return true;
	}
	public static bool writeUShortBigEndian(byte[] buffer, int bufferSize, ref int index, ushort value)
	{
		if (bufferSize < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index + 1] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		index += 2;
		return true;
	}
	public static bool writeInt(byte[] buffer, int bufferSize, ref int index, int value)
	{
		if (bufferSize < index + sizeof(int))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index + 1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index + 2] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index + 3] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		index += 4;
		return true;
	}
	public static bool writeIntBigEndian(byte[] buffer, int bufferSize, ref int index, int value)
	{
		if (bufferSize < index + sizeof(int))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index + 1] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index + 2] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index + 3] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		index += 4;
		return true;
	}
	public static bool writeUInt(byte[] buffer, int bufferSize, ref int index, uint value)
	{
		if (bufferSize < index + sizeof(uint))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index + 1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index + 2] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index + 3] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		index += 4;
		return true;
	}
	public static bool writeUIntBigEndian(byte[] buffer, int bufferSize, ref int index, uint value)
	{
		if (bufferSize < index + sizeof(uint))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index + 1] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index + 2] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index + 3] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		index += 4;
		return true;
	}
	public static bool writeLong(byte[] buffer, int bufferSize, ref int index, long value)
	{
		if (bufferSize < index + sizeof(long))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)((value >> (8 * 0)) & 0xFF);
		buffer[index + 1] = (byte)((value >> (8 * 1)) & 0xFF);
		buffer[index + 2] = (byte)((value >> (8 * 2)) & 0xFF);
		buffer[index + 3] = (byte)((value >> (8 * 3)) & 0xFF);
		buffer[index + 4] = (byte)((value >> (8 * 4)) & 0xFF);
		buffer[index + 5] = (byte)((value >> (8 * 5)) & 0xFF);
		buffer[index + 6] = (byte)((value >> (8 * 6)) & 0xFF);
		buffer[index + 7] = (byte)((value >> (8 * 7)) & 0xFF);
		index += 8;
		return true;
	}
	public static bool writeLongBigEndian(byte[] buffer, int bufferSize, ref int index, long value)
	{
		if (bufferSize < index + sizeof(long))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)((value >> (8 * 7)) & 0xFF);
		buffer[index + 1] = (byte)((value >> (8 * 6)) & 0xFF);
		buffer[index + 2] = (byte)((value >> (8 * 5)) & 0xFF);
		buffer[index + 3] = (byte)((value >> (8 * 4)) & 0xFF);
		buffer[index + 4] = (byte)((value >> (8 * 3)) & 0xFF);
		buffer[index + 5] = (byte)((value >> (8 * 2)) & 0xFF);
		buffer[index + 6] = (byte)((value >> (8 * 1)) & 0xFF);
		buffer[index + 7] = (byte)((value >> (8 * 0)) & 0xFF);
		index += 8;
		return true;
	}
	public static bool writeULong(byte[] buffer, int bufferSize, ref int index, ulong value)
	{
		if (bufferSize < index + sizeof(ulong))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)((value >> (8 * 0)) & 0xFF);
		buffer[index + 1] = (byte)((value >> (8 * 1)) & 0xFF);
		buffer[index + 2] = (byte)((value >> (8 * 2)) & 0xFF);
		buffer[index + 3] = (byte)((value >> (8 * 3)) & 0xFF);
		buffer[index + 4] = (byte)((value >> (8 * 4)) & 0xFF);
		buffer[index + 5] = (byte)((value >> (8 * 5)) & 0xFF);
		buffer[index + 6] = (byte)((value >> (8 * 6)) & 0xFF);
		buffer[index + 7] = (byte)((value >> (8 * 7)) & 0xFF);
		index += 8;
		return true;
	}
	public static bool writeULongBigEndian(byte[] buffer, int bufferSize, ref int index, ulong value)
	{
		if (bufferSize < index + sizeof(ulong))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = (byte)((value >> (8 * 7)) & 0xFF);
		buffer[index + 1] = (byte)((value >> (8 * 6)) & 0xFF);
		buffer[index + 2] = (byte)((value >> (8 * 5)) & 0xFF);
		buffer[index + 3] = (byte)((value >> (8 * 4)) & 0xFF);
		buffer[index + 4] = (byte)((value >> (8 * 3)) & 0xFF);
		buffer[index + 5] = (byte)((value >> (8 * 2)) & 0xFF);
		buffer[index + 6] = (byte)((value >> (8 * 1)) & 0xFF);
		buffer[index + 7] = (byte)((value >> (8 * 0)) & 0xFF);
		index += 8;
		return true;
	}
	public static bool writeFloat(byte[] buffer, int bufferSize, ref int index, float value)
	{
		if (bufferSize < index + sizeof(float))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = valueByte[0];
		buffer[index + 1] = valueByte[1];
		buffer[index + 2] = valueByte[2];
		buffer[index + 3] = valueByte[3];
		index += 4;
		return true;
	}
	public static bool writeFloatBigEndian(byte[] buffer, int bufferSize, ref int index, float value)
	{
		if (bufferSize < index + sizeof(float))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = valueByte[3];
		buffer[index + 1] = valueByte[2];
		buffer[index + 2] = valueByte[1];
		buffer[index + 3] = valueByte[0];
		index += 4;
		return true;
	}
	public static bool writeDouble(byte[] buffer, int bufferSize, ref int index, double value)
	{
		if (bufferSize < index + sizeof(double))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = valueByte[0];
		buffer[index + 1] = valueByte[1];
		buffer[index + 2] = valueByte[2];
		buffer[index + 3] = valueByte[3];
		buffer[index + 4] = valueByte[4];
		buffer[index + 5] = valueByte[5];
		buffer[index + 6] = valueByte[6];
		buffer[index + 7] = valueByte[7];
		index += 8;
		return true;
	}
	public static bool writeDoubleBigEndian(byte[] buffer, int bufferSize, ref int index, double value)
	{
		if (bufferSize < index + sizeof(double))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index + 0] = valueByte[7];
		buffer[index + 1] = valueByte[6];
		buffer[index + 2] = valueByte[5];
		buffer[index + 3] = valueByte[4];
		buffer[index + 4] = valueByte[3];
		buffer[index + 5] = valueByte[2];
		buffer[index + 6] = valueByte[1];
		buffer[index + 7] = valueByte[0];
		index += 8;
		return true;
	}
	public static bool writeVector2(byte[] buffer, int bufferSize, ref int index, Vector2 value)
	{
		bool result = writeFloat(buffer, bufferSize, ref index, value.x);
		result = result && writeFloat(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector2UShort(byte[] buffer, int bufferSize, ref int index, Vector2UShort value)
	{
		bool result = writeUShort(buffer, bufferSize, ref index, value.x);
		result = result && writeUShort(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector2Short(byte[] buffer, int bufferSize, ref int index, Vector2Short value)
	{
		bool result = writeShort(buffer, bufferSize, ref index, value.x);
		result = result && writeShort(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector2Int(byte[] buffer, int bufferSize, ref int index, Vector2Int value)
	{
		bool result = writeInt(buffer, bufferSize, ref index, value.x);
		result = result && writeInt(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector2UInt(byte[] buffer, int bufferSize, ref int index, Vector2UInt value)
	{
		bool result = writeUInt(buffer, bufferSize, ref index, value.x);
		result = result && writeUInt(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector3(byte[] buffer, int bufferSize, ref int index, Vector3 value)
	{
		bool result = writeFloat(buffer, bufferSize, ref index, value.x);
		result = result && writeFloat(buffer, bufferSize, ref index, value.y);
		result = result && writeFloat(buffer, bufferSize, ref index, value.z);
		return result;
	}
	public static bool writeVector4(byte[] buffer, int bufferSize, ref int index, Vector4 value)
	{
		bool result = writeFloat(buffer, bufferSize, ref index, value.x);
		result = result && writeFloat(buffer, bufferSize, ref index, value.y);
		result = result && writeFloat(buffer, bufferSize, ref index, value.z);
		result = result && writeFloat(buffer, bufferSize, ref index, value.w);
		return result;
	}
	// writeCount表示要写入的bool个数,小于0表示将整个数组全部写入
	public static bool writeBools(byte[] buffer, int bufferSize, ref int index, bool[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int boolCount = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < boolCount; ++i)
		{
			ret = writeBool(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	public static bool writeBytes(byte[] buffer, ref int index, byte[] sourceBuffer, int bufferSize = -1, int sourceBufferSize = -1, int writeSize = -1)
	{
		if (bufferSize == 0 || buffer == null)
		{
			return true;
		}
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
	public static bool writeShorts(byte[] buffer, int bufferSize, ref int index, short[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeShort(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的short个数,小于0表示写入整个数组
	public static bool writeShortsBigEndian(byte[] buffer, int bufferSize, ref int index, short[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeShortBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ushort个数,小于0表示写入整个数组
	public static bool writeUShorts(byte[] buffer, int bufferSize, ref int index, ushort[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUShort(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ushort个数,小于0表示写入整个数组
	public static bool writeUShortsBigEndian(byte[] buffer, int bufferSize, ref int index, ushort[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUShortBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的int个数,小于0表示写入整个数组
	public static bool writeInts(byte[] buffer, int bufferSize, ref int index, int[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeInt(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的int个数,小于0表示写入整个数组
	public static bool writeIntsBigEndian(byte[] buffer, int bufferSize, ref int index, int[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeIntBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的uint个数,小于0表示写入整个数组
	public static bool writeUInts(byte[] buffer, int bufferSize, ref int index, uint[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUInt(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的uint个数,小于0表示写入整个数组
	public static bool writeUIntsBigEndian(byte[] buffer, int bufferSize, ref int index, uint[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUIntBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的long的个数,小于0表示写入整个数组
	public static bool writeLongs(byte[] buffer, int bufferSize, ref int index, long[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeLong(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的long的个数,小于0表示写入整个数组
	public static bool writeLongsBigEndian(byte[] buffer, int bufferSize, ref int index, long[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeLongBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ulong的个数,小于0表示写入整个数组
	public static bool writeULongs(byte[] buffer, int bufferSize, ref int index, ulong[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeULong(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ulong的个数,小于0表示写入整个数组
	public static bool writeULongsBigEndian(byte[] buffer, int bufferSize, ref int index, ulong[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeULongBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的浮点数的数量,小于0表示写入整个数组
	public static bool writeFloats(byte[] buffer, int bufferSize, ref int index, float[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeFloat(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的浮点数的数量,小于0表示写入整个数组
	public static bool writeFloatsBigEndian(byte[] buffer, int bufferSize, ref int index, float[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeFloatBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	public static void ushortToBytes(ushort value, byte[] bytes)
	{
		if (bytes.Length != sizeof(ushort))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
	}
	public static void ushortToBytes(ushort value, out byte byte0, out byte byte1)
	{
		// 为了获得最快速度,不使用for循环
		byte0 = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		byte1 = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
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
	public static void shortToBytes(short value, out byte byte0, out byte byte1)
	{
		// 为了获得最快速度,不使用for循环
		byte0 = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		byte1 = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
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
	public static void intToBytes(int value, out byte byte0, out byte byte1, out byte byte2, out byte byte3)
	{
		byte0 = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		byte1 = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		byte2 = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		byte3 = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
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
	public static byte[] toBytes(double value)
	{
		return BitConverter.GetBytes(value);
	}
	public static byte bytesToByte(byte[] array)
	{
		return array[0];
	}
	public static byte bytesToByte(Span<byte> array)
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
	public static short bytesToShort(Span<byte> array)
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
	public static short bytesToShortBigEndian(Span<byte> array)
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
	public static ushort bytesToUShort(Span<byte> array)
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
	public static ushort bytesToUShortBigEndian(Span<byte> array)
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
	public static int bytesToInt(Span<byte> array)
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
	public static int bytesToIntBigEndian(Span<byte> array)
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
	public static uint bytesToUInt(Span<byte> array)
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
	public static uint bytesToUIntBigEndian(Span<byte> array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUIntBigEndian(array[0], array[1], array[2], array[3]);
	}
	public static uint bytesToUInt(byte b0, byte b1, byte b2, byte b3)
	{
		return (uint)((b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0)));
	}
	public static uint bytesToUIntBigEndian(byte b0, byte b1, byte b2, byte b3)
	{
		return (uint)((b3 << (8 * 0)) | (b2 << (8 * 1)) | (b1 << (8 * 2)) | (b0 << (8 * 3)));
	}
	public static long bytesToLong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 7) | (long)b6 << (8 * 6) | (long)b5 << (8 * 5) | (long)b4 << (8 * 4) |
				(long)b3 << (8 * 3) | (long)b2 << (8 * 2) | (long)b1 << (8 * 1) | (long)b0 << (8 * 0);
	}
	public static long bytesToLongBigEndian(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 0) | (long)b6 << (8 * 1) | (long)b5 << (8 * 2) | (long)b4 << (8 * 3) |
				(long)b3 << (8 * 4) | (long)b2 << (8 * 5) | (long)b1 << (8 * 6) | (long)b0 << (8 * 7);
	}
	public static ulong bytesToULong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 7) | (ulong)b6 << (8 * 6) | (ulong)b5 << (8 * 5) | (ulong)b4 << (8 * 4) |
				(ulong)b3 << (8 * 3) | (ulong)b2 << (8 * 2) | (ulong)b1 << (8 * 1) | (ulong)b0 << (8 * 0);
	}
	public static ulong bytesToULongBigEndian(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 0) | (ulong)b6 << (8 * 1) | (ulong)b5 << (8 * 2) | (ulong)b4 << (8 * 3) |
				(ulong)b3 << (8 * 4) | (ulong)b2 << (8 * 5) | (ulong)b1 << (8 * 6) | (ulong)b0 << (8 * 7);
	}
	public static float bytesToFloat(byte[] array)
	{
		if (array == null)
		{
			return 0.0f;
		}
		return BitConverter.ToSingle(array, 0);
	}
	public static double bytesToDouble(byte[] array)
	{
		if (array == null)
		{
			return 0.0;
		}
		return BitConverter.ToDouble(array, 0);
	}
}
