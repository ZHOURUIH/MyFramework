#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Text;
using static BinaryUtility;

// BinaryUtility 剩余方法测试
// 仅调用验证过的 BinaryUtility 公共静态方法
public static class BinaryUtilityRemainingTest
{
	public static void Run()
	{
		testBitCount1();
		testContains();
		testCRC16();
		testCRC16Array();
		testRemoveLastZero();
		testIsMemoryEqual();
		testMemmove();
		testMemset();
		testHasBit();
		testSetBitOne();
		testSetBitZero();
		testGetLowestHighestBit();
		testSetLowestHighestBit();
	}

	private static void testBitCount1()
	{
		AssertEqual(0, bitCount1(0));
		AssertEqual(1, bitCount1(1));
		AssertEqual(3, bitCount1(7));
		AssertEqual(8, bitCount1(0xFF));
	}

	private static void testContains()
	{
		byte[] buf = { 1, 2, 3, 4, 5 };
		Assert(contains(buf, new byte[] { 3, 4 }));
		Assert(!contains(buf, new byte[] { 6, 7 }));
		Assert(!contains(null, buf));
		Assert(!contains(buf, null));
	}

	private static void testCRC16()
	{
		byte[] data = { 0x01, 0x02, 0x03, 0x04 };
		ushort crc = crc16(0, data, data.Length);
		Assert(crc != 0);
		ushort crc2 = crc16(crc, (byte)0x05);
		Assert(crc2 != crc);
	}

	private static void testCRC16Array()
	{
		byte[] data = { 0x12, 0x34, 0x56, 0x78 };
		ushort offset = crc16(0, data, 4, 0);
		ushort direct = crc16(0, (byte)0x12, (byte)0x34, (byte)0x56, (byte)0x78);
		AssertEqual(offset, direct);
	}

	private static void testRemoveLastZero()
	{
		AssertEqual("hello", removeLastZero("hello\0\0"));
		AssertEqual("test", removeLastZero("test"));
	}

	private static void testIsMemoryEqual()
	{
		byte[] a = { 1, 2, 3, 4, 5 };
		byte[] b = { 1, 2, 3, 4, 5 };
		byte[] c = { 1, 2, 9, 4, 5 };
		Assert(isMemoryEqual(a, b, 5));
		Assert(!isMemoryEqual(a, c, 5));
	}

	private static void testMemmove()
	{
		int[] d = { 0, 1, 2, 3, 4, 5 };
		memmove(d, 0, 3, 3);
		AssertEqual(3, d[0]);
	}

	private static void testMemset()
	{
		byte[] d = new byte[5];
		for (int i = 0; i < 5; i++) { d[i] = 0xFF; }
		memset(d, (byte)0, 0, 5);
		for (int i = 0; i < 5; i++) { AssertEqual((byte)0, d[i]); }
	}

	private static void testHasBit()
	{
		Assert(hasBit((int)0x08, 3));
		Assert(!hasBit((int)0x08, 2));
		Assert(hasBit((long)0x100000000L, 32));
		Assert(hasBit((byte)0x80, 7));
	}

	private static void testSetBitOne()
	{
		int v = 0;
		setBitOne(ref v, 5);
		AssertEqual(32, v);
		byte vb = 0;
		setBitOne(ref vb, 0);
		AssertEqual((byte)1, vb);
	}

	private static void testSetBitZero()
	{
		int v = -1;
		setBitZero(ref v, 0);
		AssertEqual(-2, v);
		byte vb = 0xFF;
		setBitZero(ref vb, 7);
		AssertEqual((byte)0x7F, vb);
	}

	private static void testGetLowestHighestBit()
	{
		AssertEqual(1, getLowestBit((byte)1));
		AssertEqual(0, getLowestBit((byte)2));
		AssertEqual(1, getHighestBit((byte)0x80));
		AssertEqual(0, getHighestBit((byte)0x7F));
	}

	private static void testSetLowestHighestBit()
	{
		byte vb = 0;
		setLowestBit(ref vb, 1);
		AssertEqual((byte)1, vb);
		vb = 0x7F;
		setHighestBit(ref vb, 1);
		AssertEqual((byte)0xFF, vb);
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(byte e, byte a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(ushort e, ushort a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
