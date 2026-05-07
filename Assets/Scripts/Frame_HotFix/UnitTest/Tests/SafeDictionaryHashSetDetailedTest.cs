#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// SafeDictionary / SafeHashSet 基础 API 测试（仅测试 new 可用的方法）
public static class SafeDictionaryHashSetDetailedTest
{
	public static void Run()
	{
		testDictAddCount();
		testDictRemove();
		testDictContainsKey();
		testDictClear();
		testDictGet();
		testHashSetAdd();
		testHashSetRemove();
		testHashSetContains();
		testHashSetClear();
		testHashSetCount();
	}

	private static void testDictAddCount()
	{
		var d = new SafeDictionary<int, string>();
		d.add(1, "a");
		d.add(2, "b");
		AssertEqual(2, d.count());
	}

	private static void testDictRemove()
	{
		var d = new SafeDictionary<int, string>();
		d.add(1, "a");
		d.add(2, "b");
		d.remove(1);
		AssertEqual(1, d.count());
	}

	private static void testDictContainsKey()
	{
		var d = new SafeDictionary<int, string>();
		d.add(5, "x");
		Assert(d.containsKey(5));
	}

	private static void testDictClear()
	{
		var d = new SafeDictionary<int, string>();
		d.add(1, "a");
		d.clear();
		AssertEqual(0, d.count());
	}

	private static void testDictGet()
	{
		var d = new SafeDictionary<int, string>();
		d.add(3, "val");
		AssertEqual("val", d.get(3));
	}

	private static void testHashSetAdd()
	{
		var s = new SafeHashSet<int>();
		s.add(1);
		s.add(2);
		s.add(1);
		AssertEqual(2, s.count());
	}

	private static void testHashSetRemove()
	{
		var s = new SafeHashSet<int>();
		s.add(1);
		s.add(2);
		s.remove(2);
		AssertEqual(1, s.count());
	}

	private static void testHashSetContains()
	{
		var s = new SafeHashSet<int>();
		s.add(42);
		Assert(s.contains(42));
	}

	private static void testHashSetClear()
	{
		var s = new SafeHashSet<int>();
		s.add(1);
		s.add(2);
		s.clear();
		AssertEqual(0, s.count());
	}

	private static void testHashSetCount()
	{
		var s = new SafeHashSet<int>();
		AssertEqual(0, s.count());
		s.add(1);
		AssertEqual(1, s.count());
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
