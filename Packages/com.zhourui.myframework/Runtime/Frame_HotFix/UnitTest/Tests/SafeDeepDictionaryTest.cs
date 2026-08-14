using static TestAssert;

// SafeDeepDictionary: 可深度嵌套安全遍历的字典(纯 C# ClassObject, 直接 new 可测)
// 核心特性: 遍历时复制主列表到快照, 遍历期间对主列表的修改不影响本次遍历
public static class SafeDeepDictionaryTest
{
	public static void Run()
	{
		testAddCount();
		testContainsKey();
		testTryGetValue();
		testTryGetDefault();
		testRemove();
		testClear();
		testSnapshotUnchangedByAdd();
		testSnapshotUnchangedByRemove();
		testNestedForeach();
		testDispose();
		testResetProperty();
		testGetMainList();
	}

	// add/count 基本操作
	private static void testAddCount()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		assertEqual(0, dict.count(), "初始 count 0");
		dict.add("a", 1);
		dict.add("b", 2);
		assertEqual(2, dict.count(), "add 2 个后 count 2");
	}

	// containsKey
	private static void testContainsKey()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("x", 10);
		assertTrue(dict.containsKey("x"), "containsKey 命中");
		assertFalse(dict.containsKey("y"), "containsKey 未命中");
	}

	// tryGetValue: 命中 out 值, 未命中 false
	private static void testTryGetValue()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("k", 42);
		assertTrue(dict.tryGetValue("k", out int value), "tryGetValue 命中返回 true");
		assertEqual(42, value, "tryGetValue out 值正确");
		assertFalse(dict.tryGetValue("missing", out int noValue), "tryGetValue 未命中返回 false");
	}

	// tryGet: 存在返回值, 不存在返回默认值
	private static void testTryGetDefault()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 7);
		assertEqual(7, dict.tryGet("a"), "tryGet 命中返回值");
		assertEqual(0, dict.tryGet("zz"), "tryGet 未命中返回默认 0");
	}

	// remove
	private static void testRemove()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.remove("a");
		assertFalse(dict.containsKey("a"), "remove 后不再包含");
		assertEqual(1, dict.count(), "remove 后 count 1");
	}

	// clear
	private static void testClear()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.clear();
		assertEqual(0, dict.count(), "clear 后 count 0");
	}

	// 遍历快照: 遍历期间 add 不影响本次遍历(快照复制)
	private static void testSnapshotUnchangedByAdd()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 1);
		dict.add("b", 2);
		int seen = 0;
		foreach (var kv in dict)
		{
			// 遍历中向主列表添加新键(只加一次, 避免重复键 Add 抛异常)
			if (seen == 0)
			{
				dict.add("c", 3);
			}
			++seen;
		}
		assertEqual(2, seen, "快照遍历只看到遍历前的 2 个键");
		assertEqual(3, dict.count(), "遍历后主列表包含新键(count 3)");
		assertTrue(dict.containsKey("c"), "遍历后新键已加入主列表");
	}

	// 遍历快照: 遍历期间 remove 不影响本次遍历, 但主列表已删除
	private static void testSnapshotUnchangedByRemove()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 1);
		dict.add("b", 2);
		int seen = 0;
		foreach (var kv in dict)
		{
			dict.remove("b");
			++seen;
		}
		assertEqual(2, seen, "快照遍历仍看到 2 个键(删除不影响快照)");
		assertEqual(1, dict.count(), "主列表已删除(count 1)");
		assertFalse(dict.containsKey("b"), "主列表不再包含 b");
	}

	// 嵌套遍历: 遍历中再次遍历(深度快照)
	private static void testNestedForeach()
	{
		SafeDeepDictionary<int, int> dict = new SafeDeepDictionary<int, int>();
		dict.add(1, 10);
		dict.add(2, 20);
		dict.add(3, 30);
		int outerCount = 0;
		foreach (var kvOuter in dict)
		{
			int innerCount = 0;
			foreach (var kvInner in dict)
			{
				++innerCount;
			}
			assertEqual(3, innerCount, "嵌套遍历每次看到 3 个键");
			++outerCount;
		}
		assertEqual(3, outerCount, "外层遍历 3 次");
	}

	// Dispose: 枚举器显式释放(快照回收)
	private static void testDispose()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("x", 1);
		using (SafeDeepDictionary<string, int>.SafeDeepDictionaryEnumerator enumerator = dict.GetEnumerator())
		{
			int count = 0;
			while (enumerator.MoveNext())
			{
				++count;
			}
			assertEqual(1, count, "枚举器遍历到 1 个元素");
		}
		// Dispose 后再次遍历正常
		using (SafeDeepDictionary<string, int>.SafeDeepDictionaryEnumerator enumerator = dict.GetEnumerator())
		{
			assertTrue(enumerator.MoveNext(), "Dispose 后重新遍历正常");
		}
	}

	// resetProperty: 复位清空
	private static void testResetProperty()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 1);
		dict.add("b", 2);
		dict.resetProperty();
		assertEqual(0, dict.count(), "resetProperty 后 count 0");
	}

	// getMainList: 实时主列表引用, 修改直接反映到字典
	private static void testGetMainList()
	{
		SafeDeepDictionary<string, int> dict = new SafeDeepDictionary<string, int>();
		dict.add("a", 1);
		var main = dict.getMainList();
		assertEqual(1, main.Count, "getMainList 与字典同步");
		// 通过主列表修改直接反映
		main["b"] = 2;
		assertEqual(2, dict.count(), "主列表修改反映到字典 count");
		assertTrue(dict.containsKey("b"), "主列表新增键可见");
	}
}
