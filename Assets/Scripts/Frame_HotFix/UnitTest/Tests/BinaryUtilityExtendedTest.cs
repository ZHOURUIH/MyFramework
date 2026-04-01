#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static BinaryUtility;
using static TestAssert;

public static class BinaryUtilityExtendedTest
{
    public static void Run()
    {
        testEncodingAndSearch();
        testCrcAndMemory();
        testBitHelpers();
    }

    private static void testEncodingAndSearch()
    {
        assertNotNull(getGB2312(), "gb2312 encoding");
        assertNotNull(getGBK(), "gbk encoding");
        assertTrue(contains(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 2, 3, 4 }), "contains true");
        assertFalse(contains(new byte[] { 1, 2, 3 }, new byte[] { 4 }), "contains false");
		assertEqual("123", removeLastZero("123\0\0\0"), "removeLastZero");
    }

    private static void testCrcAndMemory()
    {
        byte[] data = { 1, 2, 3, 4, 5 };
        ushort c0 = crc16((ushort)0x1F, data, data.Length);
        ushort c1 = crc16((ushort)0x1F, (byte)1, (byte)2, (byte)3, (byte)4);
        assertTrue(c0 != 0, "crc array non-zero");
        assertTrue(c1 != 0, "crc bytes non-zero");

        byte[] src = { 10, 20, 30, 40 };
        byte[] dst = { 0, 0, 0, 0 };
        memcpy(dst, src, 0, 0, 4);
        assertTrue(isMemoryEqual(dst, src, 4), "memcpy equal");
        memmove(dst, 1, 0, 3);
        assertEqual((byte)10, dst[0], "memmove first");
        memset(dst, (byte)7, 1, 2);
        assertEqual((byte)7, dst[1], "memset first");
        assertEqual((byte)7, dst[2], "memset second");
    }

    private static void testBitHelpers()
    {
        assertEqual(0, bitCount1((byte)0), "bit count zero");
        assertEqual(1, bitCount1((byte)1), "bit count one");
        assertEqual(8, bitCount1((byte)255), "bit count all");
        byte value = 0;
        setBitOne(ref value, 3);
        assertTrue(hasBit(value, 3), "set bit one");
        setBitZero(ref value, 3);
        assertFalse(hasBit(value, 3), "set bit zero");
        setLowestBit(ref value, 1);
        assertEqual(1, getLowestBit(value), "lowest bit");
        value = 0;
        setHighestBit(ref value, 1);
        assertEqual(1, getHighestBit(value), "highest bit");
        byte[] buf = new byte[2];
        setBufferBitOne(buf, 10);
        assertTrue(getBufferBit(buf, 10), "buffer bit");
    }
}
#endif
