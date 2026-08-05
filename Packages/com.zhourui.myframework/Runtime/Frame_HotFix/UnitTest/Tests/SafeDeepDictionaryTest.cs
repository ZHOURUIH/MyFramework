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
		testForeachSnapshotUnchangedByAdd();
		testForeachSnapshotUnchangedByRemove();
		testForeachSnapshotUnchangedByClear();
		testModificationsVisibleOnNextForeach();
		testForeachDispatchLike();
		testSequentialForeachSeesUpdates();
		testForeachOnEmpty();
		testLargeForeachSum();
		testForeachEnumNested();
		testAddRemoveDuringForeachThenVerifyMain();
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
	private static void testClearDuringForeach()
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
	private static void testForeachSnapshotUnchangedByAdd()
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
	private static void testForeachSnapshotUnchangedByRemove()
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
	private static void testForeachSnapshotUnchangedByClear()
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
	private static void testModificationsVisibleOnNextForeach()
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
	private static void testForeachDispatchLike()
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
	private static void testSequentialForeachSeesUpdates()
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
	private static void testForeachOnEmpty()
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
	private static void testLargeForeachSum()
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
	private static void testAddRemoveDuringForeachThenVerifyMain()
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
}