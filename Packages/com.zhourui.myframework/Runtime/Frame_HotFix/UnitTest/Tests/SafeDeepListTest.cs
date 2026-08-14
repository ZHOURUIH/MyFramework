using static TestAssert;

// SafeDeepList: 可深度嵌套安全遍历的列表(纯 C# ClassObject, 直接 new 可测)
// 核心特性: 遍历时复制主列表到快照, 遍历期间对主列表的修改不影响本次遍历
public static class SafeDeepListTest
{
	public static void Run()
	{
		testAddCount();
		testContainsRemove();
		testClear();
		testSnapshotUnchangedByAdd();
		testSnapshotUnchangedByRemove();
		testNestedForeach();
		testIsForeaching();
		testDispose();
		testResetProperty();
		testGetMainList();
	}

	// add/count 基本操作
	private static void testAddCount()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		assertEqual(0, list.count(), "初始 count 0");
		list.add(10);
		list.add(20);
		list.add(30);
		assertEqual(3, list.count(), "add 3 个后 count 3");
		assertTrue(list.contains(20), "contains 命中");
		assertFalse(list.contains(99), "contains 未命中");
	}

	// remove
	private static void testContainsRemove()
	{
		SafeDeepList<string> list = new SafeDeepList<string>();
		list.add("a");
		list.add("b");
		list.remove("a");
		assertFalse(list.contains("a"), "remove 后不再包含");
		assertTrue(list.contains("b"), "remove 后保留 b");
		assertEqual(1, list.count(), "remove 后 count 1");
	}

	// clear
	private static void testClear()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		list.add(2);
		list.clear();
		assertEqual(0, list.count(), "clear 后 count 0");
	}

	// 遍历快照: 遍历期间 add 不影响本次遍历
	private static void testSnapshotUnchangedByAdd()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		list.add(2);
		int seen = 0;
		foreach (int v in list)
		{
			// 遍历中向主列表添加新元素(只加一次)
			if (seen == 0)
			{
				list.add(3);
			}
			++seen;
		}
		assertEqual(2, seen, "快照遍历只看到遍历前的 2 个元素");
		assertEqual(3, list.count(), "遍历后主列表包含新元素(count 3)");
		assertTrue(list.contains(3), "遍历后新元素已加入主列表");
	}

	// 遍历快照: 遍历期间 remove 不影响本次遍历, 但主列表已删除
	private static void testSnapshotUnchangedByRemove()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		list.add(2);
		int seen = 0;
		foreach (int v in list)
		{
			list.remove(2);
			++seen;
		}
		assertEqual(2, seen, "快照遍历仍看到 2 个元素(删除不影响快照)");
		assertEqual(1, list.count(), "主列表已删除(count 1)");
	}

	// 嵌套遍历: 遍历中再次遍历(深度快照)
	private static void testNestedForeach()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int outerCount = 0;
		foreach (int vOuter in list)
		{
			int innerCount = 0;
			foreach (int vInner in list)
			{
				++innerCount;
			}
			assertEqual(3, innerCount, "嵌套遍历每次看到 3 个元素");
			++outerCount;
		}
		assertEqual(3, outerCount, "外层遍历 3 次");
	}

	// isForeaching: 遍历中状态
	private static void testIsForeaching()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		assertFalse(list.isForeaching(), "初始未在遍历中");
		foreach (int v in list)
		{
			assertTrue(list.isForeaching(), "遍历中 isForeaching true");
		}
		assertFalse(list.isForeaching(), "遍历结束后 isForeaching false");
	}

	// Dispose: 枚举器显式释放(快照回收)
	private static void testDispose()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		list.add(2);
		using (SafeDeepList<int>.SafeDeepListEnumerator enumerator = list.GetEnumerator())
		{
			int count = 0;
			while (enumerator.MoveNext())
			{
				++count;
			}
			assertEqual(2, count, "枚举器遍历到 2 个元素");
		}
		// Dispose 后再次遍历正常
		using (SafeDeepList<int>.SafeDeepListEnumerator enumerator = list.GetEnumerator())
		{
			assertTrue(enumerator.MoveNext(), "Dispose 后重新遍历正常");
		}
	}

	// resetProperty: 复位清空
	private static void testResetProperty()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		list.add(2);
		list.resetProperty();
		assertEqual(0, list.count(), "resetProperty 后 count 0");
	}

	// getMainList: 实时主列表引用, 修改直接反映到列表
	private static void testGetMainList()
	{
		SafeDeepList<int> list = new SafeDeepList<int>();
		list.add(1);
		var main = list.getMainList();
		assertEqual(1, main.Count, "getMainList 与列表同步");
		// 通过主列表修改直接反映
		main.Add(2);
		assertEqual(2, list.count(), "主列表修改反映到列表 count");
		assertTrue(list.contains(2), "主列表新增元素可见");
	}
}
