using System.Collections.Generic;
using static TestAssert;

// SafeFastDeepList 及其配套 SafeFastDeepListReader 的测试
// 遍历统一使用 SafeFastDeepListReader<T> 的 using 模式,count 由 Reader 的 out 参数提供
public class SafeFastDeepListTest
{
	public static void Run()
	{
		// ---------- 基础数据操作 ----------
		testAddAndCount();
		testGetByIndex();
		testGetOutOfBounds();
		testRemove();
		testRemoveNonExistent();
		testRemoveDuplicateOnlyOnce();
		testClear();
		testClearEmpty();
		testIsEmpty();
		testResetProperty();
		testGetMainList();
		testAddAndGetMainList();
		testMultipleInstances();
		testEmptyList();

		// ---------- Reader 基础遍历 ----------
		testReaderReadsCorrectly();
		testReaderMatchesSource();
		testReaderCountEqualsSource();
		testReaderEmptyList();
		testReaderSequential();
		testReaderCanReuseAfterDispose();
		testReaderGenericString();
		testReaderGenericClassObject();

		// ---------- Reader 遍历中修改 ----------
		testReaderRemoveDuringForeachCompactsOnDispose();
		testReaderRemoveAllDuringForeach();
		testReaderClearDuringForeach();
		testReaderAddDuringForeachDoesNotExtendCount();
		testReaderRemoveAndAddMixed();

		// ---------- Reader 嵌套遍历 ----------
		testReaderNested();
		testReaderNestedInnerRemoveCompactsOnlyAtOuterEnd();
		testReaderTripleNested();

		// ---------- 底层 startForeach/endForeach 机制 ----------
		testIsForeaching();
		testStartForeachReturnsMainList();
		testEndForeach();
		testNestedForeachManual();
		testMultipleStartForeachEndForeach();
		testRemoveDuringManualForeach();
		testClearDuringManualForeach();
		testNestedForeachRemoveCompactOuterOnly();
		testRemoveAllDuringManualForeach();
		testClearEmptyDuringForeach();
		testRemoveMarkDefaultDuringForeach();

		// ---------- 规模与边界 ----------
		testLargeListReader();
		testReaderOutParamAfterDispose();

		// ---------- 真实使用场景(事件分发模式) ----------
		testDispatchLikeNullSafeCall();
		testDispatchSelfUnregisterDuringIteration();
		testDispatchAddListenerDuringIterationNotVisited();
		testDispatchRemoveCurrentElement();
		testDispatchMixedUnregisterAdd();
		testDispatchPerElementTryCatchContinues();
		testDispatchBreakStillCompacts();
		testDispatchEarlyReturnStillCompacts();

		// ---------- 更多嵌套组合 ----------
		testNestedOuterRemoveInnerAdd();
		testQuadrupleNestedMixedOps();
		testNestedReaderInnerDisposeThenOuterRead();
		testDeepNestedWithHolesCompactedOnce();
		testNestedAddVisitedByNextPass();

		// ---------- 更多边界与类型 ----------
		testRemoveHeadMiddleTailDuringForeach();
		testHoleAtHeadCompactsToFront();
		testMultipleValueTypes();
		testClassObjectType();
		testGetOnEmptyDuringForeach();
		testCountConsistencyDuringModify();
		testRepeatedPassesConsistency();
		testResetThenRebuildAndReader();
		testStartForeachPairedManualCount();
		testReaderOverflowIndexGetDefault();
		testRemoveReturnValueDuringForeach();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- 基础数据操作 -----------------------------------------------
	private static void testAddAndCount()
	{
		SafeFastDeepList<int> list = new();
		assertEqual(0, list.count());
		list.add(10);
		assertEqual(1, list.count());
		list.add(20);
		list.add(30);
		assertEqual(3, list.count());
	}
	private static void testGetByIndex()
	{
		SafeFastDeepList<int> list = new();
		list.add(100);
		list.add(200);
		list.add(300);
		assertEqual(100, list.get(0));
		assertEqual(200, list.get(1));
		assertEqual(300, list.get(2));
	}
	private static void testGetOutOfBounds()
	{
		// get 通过 List.get 扩展方法访问,越界返回 default,不抛异常
		SafeFastDeepList<int> list = new();
		list.add(42);
		assertEqual(default(int), list.get(-1));
		assertEqual(default(int), list.get(999));
		assertEqual(1, list.count());
	}
	private static void testRemove()
	{
		SafeFastDeepList<int> list = new();
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
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		bool removed = list.remove(999);
		assertFalse(removed);
		assertEqual(2, list.count());
	}
	private static void testRemoveDuplicateOnlyOnce()
	{
		// remove 按 IndexOf 只移除第一个匹配项
		SafeFastDeepList<int> list = new();
		list.add(5);
		list.add(5);
		list.add(5);
		bool removed = list.remove(5);
		assertTrue(removed);
		assertEqual(2, list.count());
		removed = list.remove(5);
		assertTrue(removed);
		assertEqual(1, list.count());
	}
	private static void testClear()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.clear();
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
	}
	private static void testClearEmpty()
	{
		SafeFastDeepList<int> list = new();
		list.clear();
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
	}
	private static void testIsEmpty()
	{
		SafeFastDeepList<int> list = new();
		assertTrue(list.isEmpty());
		list.add(42);
		assertFalse(list.isEmpty());
		list.clear();
		assertTrue(list.isEmpty());
	}
	private static void testResetProperty()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.startForeach();
		list.resetProperty();
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
		assertFalse(list.isForeaching());
	}
	private static void testGetMainList()
	{
		SafeFastDeepList<int> list = new();
		list.add(10);
		list.add(20);
		List<int> main = list.getMainList();
		assertEqual(2, main.Count);
		assertEqual(10, main[0]);
		assertEqual(20, main[1]);
	}
	private static void testAddAndGetMainList()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		List<int> main = list.getMainList();
		main.Add(2);
		assertEqual(2, list.count());
	}
	private static void testMultipleInstances()
	{
		SafeFastDeepList<int> list1 = new();
		SafeFastDeepList<int> list2 = new();
		list1.add(1);
		list2.add(10);
		list2.add(20);
		assertEqual(1, list1.count());
		assertEqual(2, list2.count());
	}
	private static void testEmptyList()
	{
		SafeFastDeepList<int> list = new();
		assertTrue(list.isEmpty());
		assertEqual(0, list.count());
		assertFalse(list.isForeaching());
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- Reader 基础遍历 -----------------------------------------------
	private static void testReaderReadsCorrectly()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int sum = 0;
		int iterated = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				sum += list.get(i);
				iterated++;
			}
		}
		assertEqual(3, iterated);
		assertEqual(6, sum);
		// using 释放后已退出遍历状态
		assertFalse(list.isForeaching());
	}
	private static void testReaderMatchesSource()
	{
		SafeFastDeepList<int> list = new();
		list.add(10);
		list.add(20);
		list.add(30);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(3, count);
			assertEqual(10, list.get(0));
			assertEqual(20, list.get(1));
			assertEqual(30, list.get(2));
		}
	}
	private static void testReaderCountEqualsSource()
	{
		// Reader 的 count 应等于进入遍历前的 count()
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.add(4);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(list.count(), count);
			assertEqual(4, count);
		}
	}
	private static void testReaderEmptyList()
	{
		SafeFastDeepList<int> list = new();
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(0, count);
			assertTrue(list.isForeaching());
		}
		assertFalse(list.isForeaching());
	}
	private static void testReaderSequential()
	{
		// 连续多次用 Reader 遍历互不影响
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int count1 = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var c1))
		{
			for (int i = 0; i < c1; ++i)
			{
				count1++;
			}
		}
		int count2 = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var c2))
		{
			for (int i = 0; i < c2; ++i)
			{
				count2++;
			}
		}
		assertEqual(2, count1);
		assertEqual(2, count2);
	}
	private static void testReaderCanReuseAfterDispose()
	{
		SafeFastDeepList<int> list = new();
		list.add(7);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(1, count);
		}
		// 释放后仍可正常遍历
		using (var reader = new SafeFastDeepListReader<int>(list, out var count2))
		{
			assertEqual(1, count2);
		}
	}
	private static void testReaderGenericString()
	{
		SafeFastDeepList<string> list = new();
		list.add("a");
		list.add("b");
		list.add("c");
		string joined = "";
		using (var reader = new SafeFastDeepListReader<string>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				joined += list.get(i);
			}
		}
		assertEqual("abc", joined);
	}
	private static void testReaderGenericClassObject()
	{
		SafeFastDeepList<TestSafeFastClass> list = new();
		list.add(new TestSafeFastClass(1));
		list.add(new TestSafeFastClass(2));
		int sum = 0;
		using (var reader = new SafeFastDeepListReader<TestSafeFastClass>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				sum += list.get(i).mValue;
			}
		}
		assertEqual(3, sum);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- Reader 遍历中修改 -----------------------------------------------
	private static void testReaderRemoveDuringForeachCompactsOnDispose()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int iterated = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(3, count);
			// 遍历中删除,标记为 default
			bool removed = list.remove(2);
			assertTrue(removed);
			assertEqual(3, list.count());          // 标记删除,count 不变
			assertEqual(default(int), list.get(1)); // 被打洞
			for (int i = 0; i < count; ++i)
			{
				iterated++;
			}
		}
		// Dispose 触发 compact
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
		assertFalse(list.isForeaching());
	}
	private static void testReaderRemoveAllDuringForeach()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(3, count);
			list.remove(1);
			list.remove(2);
			list.remove(3);
			assertEqual(3, list.count());
		}
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
	}
	private static void testReaderClearDuringForeach()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(3, count);
			list.clear();
			assertEqual(3, list.count());
			assertEqual(default(int), list.get(0));
		}
		assertEqual(0, list.count());
	}
	private static void testReaderAddDuringForeachDoesNotExtendCount()
	{
		// Reader 的 count 固定,遍历中新增元素不会进入本次遍历
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int iterated = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(2, count);
			for (int i = 0; i < count; ++i)
			{
				iterated++;
				list.add(iterated + 100); // 遍历中新增
			}
		}
		// 本次只遍历到进入时的 2 个
		assertEqual(2, iterated);
		// 新增元素被保留
		assertEqual(4, list.count());
	}
	private static void testReaderRemoveAndAddMixed()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			list.remove(2);   // 标记删除
			list.add(4);      // 追加到末尾
			assertEqual(4, list.count()); // 1, default, 3, 4
			assertEqual(default(int), list.get(1));
			assertEqual(4, list.get(3));
		}
		// compact 移除打洞的 default,保留新增
		assertEqual(3, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
		assertEqual(4, list.get(2));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- Reader 嵌套遍历 -----------------------------------------------
	private static void testReaderNested()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int sum = 0;
		using (var outer = new SafeFastDeepListReader<int>(list, out var c1))
		{
			for (int i = 0; i < c1; ++i)
			{
				using (var inner = new SafeFastDeepListReader<int>(list, out var c2))
				{
					for (int j = 0; j < c2; ++j)
					{
						sum += list.get(j);
					}
				}
			}
		}
		// 内层遍历了 2 次(外层每迭代一次),每次和为 3,共 6
		assertEqual(6, sum);
		assertFalse(list.isForeaching());
	}
	private static void testReaderNestedInnerRemoveCompactsOnlyAtOuterEnd()
	{
		// 嵌套删除时,只有最外层 endForeach(dispose) 才触发 compact
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var outer = new SafeFastDeepListReader<int>(list, out var c1))
		{
			using (var inner = new SafeFastDeepListReader<int>(list, out var c2))
			{
				list.remove(2);
				assertEqual(3, list.count());
			}
			// 内层 dispose 后 depth 回到 1,不 compact
			assertEqual(3, list.count());
			assertEqual(default(int), list.get(1));
		}
		// 外层 dispose 后 compact
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
	}
	private static void testReaderTripleNested()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		int total = 0;
		using (var r1 = new SafeFastDeepListReader<int>(list, out var c1))
		{
			using (var r2 = new SafeFastDeepListReader<int>(list, out var c2))
			{
				using (var r3 = new SafeFastDeepListReader<int>(list, out var c3))
				{
					for (int i = 0; i < c3; ++i)
					{
						total++;
					}
				}
			}
		}
		assertEqual(1, total);
		assertFalse(list.isForeaching());
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- 底层 startForeach/endForeach 机制 -----------------------------------------------
	private static void testIsForeaching()
	{
		SafeFastDeepList<int> list = new();
		assertFalse(list.isForeaching());
		list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach();
		assertFalse(list.isForeaching());
	}
	private static void testStartForeachReturnsMainList()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		List<int> iter = list.startForeach();
		assertEqual(3, iter.Count);
		assertEqual(1, iter[0]);
		list.endForeach();
	}
	private static void testEndForeach()
	{
		SafeFastDeepList<int> list = new();
		list.add(10);
		list.startForeach();
		assertTrue(list.isForeaching());
		list.endForeach();
		assertFalse(list.isForeaching());
	}
	private static void testNestedForeachManual()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.startForeach();
		assertTrue(list.isForeaching());
		list.startForeach();
		list.endForeach();
		assertTrue(list.isForeaching()); // 外层仍在
		list.endForeach();
		assertFalse(list.isForeaching());
	}
	private static void testMultipleStartForeachEndForeach()
	{
		SafeFastDeepList<int> list = new();
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
	private static void testRemoveDuringManualForeach()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.startForeach();
		bool removed = list.remove(2);
		assertTrue(removed);
		assertEqual(3, list.count()); // 标记删除
		assertEqual(default(int), list.get(1));
		list.endForeach();
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
	}
	private static void testClearDuringManualForeach()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.startForeach();
		list.clear();
		assertEqual(3, list.count());
		assertEqual(default(int), list.get(0));
		list.endForeach();
		assertEqual(0, list.count());
	}
	private static void testNestedForeachRemoveCompactOuterOnly()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.startForeach(); // 外层
		list.startForeach(); // 内层
		list.remove(2);
		assertEqual(3, list.count());
		assertEqual(default(int), list.get(1));
		list.endForeach();   // 内层结束,depth=1,不 compact
		assertEqual(3, list.count());
		list.endForeach();   // 外层结束,depth=0,compact
		assertEqual(2, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
	}
	private static void testRemoveAllDuringManualForeach()
	{
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.startForeach();
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
		SafeFastDeepList<int> list = new();
		list.startForeach();
		assertEqual(0, list.count());
		list.clear();
		assertEqual(0, list.count());
		list.endForeach();
		assertEqual(0, list.count());
	}
	private static void testRemoveMarkDefaultDuringForeach()
	{
		// 遍历中删除的元素被标记为 default,且不真正移除
		SafeFastDeepList<int> list = new();
		list.add(10);
		list.add(20);
		list.add(30);
		list.startForeach();
		list.remove(20);
		// 下标 1 处被打洞为 default
		assertEqual(default(int), list.get(1));
		assertEqual(10, list.get(0));
		assertEqual(30, list.get(2));
		list.endForeach();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- 规模与边界 -----------------------------------------------
	private static void testLargeListReader()
	{
		SafeFastDeepList<int> list = new();
		const int N = 10000;
		for (int i = 0; i < N; ++i)
		{
			list.add(i);
		}
		long sum = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(N, count);
			for (int i = 0; i < count; ++i)
			{
				sum += list.get(i);
			}
		}
		// 0..N-1 求和 = N*(N-1)/2
		assertEqual((long)N * (N - 1) / 2, sum);
		assertEqual(N, list.count());
	}
	private static void testReaderOutParamAfterDispose()
	{
		// out count 在 Reader 构造时已捕获,dispose 后该值保持不变(是局部变量副本)
		SafeFastDeepList<int> list = new();
		list.add(5);
		list.add(6);
		int capturedCount;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			capturedCount = count;
			list.remove(5);
		}
		assertEqual(2, capturedCount); // 进入时 count=2
		assertEqual(1, list.count());  // dispose 后已 compact
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- 真实使用场景(事件分发模式) -----------------------------------------------
	private static void testDispatchLikeNullSafeCall()
	{
		// 模拟 EventSystem.pushEvent:遍历中用 get(i)?.call(),跳过遍历中被删除打洞的 null
		SafeFastDeepList<TestSafeFastClass> list = new();
		for (int i = 0; i < 5; ++i)
		{
			list.add(new TestSafeFastClass(i + 1));
		}
		int called = 0;
		using (var reader = new SafeFastDeepListReader<TestSafeFastClass>(list, out var count))
		{
			// 遍历中删除第一个,使其打洞为 null
			list.remove(list.get(0));
			for (int i = 0; i < count; ++i)
			{
				// 等价于 eventList.get(i)?.call(param),null 安全跳过
				TestSafeFastClass e = list.get(i);
				if (e != null)
				{
					e.call();
					called++;
				}
			}
		}
		// 1 个被打洞为 null 被跳过,其余 4 个被调用
		assertEqual(4, called);
		assertEqual(4, list.count());
	}
	private static void testDispatchSelfUnregisterDuringIteration()
	{
		// 事件分发中,某个监听者取消注册(从列表移除自己),打洞后后续遍历跳过
		// 注意:元素不能用 default(int)=0 参与删除标记的打洞与 compact,否则 0 会被误判为已删除
		SafeFastDeepList<int> list = new();
		for (int i = 1; i <= 6; ++i)
		{
			list.add(i);
		}
		int sum = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				int v = list.get(i);
				if (v == 4)
				{
					list.remove(4); // 取消注册自己 → 打洞 default
				}
				else if (v != default(int))
				{
					sum += v;
				}
			}
		}
		// 1+2+3+5+6=17
		assertEqual(17, sum);
		assertEqual(5, list.count());
		assertFalse(list.contains(4));
	}
	private static void testDispatchAddListenerDuringIterationNotVisited()
	{
		// 分发中新增的监听者不会在本次遍历中被调用(固定 count)
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		int iterated = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(2, count);
			for (int i = 0; i < count; ++i)
			{
				iterated++;
				list.add(100 + iterated); // 分发中新增监听
			}
		}
		assertEqual(2, iterated);      // 本次只 2 个
		assertEqual(4, list.count());  // 新增保留到下次
	}
	private static void testDispatchRemoveCurrentElement()
	{
		// 分发中移除"当前正在遍历"的元素(自我移除)
		SafeFastDeepList<int> list = new();
		for (int i = 0; i < 5; ++i)
		{
			list.add(i);
		}
		int sum = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				int v = list.get(i);
				if (v == default(int)) continue; // 已删除
				sum += v;
				list.remove(v); // 每个访问后立即移除自己
			}
		}
		// 0+1+2+3+4 = 10,但遍历中是固定 count=5,每个元素访问时移除后打洞
		assertEqual(10, sum);
		// dispose 后 compact,全部移除
		assertEqual(0, list.count());
		assertTrue(list.isEmpty());
	}
	private static void testDispatchMixedUnregisterAdd()
	{
		// 分发中混合:移除已有监听 + 新增监听
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(3, count);
			list.remove(2); // 打洞
			list.add(9);    // 追加
			assertEqual(default(int), list.get(1));
			assertEqual(9, list.get(3));
		}
		// compact:1,3,9
		assertEqual(3, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
		assertEqual(9, list.get(2));
	}
	private static void testDispatchPerElementTryCatchContinues()
	{
		// 单个元素回调抛异常,不影响后续元素遍历
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int processed = 0;
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				try
				{
					int v = list.get(i);
					if (v == 2)
					{
						throw new System.Exception("boom");
					}
					processed++;
				}
				catch (System.Exception) { /* 单个失败不中断 */ }
			}
		}
		// 1 和 3 处理成功,2 抛异常被捕获
		assertEqual(2, processed);
	}
	private static void testDispatchBreakStillCompacts()
	{
		// using 中 break 提前退出,Dispose 仍会调用 endForeach 触发 compact
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				if (list.get(i) == 2)
				{
					list.remove(2);
					break;
				}
			}
		}
		// break 后 Dispose 仍 compact
		assertEqual(2, list.count());
		assertFalse(list.isForeaching());
	}
	private static void testDispatchEarlyReturnStillCompacts()
	{
		// using 内提前 return,Dispose 仍触发 compact(用辅助方法模拟)
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.add(4);
		doDispatchAndReturnEarly(list);
		assertEqual(3, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
		assertEqual(4, list.get(2));
		assertFalse(list.isForeaching());
	}
	private static void doDispatchAndReturnEarly(SafeFastDeepList<int> list)
	{
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			for (int i = 0; i < count; ++i)
			{
				if (list.get(i) == 2)
				{
					list.remove(2);
					return; // 提前返回,Dispose 仍被调用
				}
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- 更多嵌套组合 -----------------------------------------------
	private static void testNestedOuterRemoveInnerAdd()
	{
		// 外层遍历中删除,内层遍历中新增
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var outer = new SafeFastDeepListReader<int>(list, out var c1))
		{
			for (int i = 0; i < c1; ++i)
			{
				if (list.get(i) == 2) 
					list.remove(2);
			}
			using (var inner = new SafeFastDeepListReader<int>(list, out var c2))
			{
				assertEqual(3, c2); // 内层 count 仍为 3(打洞未 compact)
				for (int j = 0; j < c2; ++j)
				{
					if (list.get(j) == 1) 
						list.add(9);
				}
			}
		}
		// 外层 dispose 后 compact:去掉 2 的洞,保留新增 9
		assertEqual(3, list.count());
		assertTrue(list.contains(1));
		assertTrue(list.contains(3));
		assertTrue(list.contains(9));
		assertFalse(list.contains(2));
	}
	private static void testQuadrupleNestedMixedOps()
	{
		// 四层嵌套遍历,混合操作
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		int touched = 0;
		using (var r1 = new SafeFastDeepListReader<int>(list, out var c1))
		{
			using var r2 = new SafeFastDeepListReader<int>(list, out var c2);
			using var r3 = new SafeFastDeepListReader<int>(list, out var c3);
			using var r4 = new SafeFastDeepListReader<int>(list, out var c4);
			for (int i = 0; i < c4; ++i)
			{
				if (list.get(i) == 3) 
					list.remove(3);
				touched++;
			}
		}
		// 只有最外层 dispose 才 compact
		assertEqual(2, list.count());
		assertEqual(3, touched); // c4=3,遍历 3 次
		assertFalse(list.contains(3));
	}
	private static void testNestedReaderInnerDisposeThenOuterRead()
	{
		// 内层 reader 先 dispose 后,外层仍可读取到未被 compact 的数据
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		using (var outer = new SafeFastDeepListReader<int>(list, out var c1))
		{
			using (var inner = new SafeFastDeepListReader<int>(list, out var c2))
			{
				list.remove(2);
				assertEqual(3, c2);
			}
			// 内层 dispose 后,外层仍在遍历中,数据未 compact
			assertEqual(3, list.count());
			assertEqual(default(int), list.get(1));
			assertTrue(list.isForeaching());
		}
		assertEqual(2, list.count());
	}
	private static void testDeepNestedWithHolesCompactedOnce()
	{
		// 深嵌套中打多个洞,只有最外层 end 时一次性 compact
		SafeFastDeepList<int> list = new();
		for (int i = 0; i < 6; ++i)
		{
			list.add(i);
		}
		using (var r1 = new SafeFastDeepListReader<int>(list, out var c1))
		{
			using (var r2 = new SafeFastDeepListReader<int>(list, out var c2))
			{
				list.remove(0);
				list.remove(2);
				list.remove(4);
				assertEqual(6, list.count()); // 3 个洞
			}
			assertEqual(6, list.count()); // 内层结束不 compact
		}
		// 外层结束一次性 compact,3 个洞移除
		assertEqual(3, list.count());
		assertEqual(1, list.get(0));
		assertEqual(3, list.get(1));
		assertEqual(5, list.get(2));
	}
	private static void testNestedAddVisitedByNextPass()
	{
		// 嵌套遍历中新增的元素,在"下一个新的 reader"遍历时可见
		SafeFastDeepList<int> list = new();
		list.add(1);
		using (var reader = new SafeFastDeepListReader<int>(list, out var c1))
		{
			list.add(2); // 遍历中新增
			assertEqual(1, c1);
		}
		// 新一轮 reader 应看到新增的 2
		using (var reader2 = new SafeFastDeepListReader<int>(list, out var c2))
		{
			assertEqual(2, c2);
			assertEqual(2, list.get(1));
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------- 更多边界与类型 -----------------------------------------------
	private static void testRemoveHeadMiddleTailDuringForeach()
	{
		// 遍历中分别移除头部、中部、尾部元素
		// 元素避开 default(int)=0,避免与打洞标记冲突
		SafeFastDeepList<int> list = new();
		list.add(10);
		list.add(20);
		list.add(30);
		list.add(40);
		list.add(50);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(5, count);
			list.remove(10); // 头
			list.remove(30); // 中
			list.remove(50); // 尾
		}
		// compact 后保留 20,40
		assertEqual(2, list.count());
		assertEqual(20, list.get(0));
		assertEqual(40, list.get(1));
	}
	private static void testHoleAtHeadCompactsToFront()
	{
		// 头部打洞后 compact,元素前移到最前面
		SafeFastDeepList<int> list = new();
		list.add(100);
		list.add(200);
		list.add(300);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			list.remove(100); // 头部打洞
			assertEqual(default(int), list.get(0));
		}
		assertEqual(2, list.count());
		assertEqual(200, list.get(0));
		assertEqual(300, list.get(1));
	}
	private static void testMultipleValueTypes()
	{
		// 多种值类型
		SafeFastDeepList<bool> boolList = new();
		boolList.add(true);
		boolList.add(false);
		using (var reader = new SafeFastDeepListReader<bool>(boolList, out var bc))
		{
			assertEqual(2, bc);
			assertTrue(boolList.get(0));
			assertFalse(boolList.get(1));
		}

		SafeFastDeepList<float> floatList = new();
		floatList.add(1.5f);
		floatList.add(2.5f);
		using (var reader = new SafeFastDeepListReader<float>(floatList, out var fc))
		{
			assertEqual(2, fc);
			assertEqual(1.5f, floatList.get(0));
			assertEqual(2.5f, floatList.get(1));
		}
	}
	private static void testClassObjectType()
	{
		// ClassObject 引用类型
		SafeFastDeepList<TestSafeFastClassObj> list = new();
		list.add(new TestSafeFastClassObj { mValue = 10 });
		list.add(new TestSafeFastClassObj { mValue = 20 });
		using (var reader = new SafeFastDeepListReader<TestSafeFastClassObj>(list, out var count))
		{
			assertEqual(2, count);
			assertEqual(10, list.get(0).mValue);
			assertEqual(20, list.get(1).mValue);
		}
	}
	private static void testGetOnEmptyDuringForeach()
	{
		// 空列表遍历中 get 返回 default
		SafeFastDeepList<int> list = new();
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(0, count);
			assertEqual(default(int), list.get(0));
			assertEqual(default(int), list.get(-1));
		}
	}
	private static void testCountConsistencyDuringModify()
	{
		// 遍历中修改后 count 的一致性(标记删除期间 count 不变,dispose 后减少)
		SafeFastDeepList<int> list = new();
		for (int i = 0; i < 4; ++i)
		{
			list.add(i);
		}
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(4, count);
			list.remove(0);
			assertEqual(4, list.count()); // 标记删除,count 不变
			list.add(50);
			assertEqual(5, list.count()); // 新增会真实增加 count
		}
		// dispose 后:移除 1 个洞,新增 1 个 → 4
		assertEqual(4, list.count());
		assertEqual(1, list.get(0));
		assertEqual(2, list.get(1));
		assertEqual(3, list.get(2));
		assertEqual(50, list.get(3));
	}
	private static void testRepeatedPassesConsistency()
	{
		// 多次交替增删遍历后,状态保持一致
		SafeFastDeepList<int> list = new();
		for (int pass = 0; pass < 5; ++pass)
		{
			using (var reader = new SafeFastDeepListReader<int>(list, out var count))
			{
				for (int i = 0; i < count; ++i)
				{
					if (list.get(i) == pass)
					{
						list.remove(pass);
					}
				}
				list.add(pass * 10);
			}
		}
		assertEqual(5, list.count());
	}
	private static void testResetThenRebuildAndReader()
	{
		// reset 后重建,再遍历
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.resetProperty();
		assertEqual(0, list.count());
		list.add(7);
		list.add(8);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(2, count);
			assertEqual(7, list.get(0));
			assertEqual(8, list.get(1));
		}
	}
	private static void testStartForeachPairedManualCount()
	{
		// 手动 startForeach/endForeach 配对,count 手动获取,与 Reader 等价
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		list.startForeach();
		int count = list.count();
		int sum = 0;
		for (int i = 0; i < count; ++i)
		{
			sum += list.get(i);
		}
		list.endForeach();
		assertEqual(6, sum);
		assertEqual(3, list.count());
		assertFalse(list.isForeaching());
	}
	private static void testReaderOverflowIndexGetDefault()
	{
		// Reader 遍历中 get 越界返回 default
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertEqual(2, count);
			assertEqual(default(int), list.get(999));
			assertEqual(default(int), list.get(-5));
			assertEqual(1, list.get(0));
		}
	}
	private static void testRemoveReturnValueDuringForeach()
	{
		// 遍历中 remove 的返回值:已存在返回 true,不存在返回 false
		SafeFastDeepList<int> list = new();
		list.add(1);
		list.add(2);
		using (var reader = new SafeFastDeepListReader<int>(list, out var count))
		{
			assertTrue(list.remove(1));
			assertFalse(list.remove(99)); // 不存在
			assertEqual(default(int), list.get(0)); // 1 被打洞
		}
		assertEqual(1, list.count());
		assertEqual(2, list.get(0));
	}
}

// 用于引用类型泛型测试,模拟 GameEventRegisteInfo,带实例方法 call
public class TestSafeFastClass
{
	public int mValue;
	public TestSafeFastClass(int value) { mValue = value; }
	// 实例方法,供 null 条件调用 list.get(i)?.call(...)
	public void call() { }
}

// ClassObject 引用类型
public class TestSafeFastClassObj : ClassObject
{
	public int mValue;
	public void setValue(int value) { mValue = value; }
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = 0;
	}
}
