#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static BinaryUtility;
using static MathUtility;

// Binary/Time 边缘测试
public static class BinaryTimeFileUtilityEdgeTest
{
	public static void Run()
	{
		testCRC16Edge();
		testMemmoveEdge();
		testSaturate();
		testFrac();
		testStep();
	}

	private static void testCRC16Edge()
	{
		byte[] empty = { };
		ushort crc = crc16(0, empty, 0);
		AssertEqual((ushort)0, crc);
	}

	private static void testMemmoveEdge()
	{
		int[] d = { 1, 2, 3, 4, 5, 6 };
		memmove(d, 0, 4, 2);
		AssertEqual(5, d[0]);
	}

	private static void testSaturate()
	{
		AssertEqual(0f, saturate(-1f));
		AssertEqual(0.5f, saturate(0.5f));
		AssertEqual(1f, saturate(2f));
	}

	private static void testFrac()
	{
		AssertEqual(0f, frac(0f));
		AssertEqual(0.5f, frac(0.5f));
		AssertEqual(0f, frac(1f));
	}

	private static void testStep()
	{
		AssertEqual(0f, step(5f, 3f));
		AssertEqual(1f, step(5f, 5f));
		AssertEqual(1f, step(5f, 7f));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(ushort e, ushort a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
