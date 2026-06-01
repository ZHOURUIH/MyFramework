#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using static TestAssert;

public class SafeHashSetTest
{
	public static void Run()
	{
		testAddAndCount();
		testAddDuplicate();
		testContains();
		testRemove();
		testRemoveNonExistent();
		testClear();
		testStartForeachNoModifications();
		testStartForeachWithModifications();
		testEndForeach();
		testGetMainList();
		testAddIfTrue();
		testAddIfFalse();
		testAddOrRemoveAdd();
		testAddOrRemoveRemove();
		testClearWhenNotForeaching();
		testMultipleInstances();
		testEmptySet();
		testResetProperty();
		testStartForeachEmptySet();
		testAddThenForeach();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddAndCount()
	{
		SafeHashSet<int> set = new();
		assertEqual(0, set.count());
		bool added = set.add(10);
		assertTrue(added);
		assertEqual(1, set.count());
		set.add(20);
		set.add(30);
		assertEqual(3, set.count());
	}
	private static void testAddDuplicate()
	{
		SafeHashSet<int> set = new();
		assertTrue(set.add(5));
		assertFalse(set.add(5)); // HashSet 不允许重复
		assertEqual(1, set.count());
	}
	private static void testContains()
	{
		SafeHashSet<string> set = new();
		set.add("hello");
		assertTrue(set.contains("hello"));
		assertFalse(set.contains("world"));
	}
	private static void testRemove()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		bool removed = set.remove(2);
		assertTrue(removed);
		assertEqual(2, set.count());
		assertFalse(set.contains(2));
		assertTrue(set.contains(1));
		assertTrue(set.contains(3));
	}
	private static void testRemoveNonExistent()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		bool removed = set.remove(999);
		assertFalse(removed);
		assertEqual(1, set.count());
	}
	private static void testClear()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.clear();
		assertEqual(0, set.count());
		assertFalse(set.contains(1));
	}
	private static void testStartForeachNoModifications()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		HashSet<int> snapshot = set.startForeach();
		assertEqual(3, snapshot.Count);
		assertTrue(snapshot.Contains(1));
		assertTrue(snapshot.Contains(2));
		assertTrue(snapshot.Contains(3));
		set.endForeach();
	}
	private static void testStartForeachWithModifications()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		// 添加一些数据后再 startForeach，应同步修改
		set.add(3);
		HashSet<int> snapshot = set.startForeach();
		// 应包含所有已添加的数据
		assertEqual(3, snapshot.Count);
		set.endForeach();
	}
	private static void testEndForeach()
	{
		SafeHashSet<int> set = new();
		set.startForeach();
		set.endForeach();
	}
	private static void testGetMainList()
	{
		SafeHashSet<int> set = new();
		set.add(100);
		set.add(200);
		HashSet<int> main = set.getMainList();
		assertEqual(2, main.Count);
		assertTrue(main.Contains(100));
		assertTrue(main.Contains(200));
	}
	private static void testAddIfTrue()
	{
		SafeHashSet<int> set = new();
		bool result = set.addIf(42, true);
		assertTrue(result);
		assertTrue(set.contains(42));
	}
	private static void testAddIfFalse()
	{
		SafeHashSet<int> set = new();
		bool result = set.addIf(42, false);
		assertFalse(result);
		assertFalse(set.contains(42));
	}
	private static void testAddOrRemoveAdd()
	{
		SafeHashSet<int> set = new();
		bool result = set.addOrRemove(10, true);
		assertTrue(result);
		assertTrue(set.contains(10));
	}
	private static void testAddOrRemoveRemove()
	{
		SafeHashSet<int> set = new();
		set.add(10);
		bool result = set.addOrRemove(10, false);
		assertFalse(result);
		assertFalse(set.contains(10));
	}
	private static void testClearWhenNotForeaching()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.clear();
		assertEqual(0, set.count());
		// clear 后应能继续添加
		set.add(3);
		assertEqual(1, set.count());
		assertTrue(set.contains(3));
	}
	private static void testMultipleInstances()
	{
		SafeHashSet<int> s1 = new();
		SafeHashSet<string> s2 = new();
		s1.add(1);
		s1.add(2);
		s2.add("a");
		assertEqual(2, s1.count());
		assertEqual(1, s2.count());
	}
	private static void testEmptySet()
	{
		SafeHashSet<int> set = new();
		assertEqual(0, set.count());
		assertFalse(set.contains(0));
	}
	private static void testResetProperty()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.resetProperty();
		assertEqual(0, set.count());
	}
	private static void testStartForeachEmptySet()
	{
		SafeHashSet<int> set = new();
		HashSet<int> snapshot = set.startForeach();
		assertEqual(0, snapshot.Count);
		set.endForeach();
	}
	private static void testAddThenForeach()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		// 多次 start/end
		for (int i = 0; i < 3; ++i)
		{
			HashSet<int> snapshot = set.startForeach();
			assertEqual(2, snapshot.Count);
			set.endForeach();
		}
		// 内容不变
		assertEqual(2, set.count());
	}
}
#endif
