using System.Collections.Generic;
using static TestAssert;

public class SafeList0Test
{
	public static void Run()
	{
		testAddAndCount();
		testGetByIndex();
		testRemove();
		testRemoveNonExistent();
		testClear();
		testIsEmpty();
		testIsForeaching();
		testStartForeach();
		testEndForeach();
		testNestedForeach();
		testRemoveDuringForeach();
		testClearDuringForeach();
		testMultipleStartForeachEndForeach();
		testResetProperty();
		testGetMainList();
		testMultipleInstances();
		testEmptyList();
		testAddAndGetMainList();
		testNestedForeachRemoveCompactOuterOnly();
		testRemoveAllDuringForeach();
		testClearEmptyDuringForeach();
		testRemoveAtIndexOutOfBounds();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddAndCount()
	{
		SafeList0<int> list = new();
		assertEqual(0, list.count());
		list.add(10);
		assertEqual(1, list.count());
		list.add(20);
		list.add(30);
		assertEqual(3, list.count());
	}
	private static void testGetByIndex()
	{
		SafeList0<int> list = new();
		list.add(100);
		list.add(200);
		list.add(300);
		assertEqual(100, list.get(0));
		assertEqual(200, list.get(1));
		assertEqual(300, list.get(2));
	}
	private static void testRemove()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		bool removed = list.remove(2);
		assertTrue(removed);
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
	}
	private static void testRemoveNonExistent()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		bool removed = list.remove(999);
		assertFalse(removed);
		assertEqual(2, list.count());
	}
	private static void testClear()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.clear();
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
	}
	private static void testIsEmpty()
	{
		SafeList0<int> list = new();
		assertTrue(list.isEmpty());
		list.add(42);
		assertFalse(list.isEmpty());
		list.clear();
		assertTrue(list.isEmpty());
	}
	private static void testIsForeaching()
	{
		SafeList0<int> list = new();
		assertFalse(list.isForeaching());
		List<int> snapshot = list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach();
		assertFalse(list.isForeaching());
	}
	private static void testStartForeach()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> iter = list.startForeach();
		// startForeach 返回的是主列表引用
		assertEqual(3, iter.Count);
		assertEqual(1, iter[0]);
		list.endForeach();
	}
	private static void testEndForeach()
	{
		SafeList0<int> list = new();
		list.add(10);
		list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach();
		assertFalse(list.isForeaching());
	}
	private static void testNestedForeach()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		List<int> outer = list.startForeach();
		assertTrue(list.isForeaching());
		// 嵌套遍历 - SafeList0 支持
		List<int> inner = list.startForeach();
		assertEqual(2, inner.Count);
		list.endForeach();
		assertTrue(list.isForeaching()); // 外层仍在
		list.endForeach();
		assertFalse(list.isForeaching());
	}
	private static void testRemoveDuringForeach()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> iter = list.startForeach();
		// 遍历中删除 - 标记删除
		bool removed = list.remove(2);
		assertTrue(removed);
		// 遍历列表不受影响
		assertEqual(3, iter.Count);
		// 主列表 count 不变（标记删除）
		assertEqual(3, list.count());
		// get(1) 应该是被标记为 default 的位置
		assertEqual(default(int), list.get(1));
		list.endForeach();
		// endForeach 后 compact，count 应减少
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
	}
	private static void testClearDuringForeach()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> iter = list.startForeach();
		list.clear();
		// clear 在遍历中标记所有元素为 default
		assertEqual(3, iter.Count);
		assertEqual(default(int), list.get(0));
		assertEqual(default(int), list.get(1));
		assertEqual(default(int), list.get(2));
		list.endForeach();
		// endForeach 后 compact
		assertEqual(0, list.count());
	}
	private static void testMultipleStartForeachEndForeach()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		for (int i = 0; i < 5; ++i)
		{
			List<int> iter = list.startForeach();
			assertEqual(2, iter.Count);
			list.endForeach();
		}
		assertEqual(2, list.count());
	}
	private static void testResetProperty()
	{
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.resetProperty();
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
		assertFalse(list.isForeaching());
	}
	private static void testGetMainList()
	{
		SafeList0<int> list = new();
		list.add(10);
		list.add(20);
		List<int> main = list.getMainList();
		assertEqual(2, main.Count);
		assertEqual(10, main[0]);
		assertEqual(20, main[1]);
	}
	private static void testMultipleInstances()
	{
		SafeList0<int> list1 = new();
		SafeList0<int> list2 = new();
		list1.add(1);
		list2.add(10);
		list2.add(20);
		assertEqual(1, list1.count());
		assertEqual(2, list2.count());
	}
	private static void testEmptyList()
	{
		SafeList0<int> list = new();
		assertTrue(list.isEmpty());
		assertEqual(0, list.count());
		assertFalse(list.isForeaching());
		List<int> iter = list.startForeach();
		assertEqual(0, iter.Count);
		list.endForeach();
	}
	private static void testAddAndGetMainList()
	{
		SafeList0<int> list = new();
		list.add(1);
		List<int> main = list.getMainList();
		main.Add(2);
		assertEqual(2, list.count());
	}
	private static void testNestedForeachRemoveCompactOuterOnly()
	{
		// 嵌套遍历中删除，compact 只在最外层 endForeach 触发
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		// 第一层遍历
		List<int> outer = list.startForeach();
		// 第二层遍历
		List<int> inner = list.startForeach();
		// 在嵌套中删除
		list.remove(2);
		assertEqual(3, list.count()); // 标记删除，count 不变
		assertEqual(default(int), list.get(1));
		// 结束内层：depth 1→0，但 needCompact=true，应触发 compact
		list.endForeach();
		// 内层 endForeach 后 depth 回到 1（外层还在），不应 compact
		assertEqual(3, list.count()); // depth>0 时不会 compact
		// 结束外层
		list.endForeach();
		// 外层 endForeach 后 depth=0，应 compact
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
	}
	private static void testRemoveAllDuringForeach()
	{
		// 遍历中删除所有元素
		SafeList0<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> iter = list.startForeach();
		list.remove(1);
		list.remove(2);
		list.remove(3);
		assertEqual(3, list.count());
		assertEqual(default(int), list.get(0));
		assertEqual(default(int), list.get(1));
		assertEqual(default(int), list.get(2));
		list.endForeach();
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
	}
	private static void testClearEmptyDuringForeach()
	{
		// 空列表遍历中清空
		SafeList0<int> list = new();
		List<int> iter = list.startForeach();
		assertEqual(0, iter.Count);
		list.clear();
		assertEqual(0, list.count());
		list.endForeach();
		assertEqual(0, list.count());
	}
	private static void testRemoveAtIndexOutOfBounds()
	{
		// SafeList0 没有 removeAt，此测试验证通过 get 越界行为
		// get 通过 mMainList.get(index) 访问，越界返回 default
		SafeList0<int> list = new();
		list.add(42);
		// 越界 get 返回 default（通过 List.get 扩展方法）
		int val = list.get(999);
		assertEqual(default(int), val);
		assertEqual(1, list.count());
	}
}