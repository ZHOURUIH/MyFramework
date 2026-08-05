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
	private static void testIsForeaching()
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
	private static void testClearDuringForeach()
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
	private static void testForeachSnapshotUnchangedByAdd()
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
	private static void testForeachSnapshotUnchangedByRemove()
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
	private static void testForeachSnapshotUnchangedByClear()
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
	private static void testModificationsVisibleOnNextForeach()
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
	private static void testSequentialForeachSeesUpdates()
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
	private static void testLargeForeachSum()
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
	private static void testAddRemoveDuringForeachThenVerifyMain()
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