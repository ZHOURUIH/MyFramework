using System;
using System.Collections.Generic;
using static SerializeByteUtility;
using static BinaryUtility;
using static FrameUtility;

// 按位序列化的工具函数
public class SerializeBitUtility
{
	// 下标是sizeof(T),value是此T类型的bit长度所需要的长度表示
	// 比如1个char的取值范围是-127~127,去除符号以后是0~127,127占7个bit,可以使用3个bit存储这个7,所以长度位就是用3个bit来表示,所以下标1的值是3
	// 如果是byte,取值范围是0~255,255占8bit,可以使用0001表示,但是实际上超过127的情况比较少,所以可以将127以上的合并,7bit和8bit都用7bit,只不过数据位还是都读8bit
	protected static byte[] LENGTH_MAX_BIT = new byte[9] { 0, 3, 4, 0, 5, 0, 0, 0, 6 };
	// 0到65535的每个数中的最高位1的下标,也就是需要用几个bit来表示,1可以用1个bit表示,5可以使用3个bit表示,8可以使用5个bit表示
	protected static byte[] mBitCountTable;
	protected static ThreadLock mBitCountTableLock = new();
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out bool value)
	{
		// bool固定读取1位
		if (bitCountToByteCount(bitIndex + 1) > bufferSize)
		{
			value = false;
			return false;
		}
		value = getBufferBit(buffer, bitIndex);
		++bitIndex;
		return true;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out sbyte value, bool needReadSign)
	{
		value = (sbyte)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(sbyte), needReadSign);
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out byte value)
	{
		value = (byte)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(byte));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out short value, bool needReadSign)
	{
		value = (short)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(short), needReadSign);
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out ushort value)
	{
		value = (ushort)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(ushort));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out int value, bool needReadSign)
	{
		value = (int)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(int), needReadSign);
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out uint value)
	{
		value = (uint)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(uint));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out long value, bool needReadSign)
	{
		value = readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(long), needReadSign);
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out ulong value)
	{
		value = readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(ulong));
		return success;
	}
	// 因为long是最大的带符号整型类型,所以可以使用long传递任何带符号整数类型的值
	public static long readSignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, out bool success, int typeSize, bool needReadSign)
	{
		success = false;
		if (!readSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, out byte bitCount))
		{
			return 0;
		}
		if (bitCount == 0)
		{
			success = true;
			return 0;
		}
		--bitCount;
		if (bitCountToByteCount(bitIndex + bitCount + (needReadSign ? 1 : 0)) > bufferSize)
		{
			return 0;
		}

		// 读取值
		long value;
		if (bitCount > 0)
		{
			value = readSignedValueBit(buffer, ref bitIndex, bitCount, out bool isNegative, needReadSign);
			// 负数由于存储是使用补码存储的,所以不能直接按位或
			if (isNegative)
			{
				value = -value;
				setBitOne(ref value, bitCount);
				value = -value;
			}
			else
			{
				setBitOne(ref value, bitCount);
			}
		}
		else
		{
			if (needReadSign)
			{
				value = getBufferBit(buffer, bitIndex++) ? -1 : 1;
			}
			else
			{
				value = 1;
			}
		}
		success = true;
		return value;
	}
	// 因为ulong是最大的无符号整型类型,所以可以使用ulong传递任何无符号整数类型的值
	public static ulong readUnsignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, out bool success, int typeSize)
	{
		success = false;
		if (!readUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, out byte bitCount))
		{
			return 0;
		}
		if (bitCount == 0)
		{
			success = true;
			return 0;
		}
		// 32位及以下保持原协议规则,64位则以完整64bit作为不能省略最高位的边界。
		// ulong的长度字段用63同时表示63/64bit,读取到63后会被还原为64,此时必须读取完整64bit数据。
		int removeHighBitLimit = typeSize == sizeof(ulong) ? typeSize << 3 : (1 << typeSize) - 1;
		bool removeHighBit = bitCount < removeHighBitLimit;
		if (removeHighBit)
		{
			--bitCount;
		}
		if (bitCountToByteCount(bitIndex + bitCount) > bufferSize)
		{
			return 0;
		}

		// 读取值
		ulong value;
		if (bitCount > 0)
		{
			value = readUnsignedValueBit(buffer, ref bitIndex, bitCount);
			if (removeHighBit)
			{
				setBitOne(ref value, bitCount);
			}
		}
		else
		{
			value = 1;
		}
		success = true;
		return value;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<sbyte> list, bool needReadSign)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(sbyte);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = (sbyte)readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = (sbyte)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<sbyte> list, bool needReadSign)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(sbyte);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add((sbyte)readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add((sbyte)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<byte> list)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(byte);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = (byte)readUnsignedValueBit(buffer, ref bitIndex, bitCount);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = (byte)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<byte> list)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(byte);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add((byte)readUnsignedValueBit(buffer, ref bitIndex, bitCount));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add((byte)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<short> list, bool needReadSign)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(short);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = (short)readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = (short)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<short> list, bool needReadSign)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(short);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add((short)readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add((short)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<ushort> list)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(ushort);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = (ushort)readUnsignedValueBit(buffer, ref bitIndex, bitCount);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = (ushort)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<ushort> list)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(ushort);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add((ushort)readUnsignedValueBit(buffer, ref bitIndex, bitCount));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add((ushort)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<int> list, bool needReadSign)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(int);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = (int)readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = (int)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<int> list, bool needReadSign)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(int);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add((int)readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add((int)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<uint> list)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(uint);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = (uint)readUnsignedValueBit(buffer, ref bitIndex, bitCount);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = (uint)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<uint> list)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(uint);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add((uint)readUnsignedValueBit(buffer, ref bitIndex, bitCount));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add((uint)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<long> list, bool needReadSign)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(long);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<long> list, bool needReadSign)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(long);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add(readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add(readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<ulong> list)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(ulong);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list[i] = readUnsignedValueBit(buffer, ref bitIndex, bitCount);
				}
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<ulong> list)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		int typeSize = sizeof(ulong);
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readUnsignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount))
			{
				return false;
			}
			// 读取所有元素,每个元素占的bit数量固定
			if (bitCount > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					list.Add(readUnsignedValueBit(buffer, ref bitIndex, bitCount));
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		// 每个元素使用独立的长度位
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add(readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize));
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<float> list, bool needReadSign, int precision = 3)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 这里本来应该是sizeof(int),因为float最终会转换为int存储,只不过sizeof(int)跟sizeof(float)一样
		int typeSize = sizeof(float);
		float powValue = precision.inversePow10();
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			if (bitCount > 0)
			{
				// 读取所有元素,每个元素占的bit数量固定
				for (int i = 0; i < count; ++i)
				{
					list[i] = readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign) * powValue;
				}
			}
		}
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign) * powValue;
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<float> list, bool needReadSign, int precision = 3)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 这里本来应该是sizeof(int),因为float最终会转换为int存储,只不过sizeof(int)跟sizeof(float)一样
		int typeSize = sizeof(float);
		float powValue = precision.inversePow10();
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			if (bitCount > 0)
			{
				// 读取所有元素,每个元素占的bit数量固定
				for (int i = 0; i < count; ++i)
				{
					list.Add(readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign) * powValue);
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add(readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign) * powValue);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, ref Span<double> list, bool needReadSign, int precision = 4)
	{
		int count = list.Length;
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 这里本来应该是sizeof(long),因为double最终会转换为long存储,只不过sizeof(long)跟sizeof(double)一样
		int typeSize = sizeof(double);
		double powValue = 1.0.divide(precision.pow10());
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			if (bitCount > 0)
			{
				// 读取所有元素,每个元素占的bit数量固定
				for (int i = 0; i < count; ++i)
				{
					list[i] = readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign) * powValue;
				}
			}
		}
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list[i] = readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign) * powValue;
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool readListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<double> list, bool needReadSign, int precision = 4)
	{
		list.Clear();
		if (!readBit(buffer, bufferSize, ref bitIndex, out ushort count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		bool lengthBitType = getBufferBit(buffer, bitIndex++);
		// 这里本来应该是sizeof(long),因为double最终会转换为long存储,只不过sizeof(long)跟sizeof(double)一样
		int typeSize = sizeof(double);
		double powValue = 1.0.divide(precision.pow10());
		// 使用统一的长度位
		if (lengthBitType)
		{
			if (!readSignedListLengthBit(buffer, bufferSize, ref bitIndex, typeSize, count, out byte bitCount, needReadSign))
			{
				return false;
			}
			if (bitCount > 0)
			{
				// 读取所有元素,每个元素占的bit数量固定
				for (int i = 0; i < count; ++i)
				{
					list.Add(readSignedValueBit(buffer, ref bitIndex, bitCount, out _, needReadSign) * powValue);
				}
			}
			else
			{
				list.addCount(count);
			}
		}
		else
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add(readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, typeSize, needReadSign) * powValue);
				if (!success)
				{
					return false;
				}
			}
		}
		return true;
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, bool value)
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
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, byte value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(byte), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, sbyte value, bool needWriteSign)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(sbyte), generateBitCount((ushort)value.abs()), value, needWriteSign);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, ushort value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(ushort), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, short value, bool needWriteSign)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(short), generateBitCount((ushort)value.abs()), value, needWriteSign);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, uint value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(uint), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, int value, bool needWriteSign)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(int), generateBitCount((uint)value.abs()), value, needWriteSign);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, ulong value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(ulong), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, long value, bool needWriteSign)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(long), generateBitCount((ulong)value.abs()), value, needWriteSign);
	}
	// 因为long是最大的带符号整型类型,所以可以使用long传递任何带符号整数类型的值
	public static bool writeSignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, byte bitCount, long value, bool needWriteSign)
	{
		if ((value != 0 && bitCount == 0) || bitCount > typeSize << 3)
		{
			return false;
		}
		if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, bitCount))
		{
			return false;
		}
		if (bitCount == 0)
		{
			return true;
		}
		return writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, value, bitCount, true, needWriteSign);
	}
	// 因为ulong是最大的无符号整型类型,所以可以使用ulong传递任何无符号整数类型的值
	public static bool writeUnsignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, byte bitCount, ulong value)
	{
		if ((value != 0 && bitCount == 0) || bitCount > typeSize << 3)
		{
			return false;
		}
		if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref bitCount))
		{
			return false;
		}
		if (bitCount == 0)
		{
			return true;
		}
		return writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, value, bitCount, typeSize, true);
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<byte> list)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(byte);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<byte> list)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(byte);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<sbyte> list, bool needWriteSign)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(sbyte);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ushort)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<sbyte> list, bool needWriteSign)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(sbyte);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ushort)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<short> list, bool needWriteSign)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(short);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ushort)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<short> list, bool needWriteSign)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(short);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ushort)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<ushort> list)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(ushort);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<ushort> list)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(ushort);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<int> list, bool needWriteSign)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(int);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((uint)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<int> list, bool needWriteSign)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(int);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((uint)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<uint> list)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(uint);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<uint> list)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(uint);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<long> list, bool needWriteSign)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(long);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ulong)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<long> list, bool needWriteSign)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(long);
		if (isUniformCountShorter(list, out byte maxBitCount, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ulong)list[i].abs()), list[i], needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<ulong> list)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(ulong);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<ulong> list)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(ulong);
		if (isUniformCountShorter(list, out byte maxBitCount))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, ref maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], maxBitCount, typeSize, false))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用统一的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			if (!writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount(list[i]), list[i]))
			{
				return false;
			}
		}
		return true;
	}
	// 浮点数会扩大一定倍数转换为整数来写入
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<float> list, bool needWriteSign, int precision = 3)
	{
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(float);
		int powValue = precision.pow10();
		// 使用统一的长度位占空间更小
		if (isUniformCountShorter(list, out byte maxBitCount, precision, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, (list[i] * powValue).round(), maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			int value = (list[i] * powValue).round();
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((uint)value.abs()), value, needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	// 浮点数会扩大一定倍数转换为整数来写入
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<float> list, bool needWriteSign, int precision = 3)
	{
		// 动态列表的数量使用ushort存储,超过上限时必须在写入任何数据前直接失败,不能静默截断。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		// 写入长度
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(float);
		int powValue = precision.pow10();
		// 使用统一的长度位占空间更小
		if (isUniformCountShorter(list, out byte maxBitCount, precision, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, (list[i] * powValue).round(), maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			int value = (list[i] * powValue).round();
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((uint)value.abs()), value, needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	// 浮点数会扩大一定倍数转换为整数来写入
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, Span<double> list, bool needWriteSign, int precision = 4)
	{
		// 写入长度
		ushort count = (ushort)list.Length;
		int typeSize = sizeof(double);
		long powValue = precision.pow10Long();
		// 使用统一的长度位占空间更小
		if (isUniformCountShorter(list, out byte maxBitCount, precision, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, (list[i] * powValue).round(), maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			long value = (list[i] * powValue).round();
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ulong)value.abs()), value, needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	// 浮点数会扩大一定倍数转换为整数来写入
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<double> list, bool needWriteSign, int precision = 4)
	{
		// 写入长度,为了尽量节省空间,这里只支持65535的最大列表长度。
		if (list.Count > ushort.MaxValue)
		{
			return false;
		}
		ushort count = (ushort)list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		int typeSize = sizeof(double);
		long powValue = precision.pow10Long();
		// 使用统一的长度位占空间更小
		if (isUniformCountShorter(list, out byte maxBitCount, precision, needWriteSign))
		{
			// 写入1表示使用统一的长度位
			setBufferBitOne(buffer, bitIndex++);
			// 写入长度位
			if (!writeSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, maxBitCount))
			{
				return false;
			}
			// 长度位为0,则不需要再继续写入
			if (maxBitCount == 0)
			{
				return true;
			}
			// 写入列表中所有的值
			for (int i = 0; i < count; ++i)
			{
				if (!writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, (list[i] * powValue).round(), maxBitCount, false, needWriteSign))
				{
					return false;
				}
			}
			return true;
		}
		// 写入0表示使用独立的长度位,只是下标跳一位,不会实际写入
		++bitIndex;
		for (int i = 0; i < count; ++i)
		{
			long value = (list[i] * powValue).round();
			if (!writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, typeSize, generateBitCount((ulong)value.abs()), value, needWriteSign))
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeBufferBit(byte[] buffer, int bufferSize, ref int bitIndex, byte[] srcBuffer, int writeCount)
	{
		if (writeCount == 0 || srcBuffer == null)
		{
			return true;
		}
		// 将前面空出来的位填充为0
		fillZeroToByteEnd(buffer, ref bitIndex);
		int curByte = bitIndex >> 3;
		bool result = writeBytes(buffer, ref curByte, srcBuffer, bufferSize, -1, writeCount);
		bitIndex = curByte << 3;
		return result;
	}
	public static void fillZeroToByteEnd(byte[] buffer, ref int bitIndex)
	{
		// 将前面空出来的位填充为0
		int curByte = bitCountToByteCount(bitIndex);
		int targetBitIndex = curByte << 3;
		for (int i = bitIndex; i < targetBitIndex; ++i)
		{
			setBitZero(ref buffer[i >> 3], i & 7);
		}
		bitIndex = targetBitIndex;
	}
	public static int bitCountToByteCount(int bitCount) { return (bitCount + 7) >> 3; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void writeUnsignedValueBit(byte[] buffer, int bitCount, ref int bitIndex, ulong value)
	{
		if (value != 0)
		{
			for (byte i = 0; i < bitCount; ++i)
			{
				if (hasBit(value, i))
				{
					setBufferBitOne(buffer, bitIndex + i);
				}
			}
		}
		bitIndex += bitCount;
	}
	// 直接写入一个有符号整数,不考虑任何情况
	protected static void writeSignedValueBit(byte[] buffer, int bitCount, ref int bitIndex, long value)
	{
		if (value != 0)
		{
			for (byte i = 0; i < bitCount; ++i)
			{
				if (hasBit(value, i))
				{
					setBufferBitOne(buffer, bitIndex + i);
				}
			}
		}
		bitIndex += bitCount;
	}
	// 直接读取一个无符号的整数,不考虑任何情况
	protected static ulong readUnsignedValueBit(byte[] buffer, ref int bitIndex, byte bitCount)
	{
		ulong tempValue = 0;
		for (byte i = 0; i < bitCount; ++i)
		{
			if (getBufferBit(buffer, bitIndex + i))
			{
				setBitOne(ref tempValue, i);
			}
		}
		bitIndex += bitCount;
		return tempValue;
	}
	// 直接读取一个有符号的整数,不考虑任何情况
	protected static long readSignedValueBit(byte[] buffer, ref int bitIndex, byte bitCount, out bool isNegative, bool needReadSign)
	{
		long tempValue = 0;
		// 读符号位
		isNegative = needReadSign && getBufferBit(buffer, bitIndex++);
		for (byte i = 0; i < bitCount; ++i)
		{
			if (getBufferBit(buffer, bitIndex + i))
			{
				setBitOne(ref tempValue, i);
			}
		}
		bitIndex += bitCount;
		if (isNegative)
		{
			tempValue = -tempValue;
		}
		return tempValue;
	}
	// 读取一个有符号的整数的长度位
	protected static bool readSignedLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, out byte bitCount)
	{
		bitCount = 0;
		// 读取长度位
		byte TYPE_LENGTH_MAX_BIT = LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		bitCount = (byte)readUnsignedValueBit(buffer, ref bitIndex, TYPE_LENGTH_MAX_BIT);
		return true;
	}
	// 读取一个无符号的整数的长度位
	protected static bool readUnsignedLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, out byte bitCount)
	{
		bitCount = 0;
		// 读取长度位
		byte TYPE_LENGTH_MAX_BIT = LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}

		bitCount = (byte)readUnsignedValueBit(buffer, ref bitIndex, TYPE_LENGTH_MAX_BIT);
		// 因为长度位的存储会忽略一个bit,所以当长度位的bit数达到此类型的最大时,会加1
		// 比如byte类型为8bit,长度位存储只用了3bit,只能表示0到7,所以当读取到的长度位值为7时,就需要读8bit的数据位,不然就会丢失数据
		if (bitCount == (1 << TYPE_LENGTH_MAX_BIT) - 1)
		{
			++bitCount;
		}
		return true;
	}
	// 读取有符号列在使用统一长度位的情况下的长度位
	protected static bool readSignedListLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, int count, out byte bitCount, bool needReadSign = true)
	{
		if (!readSignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, out bitCount))
		{
			return false;
		}
		// 每个元素会多一个符号位,所以+1,但是如果bitCount是0,则连符号位都不会有;needReadSign为false时不写入符号位,不额外计入
		int signBit = (needReadSign && bitCount > 0) ? 1 : 0;
		return bitCountToByteCount(bitIndex + (bitCount + signBit) * count) <= bufferSize;
	}
	// 读取无符号列在使用统一长度位的情况下的长度位
	protected static bool readUnsignedListLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, int count, out byte bitCount)
	{
		if (!readUnsignedLengthBit(buffer, bufferSize, ref bitIndex, typeSize, out bitCount))
		{
			return false;
		}
		return bitCountToByteCount(bitIndex + bitCount * count) <= bufferSize;
	}
	// 可以按位写入带符号的整数,并且不写入长度位,因为long是最大的带符号整数,所以可以表示所有的带符号整数
	protected static bool writeSignedIntegerBitNoLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, long value, byte bitCount, bool dropHighestOne, bool needWriteSign)
	{
		if (bitCount == 0)
		{
			return true;
		}
		int writeBitCount = bitCount + (needWriteSign ? 1 : 0) - (dropHighestOne ? 1 : 0);
		if (bitCountToByteCount(bitIndex + writeBitCount) > bufferSize)
		{
			return false;
		}
		if (needWriteSign)
		{
			if (value < 0)
			{
				value = -value;
				setBufferBitOne(buffer, bitIndex);
			}
			++bitIndex;
		}
		if (dropHighestOne)
		{
			// 将最高位的1去掉,不需要写入
			setBitZero(ref value, --bitCount);
			if (bitCount == 0)
			{
				return true;
			}
		}
		// 再写入值的所有位
		writeSignedValueBit(buffer, bitCount, ref bitIndex, value);
		return true;
	}
	// 可以按位写入无符号的整数,并且不写入长度位,因为ulong是最大的无符号整数,所以可以表示所有的无符号整数
	protected static bool writeUnsignedIntegerBitNoLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, ulong value, byte bitCount, int typeSize, bool dropHighestOne)
	{
		if (bitCount == 0)
		{
			return true;
		}
		// 32位及以下保持原协议规则,64位达到完整64bit时不能再省略最高位。
		// ulong的63bit长度会被writeUnsignedLengthBit扩展为64,因此这里必须按64bit边界判断。
		int removeHighBitLimit = typeSize == sizeof(ulong) ? typeSize << 3 : (1 << typeSize) - 1;
		bool reallyDropHighestOne = dropHighestOne && bitCount < removeHighBitLimit;
		int writeBitCount = bitCount - (reallyDropHighestOne ? 1 : 0);
		if (bitCountToByteCount(bitIndex + writeBitCount) > bufferSize)
		{
			return false;
		}
		if (reallyDropHighestOne)
		{
			// 将最高位的1去掉,不需要写入
			setBitZero(ref value, --bitCount);
			if (bitCount == 0)
			{
				return true;
			}
		}
		// 写入值的所有位
		writeUnsignedValueBit(buffer, bitCount, ref bitIndex, value);
		return true;
	}
	protected static bool writeSignedLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, byte bitCount)
	{
		byte typeLengthMaxBit = LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + typeLengthMaxBit) > bufferSize)
		{
			return false;
		}
		writeUnsignedValueBit(buffer, typeLengthMaxBit, ref bitIndex, bitCount);
		return true;
	}
	protected static bool writeUnsignedLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, ref byte bitCount)
	{
		byte typeLengthMaxBit = LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + typeLengthMaxBit) > bufferSize)
		{
			return false;
		}
		// 因为长度位的存储会忽略一个bit,所以当长度位的bit数达到此类型的最大时,会加1
		// 比如byte类型为8bit,长度位存储只用了3bit,只能表示0到7,所以当需要写入的长度位值为8时,就只写7进去,但是实际写入数据时仍然是写入8bit
		// 但是如果写入的长度位值为7,则长度位写入7,数据位仍然是写入8bit
		byte writeBitCount = bitCount;
		if (bitCount == 1 << typeLengthMaxBit)
		{
			writeBitCount = (byte)(bitCount - 1);
		}
		else if (bitCount == (1 << typeLengthMaxBit) - 1)
		{
			writeBitCount = bitCount;
			++bitCount;
		}
		writeUnsignedValueBit(buffer, typeLengthMaxBit, ref bitIndex, writeBitCount);
		return true;
	}
	protected static int getSignedUniformBitCount(int count, byte maxBitCount, int typeLengthMaxBit, bool needWriteSign)
	{
		if (maxBitCount == 0)
		{
			return typeLengthMaxBit;
		}
		return (maxBitCount + (needWriteSign ? 1 : 0)) * count + typeLengthMaxBit;
	}
	protected static int getUnsignedUniformBitCount(int count, byte maxBitCount, int typeLengthMaxBit)
	{
		if (maxBitCount == (1 << typeLengthMaxBit) - 1)
		{
			++maxBitCount;
		}
		return maxBitCount * count + typeLengthMaxBit;
	}
	protected static bool isUniformCountShorter(Span<byte> values, out byte maxBitCount)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(byte)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			byte value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<byte> values, out byte maxBitCount)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(byte)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			byte value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<sbyte> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(sbyte)];
		maxBitCount = generateBitCount((ushort)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			sbyte value = values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((ushort)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<sbyte> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(sbyte)];
		maxBitCount = generateBitCount((ushort)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			sbyte value = values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((ushort)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<ushort> values, out byte maxBitCount)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(ushort)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			ushort value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<ushort> values, out byte maxBitCount)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(ushort)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			ushort value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<short> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(short)];
		maxBitCount = generateBitCount((ushort)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			short value = values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((ushort)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<short> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(short)];
		maxBitCount = generateBitCount((ushort)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			short value = values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((ushort)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<int> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(int)];
		maxBitCount = generateBitCount((uint)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			uint value = (uint)values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount(value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<int> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(int)];
		maxBitCount = generateBitCount((uint)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			uint value = (uint)values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount(value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<uint> values, out byte maxBitCount)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(uint)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			uint value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<uint> values, out byte maxBitCount)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(uint)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			uint value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<long> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(long)];
		maxBitCount = generateBitCount((ulong)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			ulong value = (ulong)values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount(value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<long> values, out byte maxBitCount, bool needWriteSign)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(long)];
		maxBitCount = generateBitCount((ulong)findMaxAbs(values));
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			ulong value = (ulong)values[i].abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount(value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<ulong> values, out byte maxBitCount)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(ulong)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			ulong value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<ulong> values, out byte maxBitCount)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(ulong)];
		maxBitCount = generateBitCount(findMax(values));
		int uniformBitCount = getUnsignedUniformBitCount(count, maxBitCount, typeLengthMaxBit);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			ulong value = values[i];
			if (value == 0)
			{
				continue;
			}
			byte bitCount = generateBitCount(value);
			if (bitCount == (1 << typeLengthMaxBit) - 1)
			{
				++bitCount;
			}
			singleBitCount += bitCount - 1;
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<float> values, out byte maxBitCount, int precision, bool needWriteSign)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(float)];
		int powValue = precision.pow10();
		maxBitCount = generateBitCount((uint)(findMaxAbs(values) * powValue).round());
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			int value = (values[i] * powValue).round().abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((uint)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<float> values, out byte maxBitCount, int precision, bool needWriteSign)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(float)];
		int powValue = precision.pow10();
		maxBitCount = generateBitCount((uint)(findMaxAbs(values) * powValue).round());
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			int value = (values[i] * powValue).round().abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((uint)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(Span<double> values, out byte maxBitCount, int precision, bool needWriteSign)
	{
		int count = values.Length;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(double)];
		long powValue = precision.pow10();
		maxBitCount = generateBitCount((ulong)(findMaxAbs(values) * powValue).round());
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			long value = (values[i] * powValue).round().abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((ulong)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static bool isUniformCountShorter(List<double> values, out byte maxBitCount, int precision, bool needWriteSign)
	{
		int count = values.Count;
		int typeLengthMaxBit = LENGTH_MAX_BIT[sizeof(double)];
		long powValue = precision.pow10();
		maxBitCount = generateBitCount((ulong)(findMaxAbs(values) * powValue).round());
		int uniformBitCount = getSignedUniformBitCount(count, maxBitCount, typeLengthMaxBit, needWriteSign);
		int singleBitCount = typeLengthMaxBit * count;
		if (singleBitCount >= uniformBitCount)
		{
			return true;
		}
		for (int i = 0; i < count; ++i)
		{
			long value = (values[i] * powValue).round().abs();
			if (value == 0)
			{
				continue;
			}
			singleBitCount += generateBitCount((ulong)value) - 1 + (needWriteSign ? 1 : 0);
			if (singleBitCount > uniformBitCount)
			{
				return true;
			}
		}
		return singleBitCount >= uniformBitCount;
	}
	protected static byte generateBitCount(ushort value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		return mBitCountTable[value];
	}
	protected static byte generateBitCount(uint value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		ushort part1 = (ushort)((value & 0xFFFF0000) >> 16);
		if (part1 > 0)
		{
			return (byte)(mBitCountTable[part1] + 16);
		}
		return mBitCountTable[value & 0x0000FFFF];
	}
	protected static byte generateBitCount(ulong value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		if ((value & 0xFFFFFFFF00000000) > 0)
		{
			ushort part3 = (ushort)((value & 0xFFFF000000000000) >> 48);
			if (part3 > 0)
			{
				return (byte)(mBitCountTable[part3] + 16 * 3);
			}
			ushort part2 = (ushort)((value & 0x0000FFFF00000000) >> 32);
			return (byte)(mBitCountTable[part2] + 16 * 2);
		}
		ushort part1 = (ushort)((value & 0x00000000FFFF0000) >> 16);
		if (part1 > 0)
		{
			return (byte)(mBitCountTable[part1] + 16);
		}
		return mBitCountTable[value & 0x000000000000FFFF];
	}
	protected static void initBitCountTable()
	{
		using (new ThreadLockScope(mBitCountTableLock))
		{
			if (mBitCountTable != null)
			{
				return;
			}
			mBitCountTable = new byte[65536];
			for (int i = 0; i < 65536; ++i)
			{
				mBitCountTable[i] = internalGenerateBitCount((ushort)i);
			}
		}
	}
	protected static byte internalGenerateBitCount(ushort value)
	{
		if (value == 0)
		{
			return 0;
		}
		// 从高到低遍历每一位,找到最高位1的下标
		for (byte i = 0; i < 16; ++i)
		{
			if (hasBit(value, 15 - i))
			{
				return (byte)(16 - i);
			}
		}
		return 0;
	}
}