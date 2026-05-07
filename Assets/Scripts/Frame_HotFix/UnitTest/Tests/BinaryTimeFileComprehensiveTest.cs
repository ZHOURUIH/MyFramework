#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static BinaryUtility;
using static TimeUtility;

// BinaryUtility + TimeUtility 综合测试
public static class BinaryTimeFileComprehensiveTest
{
	public static void Run()
	{
		testCRC16Basic();
		testContainsBytes();
		testIsMemoryEqual();
		testRemoveLastZero();
		testTimeStampConversion();
	}

	private static void testCRC16Basic()
	{
		byte[] d = { 1, 2, 3, 4 };
		ushort c1 = crc16(0, d, d.Length);
		ushort c2 = crc16(0, d, d.Length);
		Assert(c1 != 0);
		AssertEqual(c1, c2);
	}

	private static void testContainsBytes()
	{
		byte[] b = { 1, 2, 3, 4, 5 };
		Assert(contains(b, new byte[] { 3, 4 }));
		Assert(!contains(b, new byte[] { 9 }));
	}

	private static void testIsMemoryEqual()
	{
		byte[] a = { 1, 2, 3 };
		byte[] b = { 1, 2, 3 };
		Assert(isMemoryEqual(a, b, 3));
	}

	private static void testRemoveLastZero()
	{
		AssertEqual("hi", removeLastZero("hi\0"));
		AssertEqual("", removeLastZero(""));
	}

	private static void testTimeStampConversion()
	{
		var dt = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Local);
		long ts = dateTimeToTimeStamp(dt);
		var back = timeStampToDateTime(ts);
		Assert(isSameDay(dt, back));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(ushort e, ushort a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
