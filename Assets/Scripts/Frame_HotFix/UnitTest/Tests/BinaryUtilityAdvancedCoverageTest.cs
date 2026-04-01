#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Text;
using static FrameUtility;
using static BinaryUtility;
using static TestAssert;
using static SerializeByteUtility;

// BinaryUtility advanced coverage for memory helpers, bit helpers, and CRC helpers.
public static class BinaryUtilityAdvancedCoverageTest
{
	public static void Run()
	{
		testEncodingSingletons();
		testBitCountingAndContainment();
		testCrcConsistency();
		testMemcpyVariants();
		testMemmoveAndMemset();
		testMemoryEqualityAndZeroTrim();
		testBitAccessors();
		testBufferBitHelpers();
	}

	private static void testEncodingSingletons()
	{
		Encoding gb2312A = getGB2312();
		Encoding gb2312B = getGB2312();
		assertNotNull(gb2312A, "getGB2312 should return an encoding");
		assertEqual(gb2312A.WebName, gb2312B.WebName, "getGB2312 should cache the encoding");

		Encoding gbkA = getGBK();
		Encoding gbkB = getGBK();
		assertNotNull(gbkA, "getGBK should return an encoding");
		assertEqual(gbkA.WebName, gbkB.WebName, "getGBK should cache the encoding");
	}

	private static void testBitCountingAndContainment()
	{
		assertEqual(0, bitCount1(0x00), "bitCount1 zero");
		assertEqual(1, bitCount1(0x01), "bitCount1 one");
		assertEqual(4, bitCount1(0xAA), "bitCount1 alternating bits");
		assertEqual(8, bitCount1(0xFF), "bitCount1 all bits");

		byte[] buffer = { 1, 2, 3, 4, 5, 6 };
		assertTrue(contains(buffer, new byte[] { 1, 2 }), "contains should match prefix");
		assertTrue(contains(buffer, new byte[] { 3, 4 }), "contains should match middle");
		assertTrue(contains(buffer, new byte[] { 5, 6 }), "contains should match suffix");
		assertFalse(contains(buffer, new byte[] { 2, 4 }), "contains should reject non-contiguous bytes");
		assertFalse(contains(null, new byte[] { 1 }), "contains should reject null buffer");
		assertFalse(contains(buffer, null), "contains should reject null key");
	}

	private static void testCrcConsistency()
	{
		byte[] data = { 0x12, 0x34, 0x56, 0x78, 0x9A };
		ushort crcArray = crc16(0, data, data.Length);
		ushort crcByteByByte = 0;
		for (int i = 0; i < data.Length; ++i)
		{
			crcByteByByte = crc16(crcByteByByte, data[i]);
		}
		assertEqual(crcArray, crcByteByByte, "crc16 array and byte-by-byte should match");

		ushort crcPair = crc16(0, data[0], data[1]);
		ushort crcPairStep = crc16(crc16(0, data[0]), data[1]);
		assertEqual(crcPair, crcPairStep, "crc16 pair overload should match step-by-step");

		ushort crc4 = crc16(0, data[0], data[1], data[2], data[3]);
		ushort crc4Step = crc16(crc16(crc16(crc16(0, data[0]), data[1]), data[2]), data[3]);
		assertEqual(crc4, crc4Step, "crc16 4-byte overload should match step-by-step");

		ushort crc8 = crc16(0, data[0], data[1], data[2], data[3], data[4], 0xAA, 0xBB, 0xCC);
		ushort crc8Step = 0;
		crc8Step = crc16(crc8Step, data[0]);
		crc8Step = crc16(crc8Step, data[1]);
		crc8Step = crc16(crc8Step, data[2]);
		crc8Step = crc16(crc8Step, data[3]);
		crc8Step = crc16(crc8Step, data[4]);
		crc8Step = crc16(crc8Step, 0xAA);
		crc8Step = crc16(crc8Step, 0xBB);
		crc8Step = crc16(crc8Step, 0xCC);
		assertEqual(crc8, crc8Step, "crc16 8-byte overload should match step-by-step");

		assertEqual((ushort)(crc16(0x1F, data, data.Length) ^ 0x123F), generateCRC16(data, data.Length), "generateCRC16(byte[]) should wrap crc16 consistently");

		ushortToBytes((ushort)0x1234, out byte short0, out byte short1);
		assertEqual(generateCRC16(new byte[] { short0, short1 }, 2), generateCRC16((ushort)0x1234), "generateCRC16(ushort) should match byte-array form");

		intToBytes(0x12345678, out byte int0, out byte int1, out byte int2, out byte int3);
		assertEqual(generateCRC16(new byte[] { int0, int1, int2, int3 }, 4), generateCRC16(0x12345678), "generateCRC16(int) should match byte-array form");
	}

	private static void testMemcpyVariants()
	{
		int[] src = { 10, 20, 30, 40, 50 };
		int[] dest = new int[5];
		memcpyObject(dest, src, 0, 0, src.Length);
		for (int i = 0; i < src.Length; ++i)
		{
			assertEqual(src[i], dest[i], "memcpyObject should copy all elements");
		}

		int[] dest2 = new int[6];
		Span<int> srcSpan = src;
		memcpy(dest2, srcSpan, 1, 0, 3);
		assertEqual(10, dest2[1], "memcpy array<-span should copy first element");
		assertEqual(20, dest2[2], "memcpy array<-span should copy second element");
		assertEqual(30, dest2[3], "memcpy array<-span should copy third element");

		Span<int> destSpan = dest2;
		memcpy(destSpan, src, 2, 1, 2);
		assertEqual(20, dest2[2], "memcpy span<-array should copy at dest offset");
		assertEqual(30, dest2[3], "memcpy span<-array should copy at dest offset");

		int[] byteSrc = { 0x01020304, 0x05060708 };
		int[] byteDest = new int[2];
		memcpy(byteDest, byteSrc, 0, 0, sizeof(int) * 2);
		assertEqual(byteSrc[0], byteDest[0], "Buffer.BlockCopy overload should copy first int");
		assertEqual(byteSrc[1], byteDest[1], "Buffer.BlockCopy overload should copy second int");
	}

