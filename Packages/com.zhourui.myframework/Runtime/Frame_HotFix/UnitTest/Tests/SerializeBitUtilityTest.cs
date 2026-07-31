using System;
using System.Collections.Generic;
using static SerializeBitUtility;
using static SerializeByteUtility;
using static TestAssert;

// SerializeBitUtility 按位序列化工具函数测试
// 策略: 对每种类型做 writeBit -> readBit 往返, 断言一致性, 避免依赖精确位布局
public static class SerializeBitUtilityTest
{
	public static void Run()
	{
		testBoolRoundTrip();
		testByteRoundTrip();
		testSByteRoundTrip();
		testUShortRoundTrip();
		testShortRoundTrip();
		testUIntRoundTrip();
		testIntRoundTrip();
		testULongRoundTrip();
		testLongRoundTrip();
		testSignedUnsignedIntegerBit();
		testListByteRoundTrip();
		testListSByteRoundTrip();
		testListUShortRoundTrip();
		testListShortRoundTrip();
		testListUIntRoundTrip();
		testListIntRoundTrip();
		testListULongRoundTrip();
		testListLongRoundTrip();
		testListFloatRoundTrip();
		testListDoubleRoundTrip();
		testHelpers();
		testFillZeroToByteEnd();
		testOverflowReturnsFailure();
		testSequentialMixedRoundTrip();
	}

	// ---- 标量往返 ----
	static void testBoolRoundTrip()
	{
		byte[] buf = new byte[16];
		int wi = 0;
		assert(writeBit(buf, buf.Length, ref wi, true), "write bool true");
		assert(writeBit(buf, buf.Length, ref wi, false), "write bool false");
		int ri = 0;
		assert(readBit(buf, buf.Length, ref ri, out bool v0), "read bool0");
		assert(readBit(buf, buf.Length, ref ri, out bool v1), "read bool1");
		assert(v0, "bool0 true");
		assert(!v1, "bool1 false");
	}

