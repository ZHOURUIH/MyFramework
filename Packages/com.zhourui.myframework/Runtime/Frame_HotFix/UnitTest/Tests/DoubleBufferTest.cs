using System;
using static TestAssert;

// DoubleBuffer<T> 穷举测试
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
		testWriteLimitZero();
		testGetBufferList();
		testDestroy();
		testAddAfterClear();
		testWriteLimitDynamic();
	}

	private static void testAddSingle()
	{
		var b = new DoubleBuffer<int>();
		b.add(42);
		var list = b.get();
		assertEqual(1, list.Count);
		assertEqual(42, list[0]);
		b.endGet();
	}

	private static void testAddMultiple()
	{
		var b = new DoubleBuffer<int>();
		for (int i = 0; i < 10; i++)
		{
			b.add(i);
		}
		var list = b.get();
		assertEqual(10, list.Count);
		assertEqual(0, list[0]);
		assertEqual(9, list[9]);
		b.endGet();
	}

	private static void testRoundtrip()
	{
		var b = new DoubleBuffer<int>();
		b.add(1);
		b.add(2);
		b.add(3);
		var l1 = b.get();
		assertEqual(3, l1.Count);
		b.endGet();

		b.add(4);
		b.add(5);
		var l2 = b.get();
		assertEqual(2, l2.Count);
		assertEqual(4, l2[0]);
		assertEqual(5, l2[1]);
		b.endGet();
	}

	private static void testEndGetRelease()
	{
		var b = new DoubleBuffer<int>();
		b.add(1);
		var l1 = b.get();
		assertNotNull(l1);
		b.endGet();

		b.add(2);
		var l2 = b.get();
		assertNotNull(l2);
		assertEqual(1, l2.Count);
		b.endGet();
	}

	private static void testClear()
	{
		var b = new DoubleBuffer<int>();
		b.add(1);
		b.add(2);
		b.add(3);
		b.clear();
		var list = b.get();
		assertEqual(0, list.Count);
		b.endGet();
	}

	private static void testWriteLimit()
	{
		var b = new DoubleBuffer<int>();
		b.setWriteListLimit(3);
		b.add(1);
		b.add(2);
		b.add(3);
		b.add(4);
		b.add(5);
		var list = b.get();
		assertEqual(3, list.Count);
		assertEqual(1, list[0]);
		assertEqual(3, list[2]);
		b.endGet();
	}

	private static void testWriteLimitZero()
	{
		// limit=0 表示无上限
		var b = new DoubleBuffer<int>();
		b.setWriteListLimit(0);
		for (int i = 0; i < 100; i++)
		{
			b.add(i);
		}
		var list = b.get();
		assertEqual(100, list.Count);
		b.endGet();
	}

	private static void testGetBufferList()
	{
		var b = new DoubleBuffer<int>();
		var buffers = b.getBufferList();
		assertNotNull(buffers);
		assertEqual(2, buffers.Length);
		assertNotNull(buffers[0]);
		assertNotNull(buffers[1]);
	}

	private static void testDestroy()
	{
		var b = new DoubleBuffer<int>();
		b.add(1);
		b.add(2);
		b.destroy();
		// destroy 后仍可正常使用（重建缓冲区）
		b.add(3);
		var list = b.get();
		assertEqual(1, list.Count);
		assertEqual(3, list[0]);
		b.endGet();
	}

	private static void testAddAfterClear()
	{
		var b = new DoubleBuffer<int>();
		b.add(1);
		b.add(2);
		b.clear();
		b.add(3);
		b.add(4);
		var list = b.get();
		assertEqual(2, list.Count);
		assertEqual(3, list[0]);
		assertEqual(4, list[1]);
		b.endGet();
	}

	private static void testWriteLimitDynamic()
	{
		var b = new DoubleBuffer<int>();
		b.setWriteListLimit(5);
		for (int i = 0; i < 10; i++)
		{
			b.add(i);
		}
		var list = b.get();
		assertEqual(5, list.Count);
		b.endGet();

		// 增大 limit 后下一轮可以写入更多
		b.setWriteListLimit(10);
		for (int i = 0; i < 15; i++)
		{
			b.add(i);
		}
		list = b.get();
		assertEqual(10, list.Count);
		b.endGet();
	}
}
