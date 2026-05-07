#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

// DoubleBuffer<T> 测试：add/get/endGet/clear/setWriteListLimit
public static class DoubleBufferTest
{
	public static void Run()
	{
		testAddSingle();
		testAddMultiple();
		testRoundtrip();
		testEndGetRelease();
		testClear();
		testWriteLimit();
	}

	private static void testAddSingle()
	{
		var b = new DoubleBuffer<int>();
		b.add(42);
		var list = b.get();
		AssertEqual(1, list.Count);
		AssertEqual(42, list[0]);
		b.endGet();
	}

	private static void testAddMultiple()
	{
		var b = new DoubleBuffer<int>();
		for (int i = 0; i < 10; i++) { b.add(i); }
		var list = b.get();
		AssertEqual(10, list.Count);
		AssertEqual(0, list[0]);
		AssertEqual(9, list[9]);
		b.endGet();
	}

	private static void testRoundtrip()
	{
		var b = new DoubleBuffer<int>();
		b.add(1); b.add(2); b.add(3);
		var l1 = b.get();
		AssertEqual(3, l1.Count);
		b.endGet();

		b.add(4); b.add(5);
		var l2 = b.get();
		AssertEqual(2, l2.Count);
		AssertEqual(4, l2[0]);
		AssertEqual(5, l2[1]);
		b.endGet();
	}

	private static void testEndGetRelease()
	{
		var b = new DoubleBuffer<int>();
		b.add(1);
		var l1 = b.get();
		AssertNotNull(l1);
		b.endGet();

		b.add(2);
		var l2 = b.get();
		AssertNotNull(l2);
		AssertEqual(1, l2.Count);
		b.endGet();
	}

	private static void testClear()
	{
		var b = new DoubleBuffer<int>();
		b.add(1); b.add(2); b.add(3);
		b.clear();
		var list = b.get();
		AssertEqual(0, list.Count);
		b.endGet();
	}

	private static void testWriteLimit()
	{
		var b = new DoubleBuffer<int>();
		b.setWriteListLimit(3);
		b.add(1); b.add(2); b.add(3); b.add(4); b.add(5);
		var list = b.get();
		AssertEqual(3, list.Count);
		AssertEqual(1, list[0]);
		AssertEqual(3, list[2]);
		b.endGet();
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertNotNull(object o) { if (o == null) throw new Exception("Should not be null"); }
}
#endif
