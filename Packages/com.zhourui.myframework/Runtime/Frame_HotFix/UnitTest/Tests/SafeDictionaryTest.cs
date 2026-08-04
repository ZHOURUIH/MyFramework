using System;
using System.Collections.Generic;
using static TestAssert;

// SafeDictionary<Key, Value> 穷举测试（覆盖所有公开 API 和关键分支）
public static class SafeDictionaryTest
{
	public static void Run()
	{
		// --- 基础增删改查 ---
		testAddAndCount();
		testAddDuplicateKey();
		testContainsKey();
		testContainsValue();
		testRemove();
		testRemoveNonExistent();
		testClear();
		testGet();
		testIsEmpty();

		// --- 遍历相关 ---
		testFor();
		testForKey();
		testForValue();
		testForKeyEmpty();
		testForValueEmpty();
		testTryGetValue();
		testTryGetValueNonExistent();
		testStartForeachEndForeach();
		testIsForeaching();
		testClearDuringForeach();
		testStartForeachModifySync();
		testGetEnumerator();

		// --- 主列表 ---
		testGetMainList();

		// --- 条件操作 ---
		testAddIf_True();
		testAddIf_False();
		testRemoveIf_True();
		testRemoveIf_False();

		// --- 重置 ---
		testResetProperty();

		// --- 扩展方法 ---
		testAddClass();
	}

	//==================================================================
	// 基础增删改查
	//==================================================================
	private static void testAddAndCount()
	{
		SafeDictionary<string, int> d = new();
		assertEqual(0, d.count());
		assertTrue(d.add("a", 1));
		assertEqual(1, d.count());
		assertTrue(d.add("b", 2));
		assertTrue(d.add("c", 3));
		assertEqual(3, d.count());
	}

	private static void testAddDuplicateKey()
	{
		SafeDictionary<string, int> d = new();
		assertTrue(d.add("key", 100));
		// 重复 key 返回 false
		assertFalse(d.add("key", 200));
		assertEqual(1, d.count());
		// value 仍是第一次的值
		assertTrue(d.tryGetValue("key", out int v));
		assertEqual(100, v);
	}

	private static void testContainsKey()
	{
		SafeDictionary<string, int> d = new();
		d.add("hello", 1);
		assertTrue(d.containsKey("hello"));
		assertFalse(d.containsKey("world"));
	}

	private static void testContainsValue()
	{
		SafeDictionary<int, string> d = new();
		d.add(1, "one");
		assertTrue(d.containsValue("one"));
		assertFalse(d.containsValue("two"));
	}

