using System.Collections.Generic;
using static TestAssert;

public class SafeDeepListTest
{
	public static void Run()
	{
		testAddAndCount();
		testAddAndContains();
		testRemove();
		testClear();
		testGetMainList();
		testStartForeachReturnsCopy();
		testStartForeachReusesTempList();
		testEndForeach();
		testNestedForeach();
		testIsForeaching();
		testAddDuringForeach();
		testRemoveDuringForeach();
		testClearDuringForeach();
		testResetProperty();
		testEnumerator();
		testMultipleInstances();
		testEmptyList();
		testAddDuplicateValues();
		testStartForeachEmptyList();
		testMultipleStartForeachEndForeach();
		testGetMainListModification();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddAndCount()
	{
		SafeDeepList<int> list = new();
		assertEqual(0, list.count());
		list.add(10);
		assertEqual(1, list.count());
		list.add(20);
		list.add(30);
		assertEqual(3, list.count());
	}
	private static void testAddAndContains()
	{
		SafeDeepList<string> list = new();
		list.add("hello");
		list.add("world");
		assertTrue(list.contains("hello"));
		assertTrue(list.contains("world"));
		assertFalse(list.contains("nonexistent"));
	}
	private static void testRemove()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.remove(2);
		assertEqual(2, list.count());
		assertTrue(list.contains(1));
		assertFalse(list.contains(2));
		assertTrue(list.contains(3));
	}
	private static void testClear()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.clear();
		assertEqual(0, list.count());
		assertFalse(list.contains(1));
	}
	private static void testGetMainList()
	{
		SafeDeepList<int> list = new();
		list.add(100);
		list.add(200);
		List<int> main = list.getMainList();
		assertEqual(2, main.Count);
		assertEqual(100, main[0]);
		assertEqual(200, main[1]);
		// 修改主列表应反映到 SafeDeepList
		main.Add(300);
		assertEqual(3, list.count());
	}
	private static void testStartForeachReturnsCopy()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> snapshot = list.startForeach();
		assertEqual(3, snapshot.Count);
		// 修改 snapshot 不应影响原列表
		snapshot.Clear();
		snapshot.Add(999);
		assertEqual(3, list.count());
		assertEqual(1, list.getMainList()[0]);
		list.endForeach(snapshot);
	}
	private static void testStartForeachReusesTempList()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		List<int> snapshot1 = list.startForeach();
		list.endForeach(snapshot1);
		// 第二次 startForeach 应复用已回收的列表
		List<int> snapshot2 = list.startForeach();
		// snapshot2 可能是之前回收的 snapshot1
		assertEqual(2, snapshot2.Count);
		list.endForeach(snapshot2);
	}
	private static void testEndForeach()
	{
		SafeDeepList<int> list = new();
		list.add(10);
		list.add(20);
		List<int> snapshot = list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach(snapshot);
		assertFalse(list.isForeaching());
	}
	private static void testNestedForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> outer = list.startForeach();
		assertTrue(list.isForeaching());
		// 嵌套遍历
		List<int> inner = list.startForeach();
		assertEqual(3, inner.Count);
		list.endForeach(inner);
		assertTrue(list.isForeaching()); // 外层仍在遍历
		list.endForeach(outer);
		assertFalse(list.isForeaching());
	}
	private static void testIsForeaching()
	{
		SafeDeepList<int> list = new();
		assertFalse(list.isForeaching());
		List<int> snapshot = list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach(snapshot);
		assertFalse(list.isForeaching());
	}
	private static void testAddDuringForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		List<int> snapshot = list.startForeach();
		// 遍历中添加元素
		list.add(3);
		// 快照应只有原始数据
		assertEqual(2, snapshot.Count);
		// 主列表包含新元素
		assertEqual(3, list.count());
		list.endForeach(snapshot);
	}
	private static void testRemoveDuringForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> snapshot = list.startForeach();
		// 遍历中删除元素
		list.remove(2);
		// 快照不受影响
		assertEqual(3, snapshot.Count);
		// 主列表已删除
		assertEqual(2, list.count());
		assertFalse(list.contains(2));
		list.endForeach(snapshot);
	}
	private static void testClearDuringForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		List<int> snapshot = list.startForeach();
		list.clear();
		assertEqual(2, snapshot.Count); // 快照不受影响
		assertEqual(0, list.count());
		list.endForeach(snapshot);
	}
	private static void testResetProperty()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.resetProperty();
		assertEqual(0, list.count());
		assertFalse(list.isForeaching());
	}
	private static void testEnumerator()
	{
		SafeDeepList<int> list = new();
		list.add(10);
		list.add(20);
		list.add(30);
		int sum = 0;
		var enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current;
		}
		assertEqual(60, sum);
	}
	private static void testMultipleInstances()
	{
		SafeDeepList<int> list1 = new();
		SafeDeepList<int> list2 = new();
		list1.add(1);
		list1.add(2);
		list2.add(10);
		list2.add(20);
		list2.add(30);
		assertEqual(2, list1.count());
		assertEqual(3, list2.count());
		assertTrue(list1.contains(1));
		assertTrue(list2.contains(20));
		assertFalse(list1.contains(10));
	}
	private static void testEmptyList()
	{
		SafeDeepList<int> list = new();
		assertEqual(0, list.count());
		assertFalse(list.contains(0));
		assertFalse(list.isForeaching());
		List<int> snapshot = list.startForeach();
		assertEqual(0, snapshot.Count);
		list.endForeach(snapshot);
	}
	private static void testAddDuplicateValues()
	{
		SafeDeepList<int> list = new();
		list.add(5);
		list.add(5); // List 允许重复
		assertEqual(2, list.count());
	}
	private static void testStartForeachEmptyList()
	{
		SafeDeepList<int> list = new();
		List<int> snapshot = list.startForeach();
		assertEqual(0, snapshot.Count);
		list.endForeach(snapshot);
		// 再次 start/end
		snapshot = list.startForeach();
		assertEqual(0, snapshot.Count);
		list.endForeach(snapshot);
	}
	private static void testMultipleStartForeachEndForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		for (int i = 0; i < 5; ++i)
		{
			List<int> snapshot = list.startForeach();
			assertEqual(3, snapshot.Count);
			list.endForeach(snapshot);
		}
		// 多次 start/end 后列表内容不变
		assertEqual(3, list.count());
	}
	private static void testGetMainListModification()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		List<int> main = list.getMainList();
		main.Add(2);
		main.Add(3);
		assertEqual(3, list.count());
		assertTrue(list.contains(2));
		assertTrue(list.contains(3));
	}
}