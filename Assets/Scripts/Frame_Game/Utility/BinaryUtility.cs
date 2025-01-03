using System;
using System.Text;
using UnityEngine;
using static StringUtility;
using static MathUtility;

// 与二进制相关的工具函数
public class BinaryUtility
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
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		return true;
	}
	public static bool writeShortBigEndian(byte[] buffer, int bufferSize, ref int index, short value)
	{
		if (bufferSize < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeUShort(byte[] buffer, int bufferSize, ref int index, ushort value)
	{
		if (bufferSize < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		return true;
	}
	public static bool writeUShortBigEndian(byte[] buffer, int bufferSize, ref int index, ushort value)
	{
		if (bufferSize < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeInt(byte[] buffer, int bufferSize, ref int index, int value)
	{
		if (bufferSize < index + sizeof(int))
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
	public static bool writeIntBigEndian(byte[] buffer, int bufferSize, ref int index, int value)
	{
		if (bufferSize < index + sizeof(int))
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
	public static bool writeUInt(byte[] buffer, int bufferSize, ref int index, uint value)
	{
		if (bufferSize < index + sizeof(uint))
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
	public static bool writeUIntBigEndian(byte[] buffer, int bufferSize, ref int index, uint value)
	{
		if (bufferSize < index + sizeof(uint))
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
	public static bool writeLong(byte[] buffer, int bufferSize, ref int index, long value)
	{
		if (bufferSize < index + sizeof(long))
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
	public static bool writeLongBigEndian(byte[] buffer, int bufferSize, ref int index, long value)
	{
		if (bufferSize < index + sizeof(long))
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
	public static bool writeULong(byte[] buffer, int bufferSize, ref int index, ulong value)
	{
		if (bufferSize < index + sizeof(ulong))
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
	public static bool writeULongBigEndian(byte[] buffer, int bufferSize, ref int index, ulong value)
	{
		if (bufferSize < index + sizeof(ulong))
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
	public static bool writeFloat(byte[] buffer, int bufferSize, ref int index, float value)
	{
		if (bufferSize < index + sizeof(float))
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
	public static bool writeFloatBigEndian(byte[] buffer, int bufferSize, ref int index, float value)
	{
		if (bufferSize < index + sizeof(float))
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
	public static bool writeDouble(byte[] buffer, int bufferSize, ref int index, double value)
	{
		if (bufferSize < index + sizeof(double))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[0];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[3];
		buffer[index++] = valueByte[4];
		buffer[index++] = valueByte[5];
		buffer[index++] = valueByte[6];
		buffer[index++] = valueByte[7];
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
		buffer[index++] = valueByte[7];
		buffer[index++] = valueByte[6];
		buffer[index++] = valueByte[5];
		buffer[index++] = valueByte[4];
		buffer[index++] = valueByte[3];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[0];
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
	public static void memcpy<T>(Span<T> dest, T[] src, int destOffset, int srcOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[srcOffset + i];
		}
	}
	public static void memcpy<T>(T[] dest, T[] src, int destByteOffset, int srcByteOffset, int byteCount)
	{
		Buffer.BlockCopy(src, srcByteOffset, dest, destByteOffset, byteCount);
	}
	// 将数组每个元素值设置为指定值
	public static void memset<T>(T[] p, T value, int length = -1)
	{
		if (p == null)
		{
			return;
		}
		if (length == -1)
		{
			length = p.Length;
		}
		for (int i = 0; i < length; ++i)
		{
			p[i] = value;
		}
	}
	public static byte[] toBytes(float value)
	{
		return BitConverter.GetBytes(value);
	}
	public static byte[] toBytes(double value)
	{
		return BitConverter.GetBytes(value);
	}
	public static short bytesToShort(byte byte0, byte byte1) { return (short)((byte1 << (8 * 1)) | (byte0 << (8 * 0))); }
	public static ushort bytesToUShort(byte b0, byte b1) { return (ushort)((b1 << (8 * 1)) | (b0 << (8 * 0))); }
	public static int bytesToInt(byte b0, byte b1, byte b2, byte b3) { return (b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0)); }
	public static uint bytesToUInt(byte b0, byte b1, byte b2, byte b3)
	{
		return (uint)((b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0)));
	}
	public static long bytesToLong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 7) | (long)b6 << (8 * 6) | (long)b5 << (8 * 5) | (long)b4 << (8 * 4) |
				(long)b3 << (8 * 3) | (long)b2 << (8 * 2) | (long)b1 << (8 * 1) | (long)b0 << (8 * 0);
	}
	public static ulong bytesToULong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 7) | (ulong)b6 << (8 * 6) | (ulong)b5 << (8 * 5) | (ulong)b4 << (8 * 4) |
				(ulong)b3 << (8 * 3) | (ulong)b2 << (8 * 2) | (ulong)b1 << (8 * 1) | (ulong)b0 << (8 * 0);
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
	public static byte[] stringToBytes(string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		// 默认为UTF8
		encoding ??= Encoding.UTF8;
		return encoding.GetBytes(str);
	}
	public static string bytesToString(byte[] bytes, Encoding encoding = null)
	{
		if (bytes == null)
		{
			return null;
		}
		if (bytes.Length == 0)
		{
			return EMPTY;
		}
		// 默认为UTF8
		encoding ??= Encoding.UTF8;
		return removeLastZero(encoding.GetString(bytes));
	}
	public static string bytesToString(Span<byte> bytes, Encoding encoding = null)
	{
		if (bytes == null)
		{
			return null;
		}
		if (bytes.Length == 0)
		{
			return EMPTY;
		}
		// 默认为UTF8
		encoding ??= Encoding.UTF8;
		return removeLastZero(encoding.GetString(bytes));
	}
	public static string bytesToString(byte[] bytes, int startIndex, int count, Encoding encoding = null)
	{
		if (bytes == null || count < 0)
		{
			return null;
		}
		if (bytes.Length == 0 || count == 0 || startIndex + count > bytes.Length)
		{
			return EMPTY;
		}
		// 默认为UTF8
		encoding ??= Encoding.UTF8;
		return removeLastZero(encoding.GetString(bytes, startIndex, count));
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
}