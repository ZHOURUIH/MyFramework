#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static SerializeBitUtility;
using static TestAssert;
using static MathUtility;

// SerializeBitUtility edge case tests that focus on boundary values and mixed type coverage.
public static class SerializeBitUtilityEdgeCasesTest
{
	public static void Run()
	{
		testPrimitiveBoundaryRoundTrips();
		testMixedSequenceRoundTrip();
		testListAndSpanRoundTrips();
		testOverflowFailures();
		testBitBoundaryLengths();
	}

	private static void testPrimitiveBoundaryRoundTrips()
	{
		byte[] buffer = new byte[65536];
		int bitIndex = 0;

		assert(writeBit(buffer, buffer.Length, ref bitIndex, true), "write bool true");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, false), "write bool false");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, int.MinValue + 1, true), "write int min");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, int.MaxValue, true), "write int max");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, uint.MaxValue), "write uint max");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, long.MinValue + 1, true), "write long min");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, long.MaxValue, true), "write long max");

		Span<float> floats = stackalloc float[] { -12345.5f, 12345.5f };
		Span<double> doubles = stackalloc double[] { -1234567.5, 1234567.5 };
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, floats, true, 2), "write float span");
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, doubles, true, 2), "write double span");

		int readIndex = 0;
		assert(readBit(buffer, buffer.Length, ref readIndex, out bool b0), "read bool0");
		assert(readBit(buffer, buffer.Length, ref readIndex, out bool b1), "read bool1");
		assertTrue(b0, "bool true roundtrip");
		assertFalse(b1, "bool false roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out int i0, true), "read int min");
		assertEqual(int.MinValue + 1, i0, "int min roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out int i1, true), "read int max");
		assertEqual(int.MaxValue, i1, "int max roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint u0), "read uint max");
		assertEqual(uint.MaxValue, u0, "uint max roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out long l0, true), "read long min");
		assertEqual(long.MinValue + 1, l0, "long min roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out long l1, true), "read long max");
		assertEqual(long.MaxValue, l1, "long max roundtrip");

		Span<float> floatsBack = stackalloc float[floats.Length];
		Span<double> doublesBack = stackalloc double[doubles.Length];
		assert(readListBit(buffer, buffer.Length, ref readIndex, ref floatsBack, true, 2), "read float span");
		assert(readListBit(buffer, buffer.Length, ref readIndex, ref doublesBack, true, 2), "read double span");

		assertTrue(abs(floatsBack[0] - (-12345.5f)) < 0.01f, "float min roundtrip");
		assertTrue(abs(floatsBack[1] - 12345.5f) < 0.01f, "float max roundtrip");
		assertTrue(abs(doublesBack[0] - (-1234567.5)) < 0.01, "double min roundtrip");
		assertTrue(abs(doublesBack[1] - 1234567.5) < 0.01, "double max roundtrip");
	}

	private static void testMixedSequenceRoundTrip()
	{
		byte[] buffer = new byte[65536];
		int bitIndex = 0;

		assert(writeBit(buffer, buffer.Length, ref bitIndex, (sbyte)-1, true), "write sbyte");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (byte)255), "write byte");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (short)-32767, true), "write short min");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)65535), "write ushort max");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, 123456789, true), "write int");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, 4000000000u), "write uint");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, 1234567890123L, true), "write long");

		List<float> floats = new() { -3.141f, 0.0f, 2.718f };
		List<double> doubles = new() { -9.0, 0.0, 9.0 };
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, floats, true, 3), "write float list");
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, doubles, true, 3), "write double list");

		int readIndex = 0;
		assert(readBit(buffer, buffer.Length, ref readIndex, out sbyte s0, true), "read sbyte");
		assertEqual((sbyte)-1, s0, "sbyte roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out byte by0), "read byte");
		assertEqual((byte)255, by0, "byte roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out short sh0, true), "read short");
		assertEqual((short)-32767, sh0, "short roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out ushort ush0), "read ushort");
		assertEqual((ushort)65535, ush0, "ushort roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out int i0, true), "read int");
		assertEqual(123456789, i0, "int roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui0), "read uint");
		assertEqual(4000000000u, ui0, "uint roundtrip");
		assert(readBit(buffer, buffer.Length, ref readIndex, out long l0, true), "read long");
		assertEqual(1234567890123L, l0, "long roundtrip");

		List<float> floatsBack = new();
		List<double> doublesBack = new();
		assert(readListBit(buffer, buffer.Length, ref readIndex, floatsBack, true, 3), "read float list");
		assert(readListBit(buffer, buffer.Length, ref readIndex, doublesBack, true, 3), "read double list");

		assertEqual(floats.Count, floatsBack.Count, "float list count");
		assertEqual(doubles.Count, doublesBack.Count, "double list count");
		for (int i = 0; i < floats.Count; ++i)
		{
			assertTrue(abs(floats[i] - floatsBack[i]) < 0.01f, "float list item");
		}
		for (int i = 0; i < doubles.Count; ++i)
		{
			assertTrue(abs(doubles[i] - doublesBack[i]) < 0.01, "double list item");
		}
	}

	private static void testListAndSpanRoundTrips()
	{
		byte[] buffer = new byte[65536];
		int bitIndex = 0;

		Span<int> ints = stackalloc int[] { -3, -1, 0, 1, 3, 1024 };
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, ints, true), "write int span");
		List<uint> uints = new() { 0u, 1u, 2u, 3u, 999u, uint.MaxValue };
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, uints), "write uint list");
		Span<float> floats = stackalloc float[] { -3.5f, -1.25f, 0f, 1.25f, 3.5f };
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, floats, true, 2), "write float span");
		List<double> doubles = new() { -9.0, -1.5, 0.0, 1.5, 9.0 };
		assert(writeListBit(buffer, buffer.Length, ref bitIndex, doubles, true, 2), "write double list");

		int readIndex = 0;
		Span<int> intsBack = stackalloc int[ints.Length];
		assert(readListBit(buffer, buffer.Length, ref readIndex, ref intsBack, true), "read int span");
		for (int i = 0; i < ints.Length; ++i)
		{
			assertEqual(ints[i], intsBack[i], "int span item");
		}

		List<uint> uintsBack = new();
		assert(readListBit(buffer, buffer.Length, ref readIndex, uintsBack), "read uint list");
		assertEqual(uints.Count, uintsBack.Count, "uint list count");
		for (int i = 0; i < uints.Count; ++i)
		{
			assertEqual(uints[i], uintsBack[i], "uint list item");
		}

		Span<float> floatsBack = stackalloc float[floats.Length];
		assert(readListBit(buffer, buffer.Length, ref readIndex, ref floatsBack, true, 2), "read float span");
		for (int i = 0; i < floats.Length; ++i)
		{
			assertTrue(abs(floats[i] - floatsBack[i]) < 0.01f, "float span item");
		}

		List<double> doublesBack = new();
		assert(readListBit(buffer, buffer.Length, ref readIndex, doublesBack, true, 2), "read double list");
		assertEqual(doubles.Count, doublesBack.Count, "double list count");
		for (int i = 0; i < doubles.Count; ++i)
		{
			assertTrue(abs(doubles[i] - doublesBack[i]) < 0.01, "double list item");
		}
	}

	private static void testOverflowFailures()
	{
		byte[] tiny = new byte[1];
		int bitIndex = 0;
		assert(!writeBit(tiny, tiny.Length, ref bitIndex, int.MaxValue, true), "writeBit should fail when buffer is too small");

		int readIndex = 0;
		assert(!readBit(tiny, tiny.Length, ref readIndex, out int _, true), "readBit should fail when buffer is too small");
	}

	private static void testBitBoundaryLengths()
	{
		byte[] buffer = new byte[65536];
		int bitIndex = 0;

		assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)0), "write zero ushort");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)1), "write one ushort");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)255), "write 255 ushort");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)256), "write 256 ushort");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)65535), "write max ushort");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (uint)0), "write zero uint");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (uint)1), "write one uint");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (uint)65535), "write 65535 uint");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, (uint)65536), "write 65536 uint");
		assert(writeBit(buffer, buffer.Length, ref bitIndex, uint.MaxValue), "write max uint");

		int readIndex = 0;
		assert(readBit(buffer, buffer.Length, ref readIndex, out ushort u0), "read zero ushort");
		assertEqual((ushort)0, u0, "ushort zero");
		assert(readBit(buffer, buffer.Length, ref readIndex, out ushort u1), "read one ushort");
		assertEqual((ushort)1, u1, "ushort one");
		assert(readBit(buffer, buffer.Length, ref readIndex, out ushort u2), "read 255 ushort");
		assertEqual((ushort)255, u2, "ushort 255");
		assert(readBit(buffer, buffer.Length, ref readIndex, out ushort u3), "read 256 ushort");
		assertEqual((ushort)256, u3, "ushort 256");
		assert(readBit(buffer, buffer.Length, ref readIndex, out ushort u4), "read max ushort");
		assertEqual((ushort)65535, u4, "ushort max");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui0), "read zero uint");
		assertEqual((uint)0, ui0, "uint zero");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui1), "read one uint");
		assertEqual((uint)1, ui1, "uint one");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui2), "read 65535 uint");
		assertEqual((uint)65535, ui2, "uint 65535");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui3), "read 65536 uint");
		assertEqual((uint)65536, ui3, "uint 65536");
		assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui4), "read max uint");
		assertEqual(uint.MaxValue, ui4, "uint max");
	}
}
#endif