	private static void testRemove()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		bool removed = d.remove("b");
		assertTrue(removed);
		assertEqual(2, d.count());
		assertFalse(d.containsKey("b"));
		assertTrue(d.containsKey("a"));
		assertTrue(d.containsKey("c"));
	}

	private static void testRemoveNonExistent()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		bool removed = d.remove("nonexistent");
		assertFalse(removed);
		assertEqual(1, d.count());
	}

	private static void testClear()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.clear();
		assertEqual(0, d.count());
		assertTrue(d.isEmpty());
		// clear 后可继续添加
		d.add("c", 3);
		assertEqual(1, d.count());
	}

	private static void testGet()
	{
		SafeDictionary<string, int> d = new();
		d.add("x", 42);
		d.add("y", 99);
		assertEqual(42, d.get("x"));
		assertEqual(99, d.get("y"));
	}

	private static void testIsEmpty()
	{
		SafeDictionary<int, string> d = new();
		assertTrue(d.isEmpty());
		d.add(1, "one");
		assertFalse(d.isEmpty());
		d.clear();
		assertTrue(d.isEmpty());
	}

	//==================================================================
	// 遍历相关
	//==================================================================
	private static void testFor()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		int sumKey = 0;
		int sumValue = 0;
		d.For(kv =>
		{
			sumValue += kv.Value;
		});
		assertEqual(6, sumValue);
	}

	private static void testForKey()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		int count = 0;
		d.forKey(k =>
		{
			if (k == "a" || k == "b") count++;
		});
		assertEqual(2, count);
	}

	private static void testForValue()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 5);
		d.add("b", 10);
		int sum = 0;
		d.forValue(v => sum += v);
		assertEqual(15, sum);
	}

	private static void testForKeyEmpty()
	{
		SafeDictionary<string, int> d = new();
		// 空字典 forKey 不应抛异常
		d.forKey(k => assertTrue(false)); // 不应执行
	}

	private static void testForValueEmpty()
	{
		SafeDictionary<string, int> d = new();
		// 空字典 forValue 不应抛异常
		d.forValue(v => assertTrue(false)); // 不应执行
	}

	private static void testTryGetValue()
	{
		SafeDictionary<string, int> d = new();
		d.add("k", 42);
		assertTrue(d.tryGetValue("k", out int v));
		assertEqual(42, v);
	}

	private static void testTryGetValueNonExistent()
	{
		SafeDictionary<string, int> d = new();
		d.add("k", 1);
		assertFalse(d.tryGetValue("nonexistent", out int v));
		assertEqual(default(int), v);
	}

	private static void testStartForeachEndForeach()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		Dictionary<string, int> iter = d.startForeach();
		assertNotNull(iter);
		assertEqual(3, iter.Count);
		assertTrue(iter.ContainsKey("a"));
		assertTrue(iter.ContainsKey("b"));
		assertTrue(iter.ContainsKey("c"));
		d.endForeach();
		assertFalse(d.isForeaching());
	}

	private static void testIsForeaching()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		assertFalse(d.isForeaching());
		d.startForeach();
		assertTrue(d.isForeaching());
		d.endForeach();
		assertFalse(d.isForeaching());
	}

	private static void testClearDuringForeach()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		var iter = d.startForeach();
		assertEqual(3, iter.Count);
		// 遍历中清空
		d.clear();
		// 快照不受影响
		assertEqual(3, iter.Count);
		// 主列表已清空
		assertEqual(0, d.count());
		d.endForeach();
	}

	private static void testStartForeachModifySync()
	{
		// 测试 startForeach 将 modifyList 同步到 updateList
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		// 先做一次遍历清空 modifyList
		{
			var iter = d.startForeach();
			assertEqual(3, iter.Count);
			d.endForeach();
		}
		// 修改后再次遍历验证同步
		d.add("d", 4);
		d.remove("a");
		{
			var iter = d.startForeach();
			assertEqual(3, iter.Count); // 移除 a 添加 d = 3
			assertTrue(iter.ContainsKey("b"));
			assertTrue(iter.ContainsKey("c"));
			assertTrue(iter.ContainsKey("d"));
			assertFalse(iter.ContainsKey("a"));
			d.endForeach();
		}
	}

	private static void testGetEnumerator()
	{
		SafeDictionary<string, int> d = new();
		d.add("x", 10);
		d.add("y", 20);
		int sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
		}
		assertEqual(30, sum);
	}

	//==================================================================
	// 主列表
	//==================================================================
	private static void testGetMainList()
	{
		SafeDictionary<string, int> d = new();
		d.add("key1", 100);
		d.add("key2", 200);
		var main = d.getMainList();
		assertNotNull(main);
		assertEqual(2, main.Count);
		assertEqual(100, main["key1"]);
		assertEqual(200, main["key2"]);
	}

	//==================================================================
	// 条件操作
	//==================================================================
	private static void testAddIf_True()
	{
		SafeDictionary<string, int> d = new();
		d.addIf("a", 1, true);
		assertEqual(1, d.count());
		assertTrue(d.containsKey("a"));
	}

	private static void testAddIf_False()
	{
		SafeDictionary<string, int> d = new();
		d.addIf("a", 1, false);
		assertEqual(0, d.count());
		assertFalse(d.containsKey("a"));
	}

	private static void testRemoveIf_True()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		bool removed = d.removeIf("a", true);
		assertTrue(removed);
		assertEqual(1, d.count());
		assertFalse(d.containsKey("a"));
	}

	private static void testRemoveIf_False()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		bool removed = d.removeIf("a", false);
		assertFalse(removed);
		assertEqual(1, d.count());
		assertTrue(d.containsKey("a"));
	}

	//==================================================================
	// 重置
	//==================================================================
	private static void testResetProperty()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.startForeach();
		d.resetProperty();
		assertEqual(0, d.count());
		assertTrue(d.isEmpty());
		assertFalse(d.isForeaching());
		// reset 后可正常使用
		d.add("c", 3);
		assertEqual(1, d.count());
		assertEqual(3, d.get("c"));
	}

	//==================================================================
	// 扩展方法
	//==================================================================
	private static void testAddClass()
	{
		var d = new SafeDictionary<string, TestSafeDictionaryClassObject>();
		var obj = d.addClass("obj1");
		assertNotNull(obj);
		assertEqual(1, d.count());
		assertTrue(d.containsKey("obj1"));
	}
}

// 用于测试 SafeDictionaryExtension.addClass 的辅助类
public class TestSafeDictionaryClassObject : ClassObject
{
	public TestSafeDictionaryClassObject() { }
}
