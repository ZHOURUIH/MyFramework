#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using static TestAssert;

public class SafeDeepDictionaryTest
{
	public static void Run()
	{
		testAddAndCount();
		testContainsKey();
		testTryGetValue();
		testTryGet();
		testRemove();
		testClear();
		testStartForeachReturnsCopy();
		testEndForeach();
		testNestedForeach();
		testGetMainList();
		testAddDuringForeach();
		testRemoveDuringForeach();
		testClearDuringForeach();
		testResetProperty();
		testEnumerator();
		testMultipleInstances();
		testEmptyDictionary();
		testDuplicateKeyThrows();
		testStartForeachReusesTempList();
		testMultipleStartForeachEndForeach();
		testGetMainListModification();
		testAddThenTryGetValue();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddAndCount()
	{
		SafeDeepDictionary<string, int> dict = new();
		assertEqual(0, dict.count());
		dict.add("a", 1);
		assertEqual(1, dict.count());
		dict.add("b", 2);
		dict.add("c", 3);
		assertEqual(3, dict.count());
	}
	private static void testContainsKey()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("key1", 100);
		assertTrue(dict.containsKey("key1"));
		assertFalse(dict.containsKey("key2"));
	}
	private static void testTryGetValue()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("key", 42);
		int val;
		assertTrue(dict.tryGetValue("key", out val));
		assertEqual(42, val);
		assertFalse(dict.tryGetValue("nonexistent", out val));
	}
	private static void testTryGet()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("key", 99);
		assertEqual(99, dict.tryGet("key"));
	}
	private static void testRemove()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.remove("a");
		assertEqual(1, dict.count());
		assertFalse(dict.containsKey("a"));
		assertTrue(dict.containsKey("b"));
	}
	private static void testClear()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("x", 10);
		dict.add("y", 20);
		dict.clear();
		assertEqual(0, dict.count());
		assertFalse(dict.containsKey("x"));
	}
	private static void testStartForeachReturnsCopy()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		Dictionary<string, int> snapshot = dict.startForeach();
		assertEqual(2, snapshot.Count);
		// 修改 snapshot 不应影响原字典
		snapshot.Clear();
		snapshot["c"] = 999;
		assertEqual(2, dict.count());
		assertTrue(dict.containsKey("a"));
		dict.endForeach(snapshot);
	}
	private static void testEndForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		Dictionary<string, int> snapshot = dict.startForeach();
		dict.endForeach(snapshot);
	}
	private static void testNestedForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		Dictionary<string, int> outer = dict.startForeach();
		// 嵌套遍历
		Dictionary<string, int> inner = dict.startForeach();
		assertEqual(2, inner.Count);
		dict.endForeach(inner);
		dict.endForeach(outer);
	}
	private static void testGetMainList()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		Dictionary<string, int> main = dict.getMainList();
		assertEqual(2, main.Count);
		assertEqual(1, main["a"]);
		assertEqual(2, main["b"]);
	}
	private static void testAddDuringForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		Dictionary<string, int> snapshot = dict.startForeach();
		dict.add("b", 2);
		// 快照只有原始数据
		assertEqual(1, snapshot.Count);
		// 主字典有新元素
		assertEqual(2, dict.count());
		dict.endForeach(snapshot);
	}
	private static void testRemoveDuringForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		Dictionary<string, int> snapshot = dict.startForeach();
		dict.remove("a");
		// 快照不受影响
		assertEqual(2, snapshot.Count);
		// 主字典已删除
		assertEqual(1, dict.count());
		dict.endForeach(snapshot);
	}
	private static void testClearDuringForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		Dictionary<string, int> snapshot = dict.startForeach();
		dict.clear();
		assertEqual(1, snapshot.Count); // 快照不受影响
		assertEqual(0, dict.count());
		dict.endForeach(snapshot);
	}
	private static void testResetProperty()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("k", 1);
		dict.resetProperty();
		assertEqual(0, dict.count());
	}
	private static void testEnumerator()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("x", 10);
		dict.add("y", 20);
		int sum = 0;
		int count = 0;
		var enumerator = dict.GetEnumerator();
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current.Value;
			count++;
		}
		assertEqual(2, count);
		assertEqual(30, sum);
	}
	private static void testMultipleInstances()
	{
		SafeDeepDictionary<string, int> d1 = new();
		SafeDeepDictionary<string, int> d2 = new();
		d1.add("a", 1);
		d2.add("b", 2);
		d2.add("c", 3);
		assertEqual(1, d1.count());
		assertEqual(2, d2.count());
		assertTrue(d1.containsKey("a"));
		assertTrue(d2.containsKey("c"));
		assertFalse(d1.containsKey("b"));
	}
	private static void testEmptyDictionary()
	{
		SafeDeepDictionary<string, int> dict = new();
		assertEqual(0, dict.count());
		assertFalse(dict.containsKey("anything"));
		int val;
		assertFalse(dict.tryGetValue("nothing", out val));
		Dictionary<string, int> snapshot = dict.startForeach();
		assertEqual(0, snapshot.Count);
		dict.endForeach(snapshot);
	}
	private static void testDuplicateKeyThrows()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("k", 1);
		bool threw = false;
		try
		{
			dict.add("k", 2);
		}
		catch
		{
			threw = true;
		}
		assertTrue(threw, "重复 key 应抛出异常");
	}
	private static void testStartForeachReusesTempList()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		Dictionary<string, int> snap1 = dict.startForeach();
		dict.endForeach(snap1);
		Dictionary<string, int> snap2 = dict.startForeach();
		assertEqual(1, snap2.Count);
		dict.endForeach(snap2);
	}
	private static void testMultipleStartForeachEndForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		for (int i = 0; i < 5; ++i)
		{
			Dictionary<string, int> snapshot = dict.startForeach();
			assertEqual(2, snapshot.Count);
			dict.endForeach(snapshot);
		}
		assertEqual(2, dict.count());
	}
	private static void testGetMainListModification()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		Dictionary<string, int> main = dict.getMainList();
		main.Add("b", 2);
		assertEqual(2, dict.count());
		assertTrue(dict.containsKey("b"));
		assertEqual(2, dict.tryGet("b"));
	}
	private static void testAddThenTryGetValue()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("test", 777);
		int val;
		assertTrue(dict.tryGetValue("test", out val));
		assertEqual(777, val);
	}
}
#endif
