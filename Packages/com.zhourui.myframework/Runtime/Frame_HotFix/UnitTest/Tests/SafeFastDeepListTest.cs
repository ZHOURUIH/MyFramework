using static TestAssert;

// SafeFastDeepList: 非线程安全深遍历列表(纯 C# ClassObject, 直接 new 可测)
// 核心特性: 遍历中删除/清空不立即生效(标记 default + mNeedCompact), endForeach 时 compact 真正移除
public static class SafeFastDeepListTest
{
	public static void Run()
	{
		testAddGetCount();
		testContains();
		testRemove();
		testRemoveDuringForeachCompact();
		testClearDuringForeachCompact();
		testStartEndForeachDepth();
		testNestedForeach();
		testIsEmpty();
		testResetProperty();
		testGetMainList();
	}

	// add/get/count 基本操作
	private static void testAddGetCount()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		assertEqual(0, list.count(), "初始 count 0");
		list.add(10);
		list.add(20);
		list.add(30);
		assertEqual(3, list.count(), "add 3 个后 count 3");
		assertEqual(10, list.get(0), "get(0) 首元素");
		assertEqual(30, list.get(2), "get(2) 末元素");
	}

	// contains 判断
	private static void testContains()
	{
		SafeFastDeepList<string> list = new SafeFastDeepList<string>();
		list.add("a");
		list.add("b");
		assertTrue(list.contains("a"), "contains 命中");
		assertFalse(list.contains("c"), "contains 未命中");
	}

	// remove: 非遍历中直接删除
	private static void testRemove()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		assertTrue(list.remove(2), "remove 命中返回 true");
		assertFalse(list.contains(2), "remove 后不再包含");
		assertEqual(2, list.count(), "remove 后 count 2");
		assertFalse(list.remove(99), "remove 未命中返回 false");
	}

	// 遍历中 remove: 标记删除, endForeach 后 compact 真正移除
	private static void testRemoveDuringForeachCompact()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		list.add(4);
		var main = list.startForeach();
		// 遍历中删除 2
		assertTrue(list.remove(2), "遍历中 remove 返回 true");
		// 延迟生效: count 不变, 但该位置被标记为 default
		assertEqual(4, list.count(), "遍历中删除后 count 未立即变化");
		assertFalse(list.contains(2), "遍历中删除后 contains 已失效(值被标记)");
		list.endForeach();
		// compact 后真正移除
		assertEqual(3, list.count(), "endForeach 后 compact 移除标记元素");
		assertTrue(list.contains(1), "compact 后保留 1");
		assertTrue(list.contains(3), "compact 后保留 3");
		assertFalse(list.contains(2), "compact 后删除生效");
	}

	// 遍历中 clear: 全部标记, endForeach 后清空
	private static void testClearDuringForeachCompact()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		var main = list.startForeach();
		list.clear();
		assertEqual(3, list.count(), "遍历中 clear 后 count 未立即变化");
		list.endForeach();
		assertEqual(0, list.count(), "endForeach 后清空生效");
		assertTrue(list.isEmpty(), "clear+compact 后为空");
	}

	// startForeach/endForeach 深度状态
	private static void testStartEndForeachDepth()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(5);
		assertFalse(list.isForeaching(), "初始未在遍历中");
		var main = list.startForeach();
		assertTrue(list.isForeaching(), "startForeach 后在遍历中");
		assertEqual(1, main.Count, "startForeach 返回主列表");
		list.endForeach();
		assertFalse(list.isForeaching(), "endForeach 后不在遍历中");
	}

	// 嵌套 startForeach: 深度计数, 外层 endForeach 才触发 compact
	private static void testNestedForeach()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(1);
		list.add(2);
		list.startForeach();
		list.startForeach();
		assertTrue(list.isForeaching(), "嵌套遍历仍在遍历中");
		// 内层删除标记, 内层 endForeach 深度未归零不 compact
		list.remove(1);
		list.endForeach();
		assertEqual(2, list.count(), "内层 endForeach 深度未归零, 删除未生效");
		list.endForeach();
		assertEqual(1, list.count(), "外层 endForeach 深度归零, compact 生效");
		assertFalse(list.contains(1), "嵌套删除最终生效");
	}

	// isEmpty
	private static void testIsEmpty()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		assertTrue(list.isEmpty(), "初始为空");
		list.add(1);
		assertFalse(list.isEmpty(), "add 后非空");
		list.clear();
		assertTrue(list.isEmpty(), "clear 后为空");
	}

	// resetProperty: 复位清空
	private static void testResetProperty()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(1);
		list.add(2);
		list.resetProperty();
		assertEqual(0, list.count(), "resetProperty 后 count 0");
		assertTrue(list.isEmpty(), "resetProperty 后为空");
	}

	// getMainList: 实时主列表引用, 修改直接反映
	private static void testGetMainList()
	{
		SafeFastDeepList<int> list = new SafeFastDeepList<int>();
		list.add(1);
		var main = list.getMainList();
		assertEqual(1, main.Count, "getMainList 与列表同步");
		main.Add(2);
		assertEqual(2, list.count(), "主列表修改反映到列表 count");
		assertTrue(list.contains(2), "主列表新增元素可见");
	}
}
