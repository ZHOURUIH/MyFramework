#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static SerializeBitUtility;
using static TestAssert;

public static class SerializeBitUtilityCoverageTest
{
    public static void Run()
    {
        testPrimitiveRoundTrips();
        testListRoundTrips();
        testOverflowAndBitCounts();
        testFloatAndDoubleRoundTrips();
    }

    private static void testPrimitiveRoundTrips()
    {
        byte[] buffer = new byte[2048];
        int bitIndex = 0;

        assert(writeBit(buffer, buffer.Length, ref bitIndex, true), "write bool");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, (sbyte)-12, true), "write sbyte");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, (byte)250), "write byte");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, (short)-12345, true), "write short");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, (ushort)54321), "write ushort");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, -123456789, true), "write int");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, 3456789012u), "write uint");
        assert(writeBit(buffer, buffer.Length, ref bitIndex, -123456789012345L, true), "write long");

        int readIndex = 0;
        assert(readBit(buffer, buffer.Length, ref readIndex, out bool b), "read bool");
        assert(readBit(buffer, buffer.Length, ref readIndex, out sbyte sb, true), "read sbyte");
        assert(readBit(buffer, buffer.Length, ref readIndex, out byte by), "read byte");
        assert(readBit(buffer, buffer.Length, ref readIndex, out short sh, true), "read short");
        assert(readBit(buffer, buffer.Length, ref readIndex, out ushort ush), "read ushort");
        assert(readBit(buffer, buffer.Length, ref readIndex, out int i, true), "read int");
        assert(readBit(buffer, buffer.Length, ref readIndex, out uint ui), "read uint");
        assert(readBit(buffer, buffer.Length, ref readIndex, out long l, true), "read long");

        assertTrue(b, "bool value true");
        assertEqual((sbyte)-12, sb, "sbyte value");
        assertEqual((byte)250, by, "byte value");
        assertEqual((short)-12345, sh, "short value");
        assertEqual((ushort)54321, ush, "ushort value");
        assertEqual(-123456789, i, "int value");
        assertEqual(3456789012u, ui, "uint value");
        assertEqual(-123456789012345L, l, "long value");
    }

    private static void testListRoundTrips()
    {
        byte[] buffer = new byte[4096];
        int bitIndex = 0;
        List<int> ints = new() { -3, -1, 0, 1, 3, 1024 };
        List<float> floats = new() { -3.5f, -1.25f, 0f, 1.25f, 3.5f };

        assert(writeListBit(buffer, buffer.Length, ref bitIndex, ints, true), "write int list");
        assert(writeListBit(buffer, buffer.Length, ref bitIndex, floats, true), "write float list");

        int readIndex = 0;
        List<int> intsBack = new();
        List<float> floatsBack = new();
        assert(readListBit(buffer, buffer.Length, ref readIndex, intsBack, true), "read int list");
        assert(readListBit(buffer, buffer.Length, ref readIndex, floatsBack, true), "read float list");

		assertEqual(ints.Count, intsBack.Count, "int list count");
		assertEqual(floats.Count, floatsBack.Count, "float list count");
		for (int i = 0; i < ints.Count; ++i)
		{
			assertEqual(ints[i], intsBack[i], "int list item");
		}
		for (int i = 0; i < floats.Count; ++i)
		{
			assertTrue(Math.Abs(floats[i] - floatsBack[i]) < 0.001f, "float list item");
		}
    }

    private static void testFloatAndDoubleRoundTrips()
    {
        byte[] buffer = new byte[4096];
        int bitIndex = 0;

        Span<float> floats = stackalloc float[] { -12345.25f, 0.0f, 12345.25f };
        Span<double> doubles = stackalloc double[] { -98765.5, 0.0, 98765.5 };

        assert(writeListBit(buffer, buffer.Length, ref bitIndex, floats, true, 2), "write float span");
        assert(writeListBit(buffer, buffer.Length, ref bitIndex, doubles, true, 2), "write double span");

        int readIndex = 0;
        Span<float> floatsBack = stackalloc float[floats.Length];
        Span<double> doublesBack = stackalloc double[doubles.Length];

        assert(readListBit(buffer, buffer.Length, ref readIndex, ref floatsBack, true, 2), "read float span");
        assert(readListBit(buffer, buffer.Length, ref readIndex, ref doublesBack, true, 2), "read double span");

        for (int i = 0; i < floats.Length; ++i)
        {
            assertTrue(Math.Abs(floats[i] - floatsBack[i]) < 0.01f, "float span item");
        }

        for (int i = 0; i < doubles.Length; ++i)
        {
            assertTrue(Math.Abs(doubles[i] - doublesBack[i]) < 0.01, "double span item");
        }
    }

    private static void testOverflowAndBitCounts()
    {
        byte[] tiny = new byte[1];
        int bitIndex = 0;
        assertFalse(writeBit(tiny, tiny.Length, ref bitIndex, int.MaxValue, true), "write overflow");

        int readIndex = 0;
        assertFalse(readBit(tiny, tiny.Length, ref readIndex, out int _, true), "read overflow");

        assertEqual(0, bitCountToByteCount(0), "bitcount 0");
        assertEqual(1, bitCountToByteCount(1), "bitcount 1");
        assertEqual(1, bitCountToByteCount(8), "bitcount 8");
        assertEqual(2, bitCountToByteCount(9), "bitcount 9");
    }
}
#endif