	private static void testMemmoveAndMemset()
	{
		int[] data = { 1, 2, 3, 4, 5 };
		memmove(data, 1, 0, 4);
		assertEqual(1, data[1], "memmove should shift forward without corruption");
		assertEqual(2, data[2], "memmove should shift forward without corruption");
		assertEqual(3, data[3], "memmove should shift forward without corruption");
		assertEqual(4, data[4], "memmove should shift forward without corruption");

		int[] data2 = { 1, 2, 3, 4, 5 };
		memmove(data2, 0, 1, 4);
		assertEqual(2, data2[0], "memmove should shift backward without corruption");
		assertEqual(3, data2[1], "memmove should shift backward without corruption");
		assertEqual(4, data2[2], "memmove should shift backward without corruption");
		assertEqual(5, data2[3], "memmove should shift backward without corruption");

		byte[] bytes = new byte[8];
		memset(bytes, (byte)0xAB, 2, 3);
		assertEqual((byte)0x00, bytes[0], "memset should leave prefix untouched");
		assertEqual((byte)0xAB, bytes[2], "memset should write start index");
		assertEqual((byte)0xAB, bytes[4], "memset should write full length");
		assertEqual((byte)0x00, bytes[5], "memset should leave suffix untouched");

		string[] texts = new string[4];
		memset(texts, "x", 1, 2);
		assertNull(texts[0], "memset should leave string prefix untouched");
		assertEqual("x", texts[1], "memset should set first string element");
		assertEqual("x", texts[2], "memset should set second string element");
	}

	private static void testMemoryEqualityAndZeroTrim()
	{
		byte[] a = { 1, 2, 3, 4, 5 };
		byte[] b = { 1, 2, 3, 4, 5 };
		byte[] c = { 1, 2, 9, 4, 5 };

		assertTrue(isMemoryEqual(a, b, 5), "isMemoryEqual should accept identical buffers");
		assertTrue(isMemoryEqual(a, b, 3, 1, 1), "isMemoryEqual should accept equal ranges");
		assertFalse(isMemoryEqual(a, c, 5), "isMemoryEqual should reject different buffers");
		assertFalse(isMemoryEqual(a, b, 10), "isMemoryEqual should reject overrun");

		assertEqual("hello", removeLastZero("hello\0\0"), "removeLastZero should trim trailing zero bytes");
		assertEqual("world", removeLastZero("world"), "removeLastZero should leave strings without zeros unchanged");
	}

	private static void testBitAccessors()
	{
		assertTrue(hasBit((byte)0x02, 1), "hasBit(byte) should see set bit");
		assertFalse(hasBit((byte)0x02, 0), "hasBit(byte) should see clear bit");
		assertTrue(hasBit((short)0x0002, 1), "hasBit(short) should see set bit");
		assertTrue(hasBit(0x80000000, 31), "hasBit(int) should see sign bit");
		assertTrue(hasBit(0x80000000u, 31), "hasBit(uint) should see high bit");
		assertTrue(hasBit((long)1 << 40, 40), "hasBit(long) should see high bit");
		assertTrue(hasBit((ulong)1 << 40, 40), "hasBit(ulong) should see high bit");

		byte b = 0;
		setBitOne(ref b, 3);
		assertTrue(hasBit(b, 3), "setBitOne(byte) should set bit");
		setBitZero(ref b, 3);
		assertFalse(hasBit(b, 3), "setBitZero(byte) should clear bit");

		short s = 0;
		setBitOne(ref s, 10);
		assertTrue(hasBit(s, 10), "setBitOne(short) should set bit");
		setBitZero(ref s, 10);
		assertFalse(hasBit(s, 10), "setBitZero(short) should clear bit");

		int i = 0;
		setBitOne(ref i, 20);
		assertTrue(hasBit(i, 20), "setBitOne(int) should set bit");
		setBitZero(ref i, 20);
		assertFalse(hasBit(i, 20), "setBitZero(int) should clear bit");

		long l = 0;
		setBitOne(ref l, 40);
		assertTrue(hasBit(l, 40), "setBitOne(long) should set bit");
		setBitZero(ref l, 40);
		assertFalse(hasBit(l, 40), "setBitZero(long) should clear bit");

		byte low = 0xFF;
		setLowestBit(ref low, 0);
		assertEqual((byte)0xFE, low, "setLowestBit(byte,0) should clear low bit");
		setLowestBit(ref low, 1);
		assertEqual((byte)0xFF, low, "setLowestBit(byte,1) should set low bit");

		short high = 0x7FFF;
		setHighestBit(ref high, 1);
		assertTrue(hasBit(high, 15), "setHighestBit(short,1) should set high bit");
		setHighestBit(ref high, 0);
		assertFalse(hasBit(high, 15), "setHighestBit(short,0) should clear high bit");
	}

	private static void testBufferBitHelpers()
	{
		byte[] buffer = new byte[2];
		assertFalse(getBufferBit(buffer, 0), "getBufferBit should read clear bit");
		setBufferBitOne(buffer, 0);
		assertTrue(getBufferBit(buffer, 0), "setBufferBitOne should set bit 0");
		setBufferBitOne(buffer, 9);
		assertTrue(getBufferBit(buffer, 9), "setBufferBitOne should set bit across byte boundary");
	}
}
#endif
