using System;

// C++ / C# 按位协议兼容测试。
// 这里不能使用C#生产Writer来构造所有输入，否则Writer和Reader同时存在同一种Bug时会假通过。
// Reference Writer严格按当前C++协议规则手工构造wire-format。
public class SerializeBitCompatibilityTest
{
	public static void Run()
	{
		testCppUnsignedByte();
		testCppUnsignedUShort();
		testCppUnsignedUInt();
		testCppUnsignedULong();
		testCppSignedSByte();
		testCppSignedShort();
		testCppSignedInt();
		testCppSignedLong();
		testCppProtocolGoldenBuffer();
		testCppIndependentListGoldenBuffer();
	}

	private static void testCppUnsignedByte()
	{
		ulong[] values = { 0UL, 1UL, 2UL, 63UL, 64UL, 127UL, 128UL, 255UL };
		foreach (ulong value in values)
		{
			testCppUnsignedValue(value, sizeof(byte));
		}
	}

	private static void testCppUnsignedUShort()
	{
		ulong[] values = { 0UL, 1UL, 16383UL, 16384UL, 32767UL, 32768UL, ushort.MaxValue };
		foreach (ulong value in values)
		{
			testCppUnsignedValue(value, sizeof(ushort));
		}
	}

	private static void testCppUnsignedUInt()
	{
		ulong[] values = { 0UL, 1UL, 0x3FFFFFFFUL, 0x40000000UL, 0x7FFFFFFFUL, 0x80000000UL, uint.MaxValue };
		foreach (ulong value in values)
		{
			testCppUnsignedValue(value, sizeof(uint));
		}
	}

	private static void testCppUnsignedULong()
	{
		ulong[] values =
		{
			0UL,
			1UL,
			(1UL << 62) - 1UL,
			1UL << 62,
			(1UL << 62) + 1UL,
			(1UL << 63) - 1UL,
			1UL << 63,
			ulong.MaxValue - 1UL,
			ulong.MaxValue,
		};
		foreach (ulong value in values)
		{
			testCppUnsignedValue(value, sizeof(ulong));
		}
	}

	private static void testCppSignedSByte()
	{
		long[] values = { 0L, 1L, -1L, 2L, -2L, sbyte.MaxValue, -sbyte.MaxValue };
		foreach (long value in values)
		{
			testCppSignedValue(value, sizeof(sbyte));
		}
	}

	private static void testCppSignedShort()
	{
		long[] values = { 0L, 1L, -1L, 16384L, -16384L, short.MaxValue, -short.MaxValue };
		foreach (long value in values)
		{
			testCppSignedValue(value, sizeof(short));
		}
	}

	private static void testCppSignedInt()
	{
		long[] values = { 0L, 1L, -1L, 0x3FFFFFFFL, -0x3FFFFFFFL, int.MaxValue, -int.MaxValue };
		foreach (long value in values)
		{
			testCppSignedValue(value, sizeof(int));
		}
	}

	private static void testCppSignedLong()
	{
		long[] values = { 0L, 1L, -1L, (1L << 62) - 1L, -((1L << 62) - 1L), long.MaxValue, -long.MaxValue };
		foreach (long value in values)
		{
			testCppSignedValue(value, sizeof(long));
		}
	}

