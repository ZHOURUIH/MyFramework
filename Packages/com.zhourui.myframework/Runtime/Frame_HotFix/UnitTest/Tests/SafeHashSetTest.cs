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
		testClearDuringForeach();
		testStartForeachModifySyncIncremental();
		testStartForeachModifySyncFullSync();
		testGetEnumerator();

		// --- 遍历使用场景 ---
		testForeachSnapshotUnchangedByAdd();
		testForeachSnapshotUnchangedByRemove();
		testForeachSnapshotUnchangedByClear();
		testModificationsVisibleOnNextForeach();
		testForeachDispatchLike();
		testSequentialForeachSeesUpdates();
		testRepeatedStartForeachPairs();
		testModifySyncAddRemoveMixed();
		testForeachOnEmpty();
		testLargeForeachSum();
		testAddRemoveDuringForeachThenVerifyMain();
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
		var seen = new System.Collections.Generic.HashSet<int>();
		foreach (int v in set)
		{
			seen.Add(v);
		}
		assertEqual(3, seen.Count);
		assertTrue(seen.Contains(1));
		assertTrue(seen.Contains(2));
		assertTrue(seen.Contains(3));
		assertFalse(set.isForeaching());
	}
	private static void testStartForeachWithModifications()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		// 添加一些数据后再遍历，应包含所有已添加的数据
		set.add(3);
		var seen = new System.Collections.Generic.HashSet<int>();
		foreach (int v in set)
		{
			seen.Add(v);
		}
		assertEqual(3, seen.Count);
	}
	private static void testEndForeach()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		foreach (int v in set)
		{
			_ = v;
			assertTrue(set.isForeaching());
		}
		assertFalse(set.isForeaching());
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
		int count = 0;
		foreach (int v in set)
		{
			_ = v;
			count++;
		}
		assertEqual(0, count);
		assertFalse(set.isForeaching());
	}
	private static void testAddThenForeach()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		// 多次遍历
		for (int i = 0; i < 3; ++i)
		{
			int count = 0;
			foreach (int v in set)
			{
				_ = v;
				count++;
			}
			assertEqual(2, count);
			assertFalse(set.isForeaching());
		}
		// 内容不变
		assertEqual(2, set.count());
	}
	private static void testClearDuringForeach()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		int sum = 0;
		foreach (int v in set)
		{
			sum += v;
			// 遍历中清空,当前快照仍完整
			set.clear();
		}
		// 快照 1,2,3 → sum=6
		assertEqual(6, sum);
		// 主集合已清空
		assertEqual(0, set.count());
	}
	private static void testStartForeachModifySyncIncremental()
	{
		// 两次遍历之间修改,下次遍历能看到同步结果
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		// 先做一次遍历
		{
			int count = 0;
			foreach (int v in set)
			{
				_ = v;
				count++;
			}
			assertEqual(3, count);
		}
		// 修改后再次遍历验证同步
		set.add(4);
		set.remove(1);
		{
			var seen = new System.Collections.Generic.HashSet<int>();
			foreach (int v in set)
			{
				seen.Add(v);
			}
			assertEqual(3, seen.Count); // 移除 1 添加 4 = 3
			assertTrue(seen.Contains(2));
			assertTrue(seen.Contains(3));
			assertTrue(seen.Contains(4));
			assertFalse(seen.Contains(1));
		}
	}
	private static void testStartForeachModifySyncFullSync()
	{
		// 大量修改后再次遍历,验证同步结果
		SafeHashSet<int> set = new();
		set.add(1);
		// 先遍历一次
		{
			int count = 0;
			foreach (int v in set)
			{
				_ = v;
				count++;
			}
			assertEqual(1, count);
		}
		// 添加更多元素
		set.add(2);
		set.add(3);
		set.add(4);
		{
			var seen = new System.Collections.Generic.HashSet<int>();
			foreach (int v in set)
			{
				seen.Add(v);
			}
			assertEqual(4, seen.Count);
			assertTrue(seen.Contains(1));
			assertTrue(seen.Contains(2));
			assertTrue(seen.Contains(3));
			assertTrue(seen.Contains(4));
		}
	}
	private static void testGetEnumerator()
	{
		SafeHashSet<int> set = new();
		set.add(10);
		set.add(20);
		set.add(30);
		int sum = 0;
		foreach (int v in set)
		{
			sum += v;
		}
		assertEqual(60, sum);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 遍历使用场景
	private static void testForeachSnapshotUnchangedByAdd()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		int sum = 0;
		foreach (int v in set)
		{
			sum += v;
			set.add(100 + v); // 遍历中新增,不进当前快照
		}
		assertEqual(6, sum);
		assertEqual(6, set.count());
	}
	private static void testForeachSnapshotUnchangedByRemove()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		int sum = 0;
		foreach (int v in set)
		{
			sum += v;
			set.remove(1);
			set.remove(2);
		}
		assertEqual(6, sum);
		assertEqual(1, set.count()); // 剩 3
	}
	private static void testForeachSnapshotUnchangedByClear()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		int sum = 0;
		foreach (int v in set)
		{
			sum += v;
			set.clear();
		}
		assertEqual(6, sum);
		assertEqual(0, set.count());
	}
	private static void testModificationsVisibleOnNextForeach()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		foreach (int v in set)
		{
			if (v == 1)
			{
				set.add(4);
				set.remove(2);
			}
		}
		var main = set.getMainList();
		assertEqual(2, main.Count);
		assertTrue(main.Contains(1));
		assertTrue(main.Contains(4));
		assertFalse(main.Contains(2));
		// 新 foreach
		int seen = 0;
		foreach (int v in set)
		{
			_ = v;
			seen++;
		}
		assertEqual(2, seen);
	}
	private static void testForeachDispatchLike()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		int sum = 0;
		foreach (int v in set)
		{
			sum += v;
			if (v == 2)
			{
				set.remove(2);
			}
		}
		assertEqual(6, sum);
		assertEqual(2, set.count());
		assertFalse(set.contains(2));
	}
	private static void testSequentialForeachSeesUpdates()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		foreach (int v in set)
		{
			_ = v;
		}
		set.add(2);
		set.add(3);
		int count = 0;
		foreach (int v in set)
		{
			_ = v;
			count++;
		}
		assertEqual(3, count);
	}
	private static void testRepeatedStartForeachPairs()
	{
		SafeHashSet<int> set = new();
		set.add(10);
		set.add(20);
		for (int i = 0; i < 5; ++i)
		{
			int count = 0;
			foreach (int v in set)
			{
				_ = v;
				count++;
			}
			assertEqual(2, count);
			assertFalse(set.isForeaching());
		}
		assertFalse(set.isForeaching());
	}
	private static void testModifySyncAddRemoveMixed()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		foreach (int v in set)
		{
			set.add(10);
			set.remove(1);
			break;
		}
		assertEqual(3, set.count()); // 2,3,10
		assertFalse(set.contains(1));
		assertTrue(set.contains(2));
		assertTrue(set.contains(3));
		assertTrue(set.contains(10));
		// 新 foreach 同步一致
		int seen = 0;
		foreach (int v in set)
		{
			_ = v;
			seen++;
		}
		assertEqual(3, seen);
	}
	private static void testForeachOnEmpty()
	{
		SafeHashSet<int> set = new();
		int count = 0;
		foreach (int v in set)
		{
			_ = v;
			count++;
		}
		assertEqual(0, count);
		assertFalse(set.isForeaching());
	}
	private static void testLargeForeachSum()
	{
		SafeHashSet<int> set = new();
		const int N = 5000;
		for (int i = 0; i < N; ++i)
		{
			set.add(i);
		}
		long sum = 0;
		foreach (int v in set)
		{
			sum += v;
		}
		assertEqual((long)N * (N - 1) / 2, sum);
	}
	private static void testAddRemoveDuringForeachThenVerifyMain()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		foreach (int v in set)
		{
			if (v == 1)
			{
				set.add(5);
				set.remove(3);
			}
		}
		var main = set.getMainList();
		assertEqual(3, main.Count);
		assertTrue(main.Contains(1));
		assertTrue(main.Contains(2));
		assertTrue(main.Contains(5));
		assertFalse(main.Contains(3));
	}
}