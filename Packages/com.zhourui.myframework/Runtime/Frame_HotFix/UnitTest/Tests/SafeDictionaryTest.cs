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
		testDispose();
		testTwoDictsIndependent();
		testForeachOneModifyOther();
		testAddDuplicateReturnValue();
		testRemoveIfTrueFalse();
		testValueUpdateSequence();
		testClearAndReaddSequence();

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
	

		testAddAndCount_Deep();
		testContainsKey_Deep();
		testTryGetValue_Deep();
		testTryGet();
		testRemove_Deep();
		testClear_Deep();
		testStartForeachReturnsCopy();
		testEndForeach();
		testNestedForeach();
		testGetMainList_Deep();
		testAddDuringForeach();
		testRemoveDuringForeach();
		testClearDuringForeach_Deep();
		testResetProperty_Deep();
		testEnumerator();
		testMultipleInstances();
		testEmptyDictionary();
		testDuplicateKeyThrows();
		testStartForeachReusesTempList();
		testMultipleStartForeachEndForeach();
		testGetMainListModification();
		testAddThenTryGetValue();

		// --- 深度嵌套遍历 ---
		testNestedForeachUsingEnumerator();
		testTripleNestedForeach();
		testQuadrupleNestedForeach();
		testNestedForeachWithModifications();
		testNestedAddDuringForeach();
		testNestedRemoveDuringForeach();
		testNestedClearDuringForeach();
		testNestedForeachOuterInnerSnapshotsIndependent();
		testNestedForeachReuseTempLists();

		// --- 遍历使用场景 ---
		testForeachSnapshotUnchangedByAdd_Deep();
		testForeachSnapshotUnchangedByRemove_Deep();
		testForeachSnapshotUnchangedByClear_Deep();
		testModificationsVisibleOnNextForeach_Deep();
		testForeachDispatchLike_Deep();
		testSequentialForeachSeesUpdates_Deep();
		testForeachOnEmpty_Deep();
		testLargeForeachSum_Deep();
		testForeachEnumNested();
		testAddRemoveDuringForeachThenVerifyMain_Deep();
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


	
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddAndCount_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		assertEqual(0, dict.count());
		dict.add("a", 1);
		assertEqual(1, dict.count());
		dict.add("b", 2);
		dict.add("c", 3);
		assertEqual(3, dict.count());
	}
	private static void testContainsKey_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("key1", 100);
		assertTrue(dict.containsKey("key1"));
		assertFalse(dict.containsKey("key2"));
	}
	private static void testTryGetValue_Deep()
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
	private static void testRemove_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.remove("a");
		assertEqual(1, dict.count());
		assertFalse(dict.containsKey("a"));
		assertTrue(dict.containsKey("b"));
	}
	private static void testClear_Deep()
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
		// foreach 遍历中的快照是主字典的副本:遍历中修改主字典不影响当前快照
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.clear(); // 清空主字典,当前快照不受影响
			dict.add("c", 999);
		}
		assertEqual(3, sum);          // 快照 [a,b] 完整
		assertEqual(1, dict.count()); // 主字典只剩 c
	}
	private static void testEndForeach()
	{
		// foreach 遍历结束即 endForeach,isForeaching 复位
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		foreach (var kv in dict)
		{
			_ = kv;
		}
		assertEqual(1, dict.count());
	}
	private static void testNestedForeach()
	{
		// 深嵌套字典支持 foreach 内再 foreach
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int count = 0;
		foreach (var outer in dict)
		{
			foreach (var inner in dict)
			{
				_ = inner;
				count++;
			}
		}
		assertEqual(4, count); // 2 外层 × 2 内层
	}
	private static void testGetMainList_Deep()
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
		// 遍历中添加元素:当前快照不变,主字典实时新增
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.add("b", 2);
			dict.add("c", 3);
		}
		assertEqual(1, sum);          // 快照只有 [a]
		assertEqual(3, dict.count()); // 主字典 [a,b,c]
	}
	private static void testRemoveDuringForeach()
	{
		// 遍历中删除元素:当前快照不受影响,主字典已删除
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.remove("a");
		}
		assertEqual(3, sum); // 快照 [a,b]
		assertEqual(1, dict.count());
		assertFalse(dict.containsKey("a"));
	}
	private static void testClearDuringForeach_Deep()
	{
		// 遍历中清空:当前快照不受影响,主字典已清空
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.clear();
		}
		assertEqual(3, sum); // 快照 [a,b]
		assertEqual(0, dict.count());
	}
	private static void testResetProperty_Deep()
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
		int count = 0;
		foreach (var kv in dict)
		{
			_ = kv;
			count++;
		}
		assertEqual(0, count);
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
		// 多次 start/end(foreach 进入/退出)后字典内容与状态保持一致
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		for (int i = 0; i < 5; ++i)
		{
			int sum = 0;
			foreach (var kv in dict)
			{
				sum += kv.Value;
			}
			assertEqual(1, sum);
		}
		assertEqual(1, dict.count());
	}
	private static void testMultipleStartForeachEndForeach()
	{
		// 连续多次 foreach 均正常遍历且状态复位
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		for (int i = 0; i < 5; ++i)
		{
			int count = 0;
			foreach (var kv in dict)
			{
				_ = kv;
				count++;
			}
			assertEqual(2, count);
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
	//------------------------------------------------------------------------------------------------------------------------------
	// 深度嵌套遍历
	private static void testNestedForeachUsingEnumerator()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int count = 0;
		foreach (var outer in dict)
		{
			foreach (var inner in dict)
			{
				count++;
			}
		}
		assertEqual(4, count);
	}
	private static void testTripleNestedForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int count = 0;
		foreach (var a in dict)
		{
			foreach (var b in dict)
			{
				foreach (var c in dict)
				{
					count++;
				}
			}
		}
		assertEqual(27, count);
	}
	private static void testQuadrupleNestedForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int count = 0;
		foreach (var a in dict)
		{
			foreach (var b in dict)
			{
				foreach (var c in dict)
				{
					foreach (var d in dict)
					{
						count++;
					}
				}
			}
		}
		assertEqual(16, count);
	}
	private static void testNestedForeachWithModifications()
	{
		// 嵌套遍历中新增,外层快照固定,内层每次取当前快照
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int sum = 0;
		foreach (var outer in dict) // 外层快照 [a,b]
		{
			foreach (var inner in dict) // 内层每次重新生成
			{
				sum += inner.Value;
			}
			dict.add("x" + outer.Key, 100); // 遍历中新增
		}
		// 第一次内层 [a,b] sum=3,第二次内层 [a,b,xa] sum=103
		assertEqual(106, sum);
		assertEqual(4, dict.count());
	}
	private static void testNestedAddDuringForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		foreach (var kv in dict)
		{
			dict.add("new" + kv.Key, 10);
		}
		assertEqual(4, dict.count());
	}
	private static void testNestedRemoveDuringForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.remove("a");
		}
		assertEqual(6, sum);
		assertEqual(2, dict.count());
		assertFalse(dict.containsKey("a"));
	}
	private static void testNestedClearDuringForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.clear();
		}
		assertEqual(6, sum);
		assertEqual(0, dict.count());
	}
	private static void testNestedForeachOuterInnerSnapshotsIndependent()
	{
		// 内外层 foreach 快照相互独立:内层遍历中的修改不影响外层本次遍历
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int outerCount = 0;
		foreach (var outer in dict)
		{
			foreach (var inner in dict)
			{
				_ = inner;
			}
			outerCount++;
		}
		assertEqual(3, outerCount);
		assertEqual(3, dict.count()); // 主字典不变
	}
	private static void testNestedForeachReuseTempLists()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		foreach (var a in dict)
		{
			foreach (var b in dict)
			{
				_ = a.Value + b.Value;
			}
		}
		int count = 0;
		foreach (var kv in dict)
		{
			_ = kv;
			count++;
		}
		assertEqual(3, count);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 遍历使用场景
	private static void testForeachSnapshotUnchangedByAdd_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.add("n" + kv.Key, 100);
		}
		assertEqual(3, sum);
		assertEqual(4, dict.count());
	}
	private static void testForeachSnapshotUnchangedByRemove_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.remove("a");
			dict.remove("b");
		}
		assertEqual(6, sum);
		assertEqual(1, dict.count());
	}
	private static void testForeachSnapshotUnchangedByClear_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			dict.clear();
		}
		assertEqual(6, sum);
		assertEqual(0, dict.count());
	}
	private static void testModificationsVisibleOnNextForeach_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		foreach (var kv in dict)
		{
			if (kv.Key == "a")
			{
				dict.add("d", 4);
				dict.remove("b");
			}
		}
		var main = dict.getMainList();
		assertEqual(2, main.Count);
		assertTrue(main.ContainsKey("a"));
		assertTrue(main.ContainsKey("d"));
		assertFalse(main.ContainsKey("b"));
		int seen = 0;
		foreach (var kv in dict)
		{
			_ = kv;
			seen++;
		}
		assertEqual(2, seen);
	}
	private static void testForeachDispatchLike_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		for (int i = 0; i < 5; ++i)
		{
			dict.add("k" + i, i);
		}
		int sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
			if (kv.Key == "k2")
			{
				dict.remove("k2");
			}
		}
		assertEqual(10, sum);
		assertEqual(4, dict.count());
		assertFalse(dict.containsKey("k2"));
	}
	private static void testSequentialForeachSeesUpdates_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		foreach (var kv in dict)
		{
			_ = kv;
		}
		dict.add("b", 2);
		dict.add("c", 3);
		int count = 0;
		foreach (var kv in dict)
		{
			_ = kv;
			count++;
		}
		assertEqual(3, count);
	}
	private static void testForeachOnEmpty_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		int count = 0;
		foreach (var kv in dict)
		{
			_ = kv;
			count++;
		}
		assertEqual(0, count);
	}
	private static void testLargeForeachSum_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		const int N = 5000;
		for (int i = 0; i < N; ++i)
		{
			dict.add("k" + i, i);
		}
		long sum = 0;
		foreach (var kv in dict)
		{
			sum += kv.Value;
		}
		assertEqual((long)N * (N - 1) / 2, sum);
	}
	private static void testForeachEnumNested()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		int count = 0;
		foreach (var outer in dict)
		{
			foreach (var inner in dict)
			{
				count++;
			}
			if (outer.Key == "a")
			{
				dict.remove("b");
			}
		}
		// 外层快照 [a,b,c] 固定 3 次;每次内层都在进入时重新快照当前主表:
		// 第一次内层 [a,b,c]→3;移除 b 后第二次内层 [a,c]→2;第三次内层 [a,c]→2
		// 合计 3+2+2=7
		assertEqual(7, count);
	}
	private static void testAddRemoveDuringForeachThenVerifyMain_Deep()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.add("c", 3);
		foreach (var kv in dict)
		{
			if (kv.Key == "a")
			{
				dict.add("e", 5);
				dict.remove("c");
			}
		}
		var main = dict.getMainList();
		assertEqual(3, main.Count);
		assertTrue(main.ContainsKey("a"));
		assertTrue(main.ContainsKey("b"));
		assertTrue(main.ContainsKey("e"));
		assertFalse(main.ContainsKey("c"));
	}
	// Dispose: 枚举器显式释放(using 作用域结束自动调用 endForeach 结束安全遍历)
	private static void testDispose()
	{
		SafeDictionary<string, int> d = new();
		d.add("x", 10);
		d.add("y", 20);
		using (SafeDictionary<string, int>.SafeDictionaryEnumerator enumerator = d.GetEnumerator())
		{
			int count = 0;
			while (enumerator.MoveNext())
			{
				++count;
			}
			assertEqual(2, count, "枚举器遍历到 2 个元素");
		}
		// Dispose 后可再次遍历(安全遍历状态已复位)
		using (SafeDictionary<string, int>.SafeDictionaryEnumerator enumerator = d.GetEnumerator())
		{
			assertTrue(enumerator.MoveNext(), "Dispose 后重新遍历正常");
		}
	}

	// ── 多实例组合场景 ──────────────────────────────────────────────
	// 两个字典独立操作互不影响
	private static void testTwoDictsIndependent()
	{
		SafeDictionary<string, int> dictA = new SafeDictionary<string, int>();
		SafeDictionary<string, int> dictB = new SafeDictionary<string, int>();
		dictA.add("a", 1);
		dictA.add("b", 2);
		dictB.add("x", 100);
		assertEqual(2, dictA.count(), "dictA count 2");
		assertEqual(1, dictB.count(), "dictB count 1");
		dictA.clear();
		assertEqual(0, dictA.count(), "clear dictA 不影响 dictB");
		assertEqual(1, dictB.count(), "dictB 保持 1");
		assertEqual(100, dictB.get("x"), "dictB 值保留");
	}

	// 遍历 dictA 时修改 dictB: 互不干扰
	private static void testForeachOneModifyOther()
	{
		SafeDictionary<string, int> dictA = new SafeDictionary<string, int>();
		SafeDictionary<string, int> dictB = new SafeDictionary<string, int>();
		dictA.add("k1", 1);
		dictA.add("k2", 2);
		dictB.add("base", 0);
		int seen = 0;
		foreach (var kv in dictA)
		{
			dictB.add("copy" + kv.Key, kv.Value);
			++seen;
		}
		assertEqual(2, seen, "dictA 遍历到 2 个键");
		assertEqual(3, dictB.count(), "dictB 被追加 2 个(count 3)");
		assertTrue(dictB.containsKey("copyk1"), "dictB 包含追加键");
	}

	// add 重复 key 返回 false, 值不覆盖
	private static void testAddDuplicateReturnValue()
	{
		SafeDictionary<string, int> d = new SafeDictionary<string, int>();
		assertTrue(d.add("a", 1), "首次 add 返回 true");
		bool dup = d.add("a", 99);
		assertFalse(dup, "重复 add 返回 false");
		assertEqual(1, d.count(), "重复 add 后 count 不变");
		assertEqual(1, d.get("a"), "重复 add 值不覆盖(保持 1)");
	}

	// removeIf: 条件为真才删除
	private static void testRemoveIfTrueFalse()
	{
		SafeDictionary<string, int> d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		assertTrue(d.removeIf("a", true), "removeIf(true) 删除成功返回 true");
		assertFalse(d.containsKey("a"), "removeIf(true) 后 a 已删除");
		assertFalse(d.removeIf("b", false), "removeIf(false) 不删除返回 false");
		assertTrue(d.containsKey("b"), "removeIf(false) 后 b 保留");
		assertEqual(1, d.count(), "count 1");
	}

	// 组合序列: add → get → removeIf → containsKey 全链路
	private static void testValueUpdateSequence()
	{
		SafeDictionary<string, int> d = new SafeDictionary<string, int>();
		d.add("hp", 100);
		d.add("mp", 50);
		assertEqual(100, d.get("hp"), "get hp=100");
		assertEqual(50, d.get("mp"), "get mp=50");
		// 组合操作序列
		d.removeIf("mp", true);
		d.add("atk", 30);
		assertFalse(d.containsKey("mp"), "序列后 mp 已删除");
		assertTrue(d.containsKey("atk"), "序列后 atk 已添加");
		assertEqual(30, d.get("atk"), "序列后 atk=30");
		assertEqual(2, d.count(), "序列后 count 2");
	}

	// clear 后重新 add: 字典可复用
	private static void testClearAndReaddSequence()
	{
		SafeDictionary<string, int> d = new SafeDictionary<string, int>();
		d.add("a", 1);
		d.add("b", 2);
		d.clear();
		assertEqual(0, d.count(), "clear 后 count 0");
		// 重新填充
		d.add("x", 10);
		d.add("y", 20);
		d.add("z", 30);
		assertEqual(3, d.count(), "重新 add 后 count 3");
		assertEqual(20, d.get("y"), "重新 add 后 y=20");
	}
}

// 用于测试 SafeDictionaryExtension.addClass 的辅助类
public class TestSafeDictionaryClassObject : ClassObject
{
	public TestSafeDictionaryClassObject() { }
}