	private static void testCppUnsignedValue(ulong value, int typeSize)
	{
		for (int offset = 0; offset < 8; ++offset)
		{
			byte[] buffer = new byte[32];
			int writeBitIndex = 0;
			writePrefixRaw(buffer, ref writeBitIndex, offset);
			writeCppUnsigned(buffer, ref writeBitIndex, value, typeSize);
			writeRawBits(buffer, ref writeBitIndex, 0UL, 1);
			writeRawBits(buffer, ref writeBitIndex, 1UL, 1);

			int expectedValueEndBit = writeBitIndex - 2;
			int readBitIndex = 0;
			readPrefixProduction(buffer, ref readBitIndex, offset);

			bool success;
			ulong result;
			if (typeSize == sizeof(byte))
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out byte temp);
				result = temp;
			}
			else if (typeSize == sizeof(ushort))
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out ushort temp);
				result = temp;
			}
			else if (typeSize == sizeof(uint))
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out uint temp);
				result = temp;
			}
			else
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out ulong temp);
				result = temp;
			}

			check(success, "C++ unsigned read失败,typeSize:" + typeSize + ",value:" + value + ",offset:" + offset);
			check(result == value, "C++ unsigned value错误,typeSize:" + typeSize + ",Expected:" + value + ",Actual:" + result + ",offset:" + offset);
			check(readBitIndex == expectedValueEndBit, "C++ unsigned value后bitIndex错误,typeSize:" + typeSize + ",value:" + value + ",Expected:" + expectedValueEndBit + ",Actual:" + readBitIndex + ",offset:" + offset);
			readSentinelProduction(buffer, bytesForBits(writeBitIndex), ref readBitIndex, "C++ unsigned,typeSize:" + typeSize + ",value:" + value + ",offset:" + offset);
			check(readBitIndex == writeBitIndex, "C++ unsigned最终bitIndex错误,typeSize:" + typeSize + ",value:" + value + ",Expected:" + writeBitIndex + ",Actual:" + readBitIndex + ",offset:" + offset);
		}
	}

	private static void testCppSignedValue(long value, int typeSize)
	{
		for (int offset = 0; offset < 8; ++offset)
		{
			byte[] buffer = new byte[32];
			int writeBitIndex = 0;
			writePrefixRaw(buffer, ref writeBitIndex, offset);
			writeCppSigned(buffer, ref writeBitIndex, value, typeSize, true);
			writeRawBits(buffer, ref writeBitIndex, 0UL, 1);
			writeRawBits(buffer, ref writeBitIndex, 1UL, 1);
			int expectedValueEndBit = writeBitIndex - 2;

			int readBitIndex = 0;
			readPrefixProduction(buffer, ref readBitIndex, offset);
			bool success;
			long result;
			if (typeSize == sizeof(sbyte))
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out sbyte temp, true);
				result = temp;
			}
			else if (typeSize == sizeof(short))
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out short temp, true);
				result = temp;
			}
			else if (typeSize == sizeof(int))
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out int temp, true);
				result = temp;
			}
			else
			{
				success = SerializeBitUtility.readBit(buffer, bytesForBits(writeBitIndex), ref readBitIndex, out long temp, true);
				result = temp;
			}
			check(success, "C++ signed read失败,typeSize:" + typeSize + ",value:" + value + ",offset:" + offset);
			check(result == value, "C++ signed value错误,typeSize:" + typeSize + ",Expected:" + value + ",Actual:" + result + ",offset:" + offset);
			check(readBitIndex == expectedValueEndBit, "C++ signed value后bitIndex错误,typeSize:" + typeSize + ",value:" + value + ",Expected:" + expectedValueEndBit + ",Actual:" + readBitIndex + ",offset:" + offset);
			readSentinelProduction(buffer, bytesForBits(writeBitIndex), ref readBitIndex, "C++ signed,typeSize:" + typeSize + ",value:" + value + ",offset:" + offset);
			check(readBitIndex == writeBitIndex, "C++ signed最终bitIndex错误,typeSize:" + typeSize + ",value:" + value + ",Expected:" + writeBitIndex + ",Actual:" + readBitIndex + ",offset:" + offset);
		}
	}

	private static void testCppProtocolGoldenBuffer()
	{
		byte[] expected =
		{
			0x3D, 0x15, 0xCD, 0x5B, 0xC7, 0x58, 0x34, 0x6F,
			0xBD, 0x03, 0xFA, 0x84, 0xB8, 0x93, 0xB3, 0x00,
			0x90, 0x26, 0x5C, 0x2C, 0x71, 0x11, 0x07,
		};

		SerializerBitWrite writer = new SerializerBitWrite();
		Span<long> signed = stackalloc long[2] { 123456789L, -987654321L };
		writer.write(signed, true);
		Span<uint> unsigned = stackalloc uint[6] { 8000u, 7234u, 10023u, 5u, 1234u, 5678u };
		writer.write(unsigned);
		writer.write(3u);
		writer.write(1.25f, true);
		check(writer.getBitCount() == 181, "C# golden bitCount改变,Expected:181,Actual:" + writer.getBitCount());
		check(writer.getByteCount() == expected.Length, "C# golden byteCount改变,Expected:" + expected.Length + ",Actual:" + writer.getByteCount());
		for (int i = 0; i < expected.Length; ++i)
		{
			check(writer.getBuffer()[i] == expected[i], "C# golden buffer不一致,index:" + i + ",Expected:" + expected[i] + ",Actual:" + writer.getBuffer()[i]);
		}

		SerializerBitRead reader = new SerializerBitRead();
		reader.init(expected, expected.Length);
		Span<long> signedResult = stackalloc long[2];
		check(reader.read(ref signedResult, true), "C++ golden signed read");
		check(signedResult[0] == 123456789L && signedResult[1] == -987654321L, "C++ golden signed value");
		Span<uint> unsignedResult = stackalloc uint[6];
		check(reader.read(ref unsignedResult), "C++ golden unsigned read");
		check(unsignedResult[0] == 8000u && unsignedResult[1] == 7234u && unsignedResult[2] == 10023u && unsignedResult[3] == 5u && unsignedResult[4] == 1234u && unsignedResult[5] == 5678u, "C++ golden unsigned value");
		check(reader.read(out uint direction) && direction == 3u, "C++ golden direction");
		check(reader.read(out float attackSpeed, true) && Math.Abs(attackSpeed - 1.25f) <= 0.0011f, "C++ golden float");
		check(reader.getBitIndex() == 181, "C++ golden read bitIndex,Expected:181,Actual:" + reader.getBitIndex());
	}

	private static void testCppIndependentListGoldenBuffer()
	{
		byte[] expected = { 0x40, 0x10, 0xC4, 0x0F, 0x00, 0x00, 0x00, 0x04, 0x00 };
		Span<uint> source = stackalloc uint[6] { 0u, 1u, 2u, 3u, 0x40000000u, 0u };
		SerializerBitWrite writer = new SerializerBitWrite();
		writer.write(source);
		check(writer.getBitCount() == 65, "C# independent golden bitCount改变,Expected:65,Actual:" + writer.getBitCount());
		check(writer.getByteCount() == expected.Length, "C# independent golden byteCount改变");
		for (int i = 0; i < expected.Length; ++i)
		{
			check(writer.getBuffer()[i] == expected[i], "C# independent golden buffer不一致,index:" + i + ",Expected:" + expected[i] + ",Actual:" + writer.getBuffer()[i]);
		}

		SerializerBitRead reader = new SerializerBitRead();
		reader.init(expected, expected.Length);
		Span<uint> result = stackalloc uint[6];
		check(reader.read(ref result), "C++ independent golden read");
		for (int i = 0; i < source.Length; ++i)
		{
			check(result[i] == source[i], "C++ independent golden value,index:" + i + ",Expected:" + source[i] + ",Actual:" + result[i]);
		}
		check(reader.getBitIndex() == 65, "C++ independent golden bitIndex,Expected:65,Actual:" + reader.getBitIndex());
	}

	private static void writeCppUnsigned(byte[] buffer, ref int bitIndex, ulong value, int typeSize)
	{
		int lengthBitCount = getLengthBitCount(typeSize);
		int fullBitCount = typeSize << 3;
		int valueBitCount = getBitCount(value);
		int maxLengthCode = (1 << lengthBitCount) - 1;
		int writeLength = valueBitCount;
		int dataLength = valueBitCount;
		if (valueBitCount == 1 << lengthBitCount)
		{
			writeLength = valueBitCount - 1;
		}
		else if (valueBitCount == maxLengthCode)
		{
			++dataLength;
		}
		writeRawBits(buffer, ref bitIndex, (ulong)writeLength, lengthBitCount);
		if (value == 0)
		{
			return;
		}
		// 32位及以下保持现有协议规则,只有64位无符号整数使用完整64bit作为边界。
		int removeHighBitLimit = typeSize == sizeof(ulong) ? fullBitCount : (1 << typeSize) - 1;
		bool removeHighBit = dataLength < removeHighBitLimit;
		int dataBitCount = removeHighBit ? dataLength - 1 : dataLength;
		writeRawBits(buffer, ref bitIndex, value, dataBitCount);
	}

	private static void writeCppSigned(byte[] buffer, ref int bitIndex, long value, int typeSize, bool needWriteSign)
	{
		if (value == long.MinValue)
		{
			throw new Exception("Reference协议不支持long.MinValue");
		}
		ulong absValue = value < 0 ? (ulong)(-value) : (ulong)value;
		int valueBitCount = getBitCount(absValue);
		int lengthBitCount = getLengthBitCount(typeSize);
		writeRawBits(buffer, ref bitIndex, (ulong)valueBitCount, lengthBitCount);
		if (valueBitCount == 0)
		{
			return;
		}
		if (needWriteSign)
		{
			writeRawBits(buffer, ref bitIndex, value < 0 ? 1UL : 0UL, 1);
		}
		writeRawBits(buffer, ref bitIndex, absValue, valueBitCount - 1);
	}

	private static int getLengthBitCount(int typeSize)
	{
		switch (typeSize)
		{
			case 1: return 3;
			case 2: return 4;
			case 4: return 5;
			case 8: return 6;
			default: throw new Exception("不支持的整数typeSize:" + typeSize);
		}
	}

	private static int getBitCount(ulong value)
	{
		if (value == 0)
		{
			return 0;
		}
		int count = 0;
		while (value != 0)
		{
			++count;
			value >>= 1;
		}
		return count;
	}

	private static void writePrefixRaw(byte[] buffer, ref int bitIndex, int offset)
	{
		for (int i = 0; i < offset; ++i)
		{
			writeRawBits(buffer, ref bitIndex, (i & 1) != 0 ? 1UL : 0UL, 1);
		}
	}

	private static void readPrefixProduction(byte[] buffer, ref int bitIndex, int offset)
	{
		for (int i = 0; i < offset; ++i)
		{
			check(SerializeBitUtility.readBit(buffer, buffer.Length, ref bitIndex, out bool value), "compat prefix read offset:" + offset + ",index:" + i);
			check(value == ((i & 1) != 0), "compat prefix value offset:" + offset + ",index:" + i);
		}
	}

	private static void readSentinelProduction(byte[] buffer, int bufferSize, ref int bitIndex, string info)
	{
		check(SerializeBitUtility.readBit(buffer, bufferSize, ref bitIndex, out bool sentinel0) && !sentinel0, info + " sentinel0");
		check(SerializeBitUtility.readBit(buffer, bufferSize, ref bitIndex, out bool sentinel1) && sentinel1, info + " sentinel1");
	}

	private static void writeRawBits(byte[] buffer, ref int bitIndex, ulong value, int bitCount)
	{
		for (int i = 0; i < bitCount; ++i)
		{
			if (((value >> i) & 1UL) != 0)
			{
				int index = bitIndex + i;
				buffer[index >> 3] |= (byte)(1 << (index & 7));
			}
		}
		bitIndex += bitCount;
	}

	private static int bytesForBits(int bitCount)
	{
		return (bitCount + 7) >> 3;
	}

	private static void check(bool condition, string info)
	{
		if (!condition)
		{
			throw new Exception(info);
		}
	}
}
