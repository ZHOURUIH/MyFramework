using System;
using System.Collections.Generic;
using static TestAssert;

// SafeList<T> 穷举测试（覆盖所有公开 API 和关键分支）
public static class SafeListTest
{
	public static void Run()
	{
		// --- 基础增删改查 ---
		testAddRemove();
		testContains();
		testCount();
		testClear();
		testGet();
		testRemoveNonExistent();
		testRemoveAt();
		testRemoveAtIndexOutOfBounds();
		testRemoveAtIndexNegative();
		testRemoveAtZero();

		// --- 遍历相关 ---
		testFor();
		testFind();
		testIsForeaching();
		testGetEnumerator();
		testStartForeachEndForeach();
		testClearDuringForeach();
		testStartForeachModifySync();

		// --- 主列表操作 ---
		testGetMainList();

		// --- 条件操作 ---
		testAddUnique();
		testAddUniqueAlreadyExists();
		testAddNotNull();
		testAddNotNullNullValue();
		testAddOrRemove_Add();
		testAddOrRemove_Remove();
		testAddIf_True();
		testAddIf_False();
		testRemoveIf_True();
		testRemoveIf_False();

		// --- 批量操作 ---
		testAddRangeList();
		testAddRangeHashSet();
		testAddRangeEmptyList();
		testSetRangeList();
		testSetRangeHashSet();
		testSetRangeOverwritesExisting();

		// --- 重置 ---
		testResetProperty();

		// --- 扩展方法 ---
		testAddClass();
	}

