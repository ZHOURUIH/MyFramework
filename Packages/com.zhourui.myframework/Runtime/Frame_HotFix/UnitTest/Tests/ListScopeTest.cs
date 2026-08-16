using System.Collections.Generic;
using static TestAssert;

// ListScope 单元测试 — 自动从对象池获取 List<T>, Dispose 自动归还
//   ListScope<T>(out list) 构造: GameEntryBase 非 null + mListPool 可用时走池分配
//   Dispose: mListPool?.destroyList 归还(池)
// 框架环境已完全初始化(GameEntryBase/mListPool 均可用)
public static class ListScopeTest
{
	public static void Run()
	{
		testCreateScope();
		testExplicitDispose();
		testDisposeIdempotent();
		testUsingWithItems();
		testMultipleScopes();
		testLargeCapacity();
	}

	// using 创建作用域
	private static void testCreateScope()
	{
		using (new ListScope<int>(out var list))
		{
			assertNotNull(list, "list 非空");
			list.Add(42);
			assertEqual(1, list.Count, "可正常使用");
		}
	}

	// 显式 Dispose(非 using)
	private static void testExplicitDispose()
	{
		var scope = new ListScope<int>(out var list);
		assertNotNull(list, "list 非空");
		scope.Dispose();
		// 无异常即通过
	}

	// Dispose 幂等(第二次 mList 已 null → 安全)
	private static void testDisposeIdempotent()
	{
		var scope = new ListScope<int>(out var list);
		scope.Dispose();
		scope.Dispose();
		// 无异常即通过
	}

	// using 内添加/移除元素
	private static void testUsingWithItems()
	{
		using (new ListScope<string>(out var list))
		{
			list.Add("a");
			list.Add("b");
			list.Remove("a");
			assertEqual(1, list.Count, "移除后 1 个");
			assertEqual("b", list[0], "剩余 b");
		}
	}

	// 多个 scope 并行
	private static void testMultipleScopes()
	{
		using (new ListScope<int>(out var a))
		{
			using (new ListScope<int>(out var b))
			{
				a.Add(1);
				b.Add(2);
				assertEqual(1, a[0], "a 内容");
				assertEqual(2, b[0], "b 内容");
				assertFalse(ReferenceEquals(a, b), "a/b 不同实例");
			}
		}
	}

	// 大量元素使用
	private static void testLargeCapacity()
	{
		using (new ListScope<int>(out var list))
		{
			for (int i = 0; i < 1000; ++i)
			{
				list.Add(i);
			}
			assertEqual(1000, list.Count, "1000 个元素");
			assertEqual(999, list[999], "末尾元素");
		}
	}
}
