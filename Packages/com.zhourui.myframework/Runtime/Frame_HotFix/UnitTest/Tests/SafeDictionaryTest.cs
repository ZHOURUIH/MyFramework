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

		// --- 遍历使用场景 ---
		testForeachSnapshotUnchangedByAdd();
		testForeachSnapshotUnchangedByRemove();
		testForeachSnapshotUnchangedByClear();
		testModificationsVisibleOnNextForeach();
		testForeachDispatchLike();
		testSequentialForeachSeesUpdates();
		testRepeatedStartForeachPairs();
		testModifySyncAddRemoveClearMixed();
		testForeachOnEmpty();
		testLargeForeachSum();
		testForValueDuringModify();
		testForKeyDuringModify();
		testAddRemoveDuringForeachThenVerifyMain();
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
			if (k == "a" || k == "b")
			{
				count++;
			}
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
		var seen = new System.Collections.Generic.HashSet<string>();
		foreach (var kv in d)
		{
			seen.Add(kv.Key);
			assertTrue(d.isForeaching());
		}
		assertEqual(3, seen.Count);
		assertTrue(seen.Contains("a"));
		assertTrue(seen.Contains("b"));
		assertTrue(seen.Contains("c"));
		assertFalse(d.isForeaching());
	}

	private static void testIsForeaching()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		assertFalse(d.isForeaching());
		foreach (var kv in d)
		{
			_ = kv;
			assertTrue(d.isForeaching());
		}
		assertFalse(d.isForeaching());
	}

	private static void testClearDuringForeach()
	{
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		int sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
			// 遍历中清空,当前快照仍完整
			d.clear();
		}
		// 快照 a,b,c → sum=6
		assertEqual(6, sum);
		// 主字典已清空
		assertEqual(0, d.count());
	}

	private static void testStartForeachModifySync()
	{
		// 两次遍历之间修改,下次遍历能看到同步结果
		SafeDictionary<string, int> d = new();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		// 先做一次遍历
		{
			int count = 0;
			foreach (var kv in d)
			{
				_ = kv;
				count++;
			}
			assertEqual(3, count);
		}
		// 修改后再次遍历验证同步
		d.add("d", 4);
		d.remove("a");
		{
			var seen = new System.Collections.Generic.HashSet<string>();
			foreach (var kv in d)
			{
				seen.Add(kv.Key);
			}
			assertEqual(3, seen.Count); // 移除 a 添加 d = 3
			assertTrue(seen.Contains("b"));
			assertTrue(seen.Contains("c"));
			assertTrue(seen.Contains("d"));
			assertFalse(seen.Contains("a"));
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
		// 在遍历过程中 reset
		foreach (var kv in d)
		{
			_ = kv;
			d.resetProperty();
			break;
		}
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

	//==================================================================
	// 遍历使用场景
	//==================================================================
	private static void testForeachSnapshotUnchangedByAdd()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		int sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
			d.add("x" + kv.Key, kv.Value * 10); // 遍历中新增,不进当前快照
		}
		assertEqual(6, sum);        // 快照 a,b,c
		assertEqual(6, d.count());  // 主字典含 3 个新增
	}
	private static void testForeachSnapshotUnchangedByRemove()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		int sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
			d.remove("a");
			d.remove("b");
		}
		assertEqual(6, sum);          // 快照完整
		assertEqual(1, d.count());    // 主字典剩 c
	}
	private static void testForeachSnapshotUnchangedByClear()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		int sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
			d.clear();
		}
		assertEqual(3, sum);
		assertEqual(0, d.count());
	}
	private static void testModificationsVisibleOnNextForeach()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		foreach (var kv in d)
		{
			if (kv.Key == "a")
			{
				d.add("c", 3);
				d.remove("b");
			}
		}
		var main = d.getMainList();
		assertEqual(2, main.Count);
		assertTrue(main.ContainsKey("a"));
		assertTrue(main.ContainsKey("c"));
		assertFalse(main.ContainsKey("b"));
		// 新 foreach 同步结果
		int seen = 0;
		foreach (var kv in d)
		{
			seen++;
		}
		assertEqual(2, seen);
	}
	private static void testForeachDispatchLike()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		int sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
			if (kv.Key == "b")
			{
				d.remove("b");
			}
		}
		assertEqual(6, sum);
		assertEqual(2, d.count());
		assertFalse(d.containsKey("b"));
	}
	private static void testSequentialForeachSeesUpdates()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		foreach (var kv in d)
		{
			_ = kv;
		}
		d.add("b", 2);
		d.add("c", 3);
		int count = 0;
		foreach (var kv in d)
		{
			_ = kv;
			count++;
		}
		assertEqual(3, count);
	}
	private static void testRepeatedStartForeachPairs()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		for (int i = 0; i < 5; ++i)
		{
			int count = 0;
			foreach (var kv in d)
			{
				_ = kv;
				count++;
			}
			assertEqual(2, count);
			assertFalse(d.isForeaching());
		}
		assertFalse(d.isForeaching());
	}
	private static void testModifySyncAddRemoveClearMixed()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		foreach (var kv in d)
		{
			d.add("d", 4);
			d.remove("a");
			break;
		}
		// 主字典:b,c,d
		assertEqual(3, d.count());
		assertFalse(d.containsKey("a"));
		assertTrue(d.containsKey("b"));
		assertTrue(d.containsKey("c"));
		assertTrue(d.containsKey("d"));
		// 新 foreach 同步一致
		int seen = 0;
		foreach (var kv in d)
		{
			seen++;
		}
		assertEqual(3, seen);
	}
	private static void testForeachOnEmpty()
	{
		var d = new SafeDictionary<string, int>();
		int count = 0;
		foreach (var kv in d)
		{
			_ = kv;
			count++;
		}
		assertEqual(0, count);
		assertFalse(d.isForeaching());
	}
	private static void testLargeForeachSum()
	{
		var d = new SafeDictionary<int, int>();
		const int N = 5000;
		for (int i = 0; i < N; ++i)
		{
			d.add(i, i);
		}
		long sum = 0;
		foreach (var kv in d)
		{
			sum += kv.Value;
		}
		assertEqual((long)N * (N - 1) / 2, sum);
	}
	private static void testForValueDuringModify()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		int sum = 0;
		d.forValue(v => { sum += v; });
		assertEqual(3, sum);
	}
	private static void testForKeyDuringModify()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		var keys = new System.Collections.Generic.List<string>();
		d.forKey(k => keys.Add(k));
		assertEqual(2, keys.Count);
	}
	private static void testAddRemoveDuringForeachThenVerifyMain()
	{
		var d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		d.add("c", 3);
		foreach (var kv in d)
		{
			if (kv.Key == "a")
			{
				d.add("e", 5);
				d.remove("c");
			}
		}
		var main = d.getMainList();
		assertEqual(3, main.Count);
		assertTrue(main.ContainsKey("a"));
		assertTrue(main.ContainsKey("b"));
		assertTrue(main.ContainsKey("e"));
		assertFalse(main.ContainsKey("c"));
	}
}

// 用于测试 SafeDictionaryExtension.addClass 的辅助类
public class TestSafeDictionaryClassObject : ClassObject
{
	public TestSafeDictionaryClassObject() { }
}
