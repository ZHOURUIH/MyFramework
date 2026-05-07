#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

// SafeList<T> 详细 API 测试（非 CLASS/UN_CLASS，直接 new）
public static class SafeListDetailedTest
{
	public static void Run()
	{
		testAddRemove();
		testContains();
		testCount();
		testClear();
		testGet();
		testFor();
		testFind();
		testIsForeaching();
		testGetEnumerator();
		testGetMainList();
		testAddUnique();
		testAddNotNull();
		testAddRangeList();
		testSetRangeList();
		testRemoveAt();
	}

	private static void testAddRemove()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		AssertEqual(3, list.count());
		list.remove(2);
		AssertEqual(2, list.count());
	}

	private static void testContains()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		Assert(list.contains(10));
		Assert(!list.contains(999));
	}

	private static void testCount()
	{
		var list = new SafeList<int>();
		AssertEqual(0, list.count());
		list.add(1);
		AssertEqual(1, list.count());
	}

	private static void testClear()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.clear();
		AssertEqual(0, list.count());
	}

	private static void testGet()
	{
		var list = new SafeList<int>();
		list.add(100);
		list.add(200);
		AssertEqual(100, list.get(0));
		AssertEqual(200, list.get(1));
	}

	private static void testFor()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		list.For(v => sum += v);
		AssertEqual(6, sum);
	}

	private static void testFind()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		list.add(30);
		int f = list.find(v => v > 15);
		AssertEqual(20, f);
	}

	private static void testIsForeaching()
	{
		var list = new SafeList<int>();
		list.add(1);
		Assert(!list.isForeaching());
		var iter = list.startForeach();
		Assert(list.isForeaching());
		list.endForeach();
		Assert(!list.isForeaching());
	}

	private static void testGetEnumerator()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list) { sum += v; }
		AssertEqual(6, sum);
	}

	private static void testGetMainList()
	{
		var list = new SafeList<int>();
		list.add(42);
		var main = list.getMainList();
		AssertNotNull(main);
		AssertEqual(1, main.Count);
		AssertEqual(42, main[0]);
	}

	private static void testAddUnique()
	{
		var list = new SafeList<int>();
		list.addUnique(1);
		list.addUnique(2);
		list.addUnique(1);
		AssertEqual(2, list.count());
	}

	private static void testAddNotNull()
	{
		var list = new SafeList<string>();
		list.addNotNull("a");
		list.addNotNull(null);
		list.addNotNull("b");
		AssertEqual(2, list.count());
	}

	private static void testAddRangeList()
	{
		var list = new SafeList<int>();
		var src = new List<int> { 1, 2, 3 };
		list.addRange(src);
		AssertEqual(3, list.count());
		Assert(list.contains(1));
		Assert(list.contains(3));
	}

	private static void testSetRangeList()
	{
		var list = new SafeList<int>();
		list.add(999);
		var src = new List<int> { 7, 8, 9 };
		list.setRange(src);
		AssertEqual(3, list.count());
		Assert(!list.contains(999));
		Assert(list.contains(7));
	}

	private static void testRemoveAt()
	{
		var list = new SafeList<int>();
		list.add(0);
		list.add(1);
		list.add(2);
		list.removeAt(1);
		AssertEqual(2, list.count());
		Assert(!list.contains(1));
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertNotNull(object o) { if (o == null) throw new Exception("Should not be null"); }
}
#endif
