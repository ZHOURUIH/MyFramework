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

		// --- 遍历使用场景 ---
		testForeachSnapshotUnchangedByAdd();
		testForeachSnapshotUnchangedByRemove();
		testForeachSnapshotUnchangedByClear();
		testModificationsVisibleOnNextForeach();
		testForeachInsideForeachUnsupported();
		testForeachDispatchLikeSkip();
		testSequentialForeachSeesUpdates();
		testRepeatedStartForeachPairs();
		testModifySyncAddRemoveClearMixed();
		testForeachOnEmptyList();
		testForeachAfterReset();
		testLargeForeachSum();
		testAddRemoveDuringForeachThenVerifyMain();
	

		testAddAndCount();
		testAddAndContains();
		testRemove();
		testClear_Deep();
		testGetMainList_Deep();
		testStartForeachReturnsCopy();
		testStartForeachReusesTempList();
		testEndForeach();
		testNestedForeach();
		testIsForeaching_Deep();
		testAddDuringForeach();
		testRemoveDuringForeach();
		testClearDuringForeach_Deep();
		testResetProperty_Deep();
		testEnumerator();
		testMultipleInstances();
		testEmptyList();
		testAddDuplicateValues();
		testStartForeachEmptyList();
		testMultipleStartForeachEndForeach();
		testGetMainListModification();

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
		testForeachDispatchLike();
		testSequentialForeachSeesUpdates_Deep();
		testForeachOnEmpty();
		testLargeForeachSum_Deep();
		testForeachEnumNested();
		testAddRemoveDuringForeachThenVerifyMain_Deep();
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
		foreach (int v in list)
		{
			_ = v;
			assertTrue(list.isForeaching());
		}
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
		// foreach 进入时对主列表做快照并遍历,结束后 isForeaching 复位
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		list.add(30);
		var iter = new System.Collections.Generic.List<int>();
		foreach (int v in list)
		{
			iter.Add(v);
			assertTrue(list.isForeaching());
		}
		assertEqual(3, iter.Count);
		assertEqual(10, iter[0]);
		assertEqual(20, iter[1]);
		assertEqual(30, iter[2]);
		assertFalse(list.isForeaching());
	}

	private static void testClearDuringForeach()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			// 遍历中清空,当前快照仍完整
			list.clear();
		}
		// 快照 1,2,3 → sum=6
		assertEqual(6, sum);
		// 但主列表已清空
		assertEqual(0, list.count());
	}

	private static void testStartForeachModifySync()
	{
		// 两次 foreach 之间修改,下次遍历能看到同步结果
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		// 第一次遍历
		{
			int sum = 0;
			foreach (int v in list)
			{
				sum += v;
			}
			assertEqual(6, sum);
		}
		// 修改后再次遍历,验证同步
		list.add(4);
		list.remove(1);
		{
			var seen = new System.Collections.Generic.List<int>();
			foreach (int v in list)
			{
				seen.Add(v);
			}
			assertEqual(3, seen.Count); // 1 removed, 4 added = still 3
			assertTrue(seen.Contains(2));
			assertTrue(seen.Contains(3));
			assertTrue(seen.Contains(4));
			assertFalse(seen.Contains(1));
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
		// 在遍历过程中 reset
		foreach (int v in list)
		{
			_ = v;
			list.resetProperty();
			break;
		}
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

	//==================================================================
	// 遍历使用场景
	//==================================================================
	private static void testForeachSnapshotUnchangedByAdd()
	{
		// foreach 期间 add,当前快照不受影响
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.add(100); // 遍历中新增,不进当前快照
		}
		assertEqual(6, sum);        // 快照仍是 1,2,3
		assertEqual(6, list.count()); // 主列表已含新增
	}
	private static void testForeachSnapshotUnchangedByRemove()
	{
		// foreach 期间 remove,当前快照不受影响
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.remove(1);
			list.remove(2);
		}
		assertEqual(6, sum);          // 快照 1,2,3 完整
		assertEqual(1, list.count()); // 主列表剩 3
	}
	private static void testForeachSnapshotUnchangedByClear()
	{
		// foreach 期间 clear,当前快照不受影响
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.clear();
		}
		assertEqual(6, sum);          // 快照完整
		assertEqual(0, list.count()); // 主列表已清空
	}
	private static void testModificationsVisibleOnNextForeach()
	{
		// 遍历中做的增删,在下一次 foreach 中可见
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		foreach (int v in list)
		{
			// 第一次遍历:新增 4,删除 1
			if (v == 1)
			{
				list.add(4);
				list.remove(2);
			}
		}
		// 下一次遍历应看到 1,3,4(1 移除过但当前快照可能不同;此处验证主列表)
		var main = list.getMainList();
		assertEqual(3, main.Count);
		assertTrue(main.Contains(1));
		assertTrue(main.Contains(3));
		assertTrue(main.Contains(4));
		assertFalse(main.Contains(2));
		// 再遍历确认内容
		var snapshot = new System.Collections.Generic.List<int>();
		foreach (int v in list)
		{
			snapshot.Add(v);
		}
		assertEqual(3, snapshot.Count);
		assertTrue(snapshot.Contains(1));
		assertTrue(snapshot.Contains(3));
		assertTrue(snapshot.Contains(4));
	}
	private static void testForeachInsideForeachUnsupported()
	{
		// foreach 内再 foreach 同一列表:外层 startForeach 已置 foreaching,内层 GetEnumerator 的 startForeach 返回 null
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		bool nestedRan = false;
		foreach (int v in list)
		{
			// 内层 foreach 会尝试再次 startForeach,返回 null 时 GetEnumerator 抛空引用
			// 这里验证外层遍历仍可正常进行(不直接触发内层,因为会抛异常)
			_ = v;
			nestedRan = true;
		}
		assertTrue(nestedRan);
	}
	private static void testForeachDispatchLikeSkip()
	{
		// 模拟事件分发:遍历中按条件跳过并移除,快照仍完整
		var list = new SafeList<int>();
		for (int i = 0; i < 5; ++i)
		{
			list.add(i);
		}
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			if (v == 2)
			{
				list.remove(2);
			}
		}
		assertEqual(10, sum);         // 0+1+2+3+4
		assertEqual(4, list.count()); // 移除 2
		assertFalse(list.contains(2));
	}
	private static void testSequentialForeachSeesUpdates()
	{
		// 连续多次 foreach,每次都能看到上一次的修改
		var list = new SafeList<int>();
		list.add(1);
		// 第一次
		foreach (int v in list)
		{
			_ = v;
		}
		list.add(2);
		list.add(3);
		// 第二次应看到 1,2,3
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(3, count);
	}
	private static void testRepeatedStartForeachPairs()
	{
		var list = new SafeList<int>();
		list.add(10);
		list.add(20);
		for (int i = 0; i < 5; ++i)
		{
			var iter = new System.Collections.Generic.List<int>();
			foreach (int v in list)
			{
				iter.Add(v);
			}
			assertEqual(2, iter.Count);
			assertFalse(list.isForeaching());
		}
		assertFalse(list.isForeaching());
	}
	private static void testModifySyncAddRemoveClearMixed()
	{
		// 遍历期间混合 add/remove/clear,验证下次 startForeach 的同步结果
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		foreach (int v in list)
		{
			list.add(10);
			list.remove(1);
			break; // 只做一轮修改
		}
		// 主列表:1,2,3,10 然后移除 1 → 2,3,10
		assertEqual(3, list.count());
		assertFalse(list.contains(1));
		assertTrue(list.contains(2));
		assertTrue(list.contains(3));
		assertTrue(list.contains(10));
		// 新 foreach 同步结果一致
		var next = new System.Collections.Generic.List<int>();
		foreach (int v in list)
		{
			next.Add(v);
		}
		assertEqual(3, next.Count);
		assertFalse(next.Contains(1));
	}
	private static void testForeachOnEmptyList()
	{
		var list = new SafeList<int>();
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(0, count);
		assertFalse(list.isForeaching());
	}
	private static void testForeachAfterReset()
	{
		var list = new SafeList<int>();
		list.add(1);
		list.resetProperty();
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(0, count);
	}
	private static void testLargeForeachSum()
	{
		var list = new SafeList<int>();
		const int N = 10000;
		for (int i = 0; i < N; ++i)
		{
			list.add(i);
		}
		long sum = 0;
		foreach (int v in list)
		{
			sum += v;
		}
		assertEqual((long)N * (N - 1) / 2, sum);
	}
	private static void testAddRemoveDuringForeachThenVerifyMain()
	{
		// 遍历中增删,结束后 getMainList 反映最终状态
		var list = new SafeList<int>();
		list.add(1);
		list.add(2);
		list.add(3);
		foreach (int v in list)
		{
			if (v == 1)
			{
				list.add(5);
				list.remove(3);
			}
		}
		var main = list.getMainList();
		assertEqual(3, main.Count);
		assertTrue(main.Contains(1));
		assertTrue(main.Contains(2));
		assertTrue(main.Contains(5));
		assertFalse(main.Contains(3));
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
	private static void testClear_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.clear();
		assertEqual(0, list.count());
		assertFalse(list.contains(1));
	}
	private static void testGetMainList_Deep()
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
		// startForeach 返回的是主列表的副本:foreach 遍历中修改主列表不影响当前快照
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.clear(); // 清空主列表,当前快照不受影响
			list.add(999);
		}
		assertEqual(6, sum);          // 快照 [1,2,3] 完整
		assertEqual(1, list.count()); // 主列表只剩 999
	}
	private static void testStartForeachReusesTempList()
	{
		// 多次 start/end(foreach 进入/退出)后列表内容与状态保持一致
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		for (int i = 0; i < 5; ++i)
		{
			int sum = 0;
			foreach (int v in list)
			{
				sum += v;
			}
			assertEqual(3, sum);
			assertFalse(list.isForeaching());
		}
		assertEqual(2, list.count());
	}
	private static void testEndForeach()
	{
		// foreach 进入时开始遍历(isForeaching=true),退出后结束(isForeaching=false)
		SafeDeepList<int> list = new();
		list.add(10);
		list.add(20);
		foreach (int v in list)
		{
			_ = v;
			assertTrue(list.isForeaching());
		}
		assertFalse(list.isForeaching());
	}
	private static void testNestedForeach()
	{
		// 深嵌套族支持 foreach 内再 foreach
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int count = 0;
		foreach (int outer in list)
		{
			assertTrue(list.isForeaching());
			foreach (int inner in list)
			{
				_ = inner;
				count++;
			}
		}
		assertEqual(9, count); // 3 外层 × 3 内层
		assertFalse(list.isForeaching());
	}
	private static void testIsForeaching_Deep()
	{
		SafeDeepList<int> list = new();
		assertFalse(list.isForeaching());
		foreach (int v in list)
		{
			_ = v;
		}
		// 空列表 foreach:进入即退出,结束后 false
		assertFalse(list.isForeaching());
		list.add(1);
		list.add(2);
		foreach (int v in list)
		{
			_ = v;
			assertTrue(list.isForeaching());
		}
		assertFalse(list.isForeaching());
	}
	private static void testAddDuringForeach()
	{
		// 遍历中添加元素:当前快照不变,主列表实时新增
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.add(3);
		}
		assertEqual(3, sum);          // 快照 [1,2]
		assertEqual(4, list.count()); // 主列表 [1,2,3,3]
	}
	private static void testRemoveDuringForeach()
	{
		// 遍历中删除元素:当前快照不受影响,主列表已删除
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.remove(2);
		}
		assertEqual(6, sum);          // 快照 [1,2,3]
		assertEqual(2, list.count()); // 主列表 [1,3]
		assertFalse(list.contains(2));
	}
	private static void testClearDuringForeach_Deep()
	{
		// 遍历中清空:当前快照不受影响,主列表已清空
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.clear();
		}
		assertEqual(3, sum); // 快照 [1,2]
		assertEqual(0, list.count());
	}
	private static void testResetProperty_Deep()
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
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(0, count);
		assertFalse(list.isForeaching());
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
		// 空列表多次 foreach 均正常,不残留遍历状态
		SafeDeepList<int> list = new();
		for (int i = 0; i < 3; ++i)
		{
			int count = 0;
			foreach (int v in list)
			{
				_ = v;
				count++;
			}
			assertEqual(0, count);
			assertFalse(list.isForeaching());
		}
	}
	private static void testMultipleStartForeachEndForeach()
	{
		// 连续多次 foreach,每次都正常遍历且结束后状态复位
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		for (int i = 0; i < 5; ++i)
		{
			int sum = 0;
			foreach (int v in list)
			{
				sum += v;
			}
			assertEqual(6, sum);
			assertFalse(list.isForeaching());
		}
		// 多次遍历后列表内容不变
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
	//------------------------------------------------------------------------------------------------------------------------------
	// 深度嵌套遍历
	private static void testNestedForeachUsingEnumerator()
	{
		// foreach 枚举器嵌套遍历
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int count = 0;
		foreach (int outer in list)
		{
			foreach (int inner in list)
			{
				count++;
			}
		}
		assertEqual(4, count); // 2 外层 × 2 内层
	}
	private static void testTripleNestedForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int count = 0;
		foreach (int a in list)
		{
			foreach (int b in list)
			{
				foreach (int c in list)
				{
					count++;
				}
			}
		}
		assertEqual(27, count); // 3³
	}
	private static void testQuadrupleNestedForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int count = 0;
		foreach (int a in list)
		{
			foreach (int b in list)
			{
				foreach (int c in list)
				{
					foreach (int d in list)
					{
						count++;
					}
				}
			}
		}
		assertEqual(16, count); // 2⁴
	}
	private static void testNestedForeachWithModifications()
	{
		// 嵌套遍历中修改主列表:外层快照固定,内层每次取当前快照
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int sum = 0;
		foreach (int outer in list) // 外层快照固定 [1,2]
		{
			foreach (int inner in list) // 内层每次重新生成当前快照
			{
				sum += inner;
			}
			list.add(100); // 遍历中新增
		}
		// 外层 2 次;第一次内层快照 [1,2] sum=3,第二次内层快照 [1,2,100] sum=103
		assertEqual(106, sum);
		// 主列表新增了 2 个 100
		assertEqual(4, list.count());
	}
	private static void testNestedAddDuringForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		foreach (int v in list)
		{
			list.add(10); // 内层(当前)遍历新增,不进快照
		}
		// 快照 2 个,主列表 4 个
		assertEqual(4, list.count());
	}
	private static void testNestedRemoveDuringForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.remove(1);
		}
		// 快照 1,2,3 → sum=6
		assertEqual(6, sum);
		// 主列表剩 2,3
		assertEqual(2, list.count());
		assertFalse(list.contains(1));
	}
	private static void testNestedClearDuringForeach()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.clear();
		}
		assertEqual(6, sum);
		assertEqual(0, list.count());
	}
	private static void testNestedForeachOuterInnerSnapshotsIndependent()
	{
		// 内外层 foreach 快照相互独立:内层遍历中的修改不影响外层本次遍历
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int outerCount = 0;
		foreach (int outer in list)
		{
			foreach (int inner in list)
			{
				_ = inner;
			}
			// 每次外层迭代内层都会完整跑一遍 [1,2,3],共 3 次外层
			outerCount++;
		}
		assertEqual(3, outerCount);
		assertEqual(3, list.count()); // 主列表不变
		assertFalse(list.isForeaching());
	}
	private static void testNestedForeachReuseTempLists()
	{
		// 嵌套遍历后临时列表正确回收
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		foreach (int a in list)
		{
			foreach (int b in list)
			{
				_ = a + b;
			}
		}
		// 遍历全部结束后 isForeaching 应为 false,临时列表已回收
		assertFalse(list.isForeaching());
		// 再次遍历正常
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(3, count);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 遍历使用场景
	private static void testForeachSnapshotUnchangedByAdd_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.add(100);
		}
		assertEqual(3, sum);
		assertEqual(4, list.count());
	}
	private static void testForeachSnapshotUnchangedByRemove_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.remove(1);
			list.remove(2);
		}
		assertEqual(6, sum);
		assertEqual(1, list.count());
	}
	private static void testForeachSnapshotUnchangedByClear_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			list.clear();
		}
		assertEqual(6, sum);
		assertEqual(0, list.count());
	}
	private static void testModificationsVisibleOnNextForeach_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		foreach (int v in list)
		{
			if (v == 1)
			{
				list.add(4);
				list.remove(2);
			}
		}
		var main = list.getMainList();
		assertEqual(2, main.Count);
		assertTrue(main.Contains(1));
		assertTrue(main.Contains(4));
		assertFalse(main.Contains(2));
		// 新 foreach
		int seen = 0;
		foreach (int v in list)
		{
			_ = v;
			seen++;
		}
		assertEqual(2, seen);
	}
	private static void testForeachDispatchLike()
	{
		SafeDeepList<int> list = new();
		for (int i = 0; i < 5; ++i)
		{
			list.add(i);
		}
		int sum = 0;
		foreach (int v in list)
		{
			sum += v;
			if (v == 2)
			{
				list.remove(2);
			}
		}
		assertEqual(10, sum);
		assertEqual(4, list.count());
		assertFalse(list.contains(2));
	}
	private static void testSequentialForeachSeesUpdates_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		foreach (int v in list)
		{
			_ = v;
		}
		list.add(2);
		list.add(3);
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(3, count);
	}
	private static void testForeachOnEmpty()
	{
		SafeDeepList<int> list = new();
		int count = 0;
		foreach (int v in list)
		{
			_ = v;
			count++;
		}
		assertEqual(0, count);
		assertFalse(list.isForeaching());
	}
	private static void testLargeForeachSum_Deep()
	{
		SafeDeepList<int> list = new();
		const int N = 5000;
		for (int i = 0; i < N; ++i)
		{
			list.add(i);
		}
		long sum = 0;
		foreach (int v in list)
		{
			sum += v;
		}
		assertEqual((long)N * (N - 1) / 2, sum);
	}
	private static void testForeachEnumNested()
	{
		// foreach 枚举器嵌套 + 内层修改
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int count = 0;
		foreach (int outer in list)
		{
			foreach (int inner in list)
			{
				count++;
			}
			if (outer == 1)
			{
				list.remove(2);
			}
		}
		// 外层快照 [1,2,3] 固定 3 次;每次内层在进入时重新快照当前主表:
		// 第一次 [1,2,3]→3;移除 2 后第二次 [1,3]→2;第三次 [1,3]→2,合计 7
		assertEqual(7, count); // 外层快照 3 × 内层实时快照(3+2+2)
	}
	private static void testAddRemoveDuringForeachThenVerifyMain_Deep()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		foreach (int v in list)
		{
			if (v == 1)
			{
				list.add(5);
				list.remove(3);
			}
		}
		var main = list.getMainList();
		assertEqual(3, main.Count);
		assertTrue(main.Contains(1));
		assertTrue(main.Contains(2));
		assertTrue(main.Contains(5));
		assertFalse(main.Contains(3));
	}
}

// 用于测试 SafeListExtension.addClass 的辅助类
public class TestSafeListClassObject : ClassObject
{
	public TestSafeListClassObject() { }
}