	//==================================================================
	// 基础增删改查
	//==================================================================
	private static void testAddRemove()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		assertEqual(3, list.count());
		list.remove(2);
		assertEqual(2, list.count());
		assertTrue(list.contains(1));
		assertFalse(list.contains(2));
		assertTrue(list.contains(3));
	}

	private static void testContains()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		assertTrue(list.contains(10));
		assertFalse(list.contains(999));
	}

	private static void testCount()
	{
		var list = new SafeList<int>();
		assertEqual(0, list.count());
		list.add(1);
		assertEqual(1, list.count());
		list.add(2);
		assertEqual(2, list.count());
		list.remove(1);
		assertEqual(1, list.count());
	}

	private static void testClear()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		list.clear();
		assertEqual(0, list.count());
		// clear 后可以继续添加
		list.add(100);
		assertEqual(1, list.count());
		assertEqual(100, list.get(0));
	}

	private static void testGet()
	{
		var list = new SafeList<int>();
		list.add(100);
		list.add(200);
		list.add(300);
		assertEqual(100, list.get(0));
		assertEqual(200, list.get(1));
		assertEqual(300, list.get(2));
	}

	private static void testRemoveNonExistent()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		bool removed = list.remove(999);
		assertFalse(removed);
		assertEqual(2, list.count());
	}

	private static void testRemoveAt()
	{
		var list = new SafeList<int>();
		list.add(0);
		list.add(1);
		list.add(2);
		int removed = list.removeAt(1);
		assertEqual(1, removed);
		assertEqual(2, list.count());
		assertFalse(list.contains(1));
		assertTrue(list.contains(0));
		assertTrue(list.contains(2));
	}

	private static void testRemoveAtIndexOutOfBounds()
	{
		var list = new SafeList<int>();
		list.add(1);
		// removeAt 越界返回 default
		int removed = list.removeAt(5);
		assertEqual(default(int), removed);
		assertEqual(1, list.count());
	}

	private static void testRemoveAtIndexNegative()
	{
		var list = new SafeList<int>();
		list.add(1);
		int removed = list.removeAt(-1);
		assertEqual(default(int), removed);
		assertEqual(1, list.count());
	}

	private static void testRemoveAtZero()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		int removed = list.removeAt(0);
		assertEqual(10, removed);
		assertEqual(1, list.count());
		assertEqual(20, list.get(0));
	}

	//==================================================================
	// 遍历相关
	//==================================================================
	private static void testFor()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		list.For(v => sum += v);
		assertEqual(6, sum);
	}

	private static void testFind()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		list.add(30);
		int f = list.find(v => v > 15);
		assertEqual(20, f);
		// find 不存在的
		int nf = list.find(v => v > 999);
		assertEqual(default(int), nf);
	}

	private static void testIsForeaching()
	{
		var list = new SafeList<int>();
		list.add(1);
		assertFalse(list.isForeaching());
		var iter = list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach();
		assertFalse(list.isForeaching());
	}

	private static void testGetEnumerator()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
		}
		assertEqual(6, sum);
	}

	private static void testStartForeachEndForeach()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		list.add(30);
		List<int> iter = list.startForeach();
		assertNotNull(iter);
		assertEqual(3, iter.Count);
		assertEqual(10, iter[0]);
		assertEqual(20, iter[1]);
		assertEqual(30, iter[2]);
		list.endForeach();
		assertFalse(list.isForeaching());
	}

	private static void testClearDuringForeach()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> iter = list.startForeach();
		assertEqual(3, iter.Count);
		// 遍历中清空
		list.clear();
		// 清空后遍历列表仍然可用（因为 startForeach 返回了快照）
		assertEqual(3, iter.Count);
		// 但主列表已清空
		assertEqual(0, list.count());
		list.endForeach();
	}

	private static void testStartForeachModifySync()
	{
		// 测试 startForeach 将 modifyList 同步到 updateList
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		// 先做一次遍历，让 modifyList 清空
		{
			List<int> iter = list.startForeach();
			assertEqual(3, iter.Count);
			list.endForeach();
		}
		// 修改后再次遍历，验证同步
		list.add(4);
		list.remove(1);
		{
			List<int> iter = list.startForeach();
			assertEqual(3, iter.Count); // 1 removed, 4 added = still 3
			assertTrue(iter.Contains(2));
			assertTrue(iter.Contains(3));
			assertTrue(iter.Contains(4));
			assertFalse(iter.Contains(1));
			list.endForeach();
		}
	}

	//==================================================================
	// 主列表操作
	//==================================================================
	private static void testGetMainList()
	{
		var list = new SafeList<int>();
		list.add(42);
		list.add(99);
		var main = list.getMainList();
		assertNotNull(main);
		assertEqual(2, main.Count);
		assertEqual(42, main[0]);
		assertEqual(99, main[1]);
	}

	//==================================================================
	// 条件操作
	//==================================================================
	private static void testAddUnique()
	{
		var list = new SafeList<int>();
		list.addUnique(1);
		list.addUnique(2);
		list.addUnique(1);
		assertEqual(2, list.count());
		assertTrue(list.contains(1));
		assertTrue(list.contains(2));
	}

	private static void testAddUniqueAlreadyExists()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		// addUnique 对已存在元素返回 false
		bool added = list.addUnique(1);
		assertFalse(added);
		assertEqual(2, list.count());
	}

	private static void testAddNotNull()
	{
		var list = new SafeList<string>();
		list.addNotNull("a");
		list.addNotNull(null);
		list.addNotNull("b");
		assertEqual(2, list.count());
		assertTrue(list.contains("a"));
		assertTrue(list.contains("b"));
		assertFalse(list.contains(null));
	}

	private static void testAddNotNullNullValue()
	{
		var list = new SafeList<string>();
		list.addNotNull(null);
		assertEqual(0, list.count());
	}

	private static void testAddOrRemove_Add()
	{
		var list = new SafeList<int>();
		bool result = list.addOrRemove(42, true);
		assertTrue(result);
		assertEqual(1, list.count());
		assertTrue(list.contains(42));
	}

	private static void testAddOrRemove_Remove()
	{
		var list = new SafeList<int>();
		list.add(100);
		list.add(200);
		bool result = list.addOrRemove(100, false);
		assertFalse(result);
		assertEqual(1, list.count());
		assertFalse(list.contains(100));
		assertTrue(list.contains(200));
	}

	private static void testAddIf_True()
	{
		var list = new SafeList<int>();
		bool result = list.addIf(10, true);
		assertTrue(result);
		assertEqual(1, list.count());
		assertTrue(list.contains(10));
	}

	private static void testAddIf_False()
	{
		var list = new SafeList<int>();
		bool result = list.addIf(10, false);
		assertFalse(result);
		assertEqual(0, list.count());
	}

	private static void testRemoveIf_True()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		bool result = list.removeIf(1, true);
		assertTrue(result);
		assertEqual(1, list.count());
		assertFalse(list.contains(1));
	}

	private static void testRemoveIf_False()
	{
		var list = new SafeList<int>();
		list.add(1);
		bool result = list.removeIf(1, false);
		assertFalse(result);
		assertEqual(1, list.count());
		assertTrue(list.contains(1));
	}

	//==================================================================
	// 批量操作
	//==================================================================
	private static void testAddRangeList()
	{
		var list = new SafeList<int>();
		var src = new List<int> { 1, 2, 3 };
		list.addRange(src);
		assertEqual(3, list.count());
		assertTrue(list.contains(1));
		assertTrue(list.contains(2));
		assertTrue(list.contains(3));
	}

	private static void testAddRangeHashSet()
	{
		var list = new SafeList<int>();
		var src = new HashSet<int> { 10, 20, 30 };
		list.addRange(src);
		assertEqual(3, list.count());
		assertTrue(list.contains(10));
		assertTrue(list.contains(20));
		assertTrue(list.contains(30));
	}

	private static void testAddRangeEmptyList()
	{
		var list = new SafeList<int>();
		list.add(1);
		var empty = new List<int>();
		list.addRange(empty);
		assertEqual(1, list.count());
	}

	private static void testSetRangeList()
	{
		var list = new SafeList<int>();
		list.add(999);
		var src = new List<int> { 7, 8, 9 };
		list.setRange(src);
		assertEqual(3, list.count());
		assertFalse(list.contains(999));
		assertTrue(list.contains(7));
		assertTrue(list.contains(8));
		assertTrue(list.contains(9));
	}

	private static void testSetRangeHashSet()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		var src = new HashSet<int> { 100, 200 };
		list.setRange(src);
		assertEqual(2, list.count());
		assertFalse(list.contains(1));
		assertFalse(list.contains(2));
		assertTrue(list.contains(100));
		assertTrue(list.contains(200));
	}

	private static void testSetRangeOverwritesExisting()
	{
		var list = new SafeList<int>();
		list.add(5);
		list.add(10);
		list.add(15);
		var src = new List<int> { 99 };
		list.setRange(src);
		assertEqual(1, list.count());
		assertEqual(99, list.get(0));
	}

	//==================================================================
	// 重置
	//==================================================================
	private static void testResetProperty()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		list.startForeach();
		list.resetProperty();
		assertEqual(0, list.count());
		assertFalse(list.isForeaching());
		// reset 后可以正常使用
		list.add(100);
		assertEqual(1, list.count());
		assertEqual(100, list.get(0));
	}

	//==================================================================
	// 扩展方法
	//==================================================================
	private static void testAddClass()
	{
		var list = new SafeList<TestSafeListClassObject>();
		TestSafeListClassObject obj = list.addClass();
		assertNotNull(obj);
		assertEqual(1, list.count());
		assertTrue(list.contains(obj));
	}
}

// 用于测试 SafeListExtension.addClass 的辅助类
public class TestSafeListClassObject : ClassObject
{
	public TestSafeListClassObject() { }
}