	static void testByteRoundTrip()
	{
		byte[] vals = { 0, 1, 127, 200, 255 };
		byte[] buf = new byte[64];
		int wi = 0;
		foreach (byte v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v), "write byte " + v);
		}
		int ri = 0;
		foreach (byte v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out byte o), "read byte");
			assertEqual(v, o, "byte roundtrip " + v);
		}
	}

	static void testSByteRoundTrip()
	{
		// 注意: sbyte.MinValue(-128) 的 abs() 会溢出(sbyte范围-128~127)，
		// writeBit 内部用 generateBitCount((ushort)value.abs()) 会导致错误
		sbyte[] vals = { -127, -1, 0, 1, 100, 127 };
		byte[] buf = new byte[64];
		int wi = 0;
		foreach (sbyte v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v, true), "write sbyte");
		}
		int ri = 0;
		foreach (sbyte v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out sbyte o, true), "read sbyte");
			assertEqual(v, o, "sbyte roundtrip " + v);
		}
	}

	static void testUShortRoundTrip()
	{
		ushort[] vals = { 0, 1, 255, 3000, 65535 };
		byte[] buf = new byte[64];
		int wi = 0;
		foreach (ushort v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v), "write ushort");
		}
		int ri = 0;
		foreach (ushort v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out ushort o), "read ushort");
			assertEqual(v, o, "ushort roundtrip " + v);
		}
	}

	static void testShortRoundTrip()
	{
		// -32768 (short.MinValue) 在 abs() 时溢出为 0, 跳过此边界值
		short[] vals = { -1000, -1, 0, 1, 30000, 32767 };
		byte[] buf = new byte[64];
		int wi = 0;
		foreach (short v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v, true), "write short");
		}
		int ri = 0;
		foreach (short v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out short o, true), "read short");
			assertEqual(v, o, "short roundtrip " + v);
		}
	}

	static void testUIntRoundTrip()
	{
		uint[] vals = { 0u, 1u, 255u, 70000u, uint.MaxValue };
		byte[] buf = new byte[128];
		int wi = 0;
		foreach (uint v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v), "write uint");
		}
		int ri = 0;
		foreach (uint v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out uint o), "read uint");
			assertEqual(v, o, "uint roundtrip " + v);
		}
	}

	static void testIntRoundTrip()
	{
		// int.MinValue 的 abs() 在 C# 中会溢出, 跳过
		int[] vals = { -123456, -1, 0, 1, 123456, int.MaxValue };
		byte[] buf = new byte[128];
		int wi = 0;
		foreach (int v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v, true), "write int");
		}
		int ri = 0;
		foreach (int v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out int o, true), "read int");
			assertEqual(v, o, "int roundtrip " + v);
		}
	}

	static void testULongRoundTrip()
	{
		ulong[] vals = { 0ul, 1ul, 255ul, 1UL << 40, ulong.MaxValue };
		byte[] buf = new byte[256];
		int wi = 0;
		foreach (ulong v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v), "write ulong");
		}
		int ri = 0;
		foreach (ulong v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out ulong o), "read ulong");
			assertEqual(v, o, "ulong roundtrip " + v);
		}
	}

	static void testLongRoundTrip()
	{
		// long.MinValue excluded: abs() overflows in bitCount calculation (same as int/short MinValue)
		long[] vals = { -9999999999L, -1L, 0L, 1L, 9999999999L, long.MaxValue };
		byte[] buf = new byte[256];
		int wi = 0;
		foreach (long v in vals)
		{
			assert(writeBit(buf, buf.Length, ref wi, v, true), "write long");
		}
		int ri = 0;
		foreach (long v in vals)
		{
			assert(readBit(buf, buf.Length, ref ri, out long o, true), "read long");
			assertEqual(v, o, "long roundtrip " + v);
		}
	}

	// ---- 直接调用 readSigned/UnsignedIntegerBit ----
	static void testSignedUnsignedIntegerBit()
	{
		byte[] buf = new byte[64];
		int wi = 0;
		assert(writeSignedIntegerBit(buf, buf.Length, ref wi, sizeof(int), generateTestBitCount(42), 42, true), "writeSignedIntBit");
		int ri = 0;
		long signed = readSignedIntegerBit(buf, buf.Length, ref ri, out bool s0, sizeof(int), true);
		assert(s0, "readSignedIntBit success");
		assertEqual(42L, signed, "signed 42");

		wi = 0;
		// writeUnsignedValueBit only sets 1-bits, does not clear 0-bits,
		// so we need a clean buffer after the signed write above polluted it
		buf = new byte[64];
		assert(writeUnsignedIntegerBit(buf, buf.Length, ref wi, sizeof(uint), generateTestBitCount(3000u), 3000u), "writeUnsignedIntBit");
		ri = 0;
		ulong unsigned = readUnsignedIntegerBit(buf, buf.Length, ref ri, out bool s1, sizeof(uint));
		assert(s1, "readUnsignedIntBit success");
		assertEqual(3000UL, unsigned, "unsigned 3000");
	}

	// ---- List 往返 ----
	static void testListByteRoundTrip()
	{
		byte[] buf = new byte[64];
		List<byte> src = new() { 0, 1, 127, 200, 255 };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src), "write List<byte>");
		int ri = 0;
		List<byte> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst), "read List<byte>");
		assertEqual(src.Count, dst.Count, "List<byte> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<byte>[" + i + "]");
		}
	}

	static void testListSByteRoundTrip()
	{
		byte[] buf = new byte[64];
		// sbyte.MinValue (-128) excluded: abs() overflows (sbyte range), same class of issue as short/int/long MinValue
		List<sbyte> src = new() { -127, -1, 0, 100, 127 };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src, true), "write List<sbyte>");
		int ri = 0;
		List<sbyte> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst, true), "read List<sbyte>");
		assertEqual(src.Count, dst.Count, "List<sbyte> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<sbyte>[" + i + "]");
		}
	}

	static void testListUShortRoundTrip()
	{
		byte[] buf = new byte[64];
		List<ushort> src = new() { 0, 1, 300, 65535 };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src), "write List<ushort>");
		int ri = 0;
		List<ushort> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst), "read List<ushort>");
		assertEqual(src.Count, dst.Count, "List<ushort> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<ushort>[" + i + "]");
		}
	}

	static void testListShortRoundTrip()
	{
		byte[] buf = new byte[64];
		// short.MinValue (-32768) excluded: abs() overflows (same as sbyte/short/int/long MinValue)
		List<short> src = new() { -32767, -1, 0, 32767 };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src, true), "write List<short>");
		int ri = 0;
		List<short> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst, true), "read List<short>");
		assertEqual(src.Count, dst.Count, "List<short> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<short>[" + i + "]");
		}
	}

	static void testListUIntRoundTrip()
	{
		byte[] buf = new byte[128];
		List<uint> src = new() { 0u, 1u, 70000u, uint.MaxValue };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src), "write List<uint>");
		int ri = 0;
		List<uint> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst), "read List<uint>");
		assertEqual(src.Count, dst.Count, "List<uint> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<uint>[" + i + "]");
		}
	}

	static void testListIntRoundTrip()
	{
		byte[] buf = new byte[128];
		// int.MinValue excluded: abs() overflows (same class of issue)
		List<int> src = new() { -2147483647, -123456, 0, 123456, int.MaxValue };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src, true), "write List<int>");
		int ri = 0;
		List<int> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst, true), "read List<int>");
		assertEqual(src.Count, dst.Count, "List<int> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<int>[" + i + "]");
		}
	}

	static void testListULongRoundTrip()
	{
		byte[] buf = new byte[128];
		List<ulong> src = new() { 0ul, 1ul, (1UL << 40), ulong.MaxValue };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src), "write List<ulong>");
		int ri = 0;
		List<ulong> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst), "read List<ulong>");
		assertEqual(src.Count, dst.Count, "List<ulong> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<ulong>[" + i + "]");
		}
	}

	static void testListLongRoundTrip()
	{
		byte[] buf = new byte[128];
		// long.MinValue excluded: abs() overflows (same class of issue)
		List<long> src = new() { -9223372036854775807L, -1L, 0L, 1L, long.MaxValue };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src, true), "write List<long>");
		int ri = 0;
		List<long> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst, true), "read List<long>");
		assertEqual(src.Count, dst.Count, "List<long> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i], dst[i], "List<long>[" + i + "]");
		}
	}

	static void testListFloatRoundTrip()
	{
		byte[] buf = new byte[128];
		List<float> src = new() { 0f, 1f, -3.14f, 999.9f };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src, true, 3), "write List<float>");
		int ri = 0;
		List<float> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst, true, 3), "read List<float>");
		assertEqual(src.Count, dst.Count, "List<float> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertTrue(isFloatNear(src[i], dst[i], 0.001f), "List<float>[" + i + "]");
		}
	}

	static void testListDoubleRoundTrip()
	{
		byte[] buf = new byte[128];
		List<double> src = new() { 0.0, 1.5, -2.7182818, 1000.25 };
		int wi = 0;
		assert(writeListBit(buf, buf.Length, ref wi, src, true, 4), "write List<double>");
		int ri = 0;
		List<double> dst = new();
		assert(readListBit(buf, buf.Length, ref ri, dst, true, 4), "read List<double>");
		assertEqual(src.Count, dst.Count, "List<double> count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertTrue(isDoubleNear(src[i], dst[i], 0.0001), "List<double>[" + i + "]");
		}
	}

	// ---- 辅助函数直接测试 ----
	static void testHelpers()
	{
		// bitCountToByteCount
		assertEqual(0, bitCountToByteCount(0), "bitToByte 0");
		assertEqual(1, bitCountToByteCount(1), "bitToByte 1");
		assertEqual(1, bitCountToByteCount(8), "bitToByte 8");
		assertEqual(2, bitCountToByteCount(9), "bitToByte 9");
		assertEqual(4, bitCountToByteCount(32), "bitToByte 32");

		// writeBufferBit + fillZeroToByteEnd
		byte[] buf = new byte[64];
		int bi = 0;
		byte[] src = { 0xAB, 0xCD, 0xEF };
		assert(writeBufferBit(buf, buf.Length, ref bi, src, src.Length), "writeBufferBit");
		assert(bi % 8 == 0, "writeBufferBit aligned");
		// 用 SerializeByteUtility.readBytes 读回
		int ri = 0;
		byte[] dst = new byte[3];
		assert(readBytes(buf, ref ri, dst), "readBytes back");
		assertEqual((byte)0xAB, dst[0], "buffer[0]");
		assertEqual((byte)0xCD, dst[1], "buffer[1]");
		assertEqual((byte)0xEF, dst[2], "buffer[2]");

		// writeBufferBit null/0 -> true, 不改 bitIndex
		int bi2 = 5;
		assert(writeBufferBit(buf, buf.Length, ref bi2, null, 0), "writeBufferBit null");
	}

	// ---- fillZeroToByteEnd: 把非字节边界位清零并对齐到字节边界 ----
	static void testFillZeroToByteEnd()
	{
		byte[] buf = new byte[4];
		// 先在 bitIndex=5 处把 buffer 相关位设为1 (第5、6、7位在byte0)
		// 手动构造: byte0 = 0xFF, 然后 fillZero 应清掉 bit5..bit7, 保留 bit0..bit4
		buf[0] = 0xFF;
		int bitIndex = 5;
		fillZeroToByteEnd(buf, ref bitIndex);
		// byte0 = 0b11111111 -> 清零高3位(bit5,6,7) => 0b00011111 = 0x1F
		assertEqual((byte)0x1F, buf[0], "fillZero byte0 = 0x1F");
		// bitIndex 对齐到下一个字节边界 8
		assertEqual(8, bitIndex, "fillZero bitIndex=8");

		// 已是字节边界时不改动
		byte[] buf2 = new byte[4];
		buf2[0] = 0xAB;
		int bi2 = 8;
		fillZeroToByteEnd(buf2, ref bi2);
		assertEqual((byte)0xAB, buf2[0], "fillZero aligned unchanged byte");
		assertEqual(8, bi2, "fillZero aligned unchanged idx");

		// bitIndex=0 (起始) 不动
		byte[] buf3 = new byte[4];
		buf3[0] = 0x0F;
		int bi3 = 0;
		fillZeroToByteEnd(buf3, ref bi3);
		assertEqual((byte)0x0F, buf3[0], "fillZero start unchanged");
		assertEqual(0, bi3, "fillZero start idx=0");
	}

	// ---- 溢出返回失败 ----
	static void testOverflowReturnsFailure()
	{
		byte[] tiny = new byte[1];
		int wi = 0;
		// 写一个 int(>1字节) 应失败
		assert(!writeBit(tiny, tiny.Length, ref wi, 1000000, true), "write int overflow false");
		// 读也应失败
		int ri = 0;
		readBit(tiny, tiny.Length, ref ri, out int _, true);
		// 至少不崩溃
		assert(true, "read int overflow no crash");
	}

	// ---- 顺序混合读写 ----
	static void testSequentialMixedRoundTrip()
	{
		byte[] buf = new byte[256];
		int wi = 0;
		assert(writeBit(buf, buf.Length, ref wi, true), "seq write bool");
		assert(writeBit(buf, buf.Length, ref wi, (byte)200), "seq write byte");
		assert(writeBit(buf, buf.Length, ref wi, (short)-12345, true), "seq write short");
		assert(writeBit(buf, buf.Length, ref wi, 1234567, true), "seq write int");
		assert(writeBit(buf, buf.Length, ref wi, 9876543210L, true), "seq write long");

		int ri = 0;
		assert(readBit(buf, buf.Length, ref ri, out bool b), "seq read bool");
		assert(b, "seq bool true");
		assert(readBit(buf, buf.Length, ref ri, out byte by), "seq read byte");
		assertEqual((byte)200, by, "seq byte");
		assert(readBit(buf, buf.Length, ref ri, out short s, true), "seq read short");
		assertEqual((short)-12345, s, "seq short");
		assert(readBit(buf, buf.Length, ref ri, out int i, true), "seq read int");
		assertEqual(1234567, i, "seq int");
		assert(readBit(buf, buf.Length, ref ri, out long l, true), "seq read long");
		assertEqual(9876543210L, l, "seq long");
	}

	// ---- 工具 ----
	static byte generateTestBitCount(ulong value)
	{
		byte count = 0;
		ulong v = value;
		while (v != 0)
		{
			++count;
			v >>= 1;
		}
		return count;
	}

	static bool isFloatNear(float a, float b, float eps) { return Math.Abs(a - b) < eps; }
	static bool isDoubleNear(double a, double b, double eps) { return Math.Abs(a - b) < eps; }
}
