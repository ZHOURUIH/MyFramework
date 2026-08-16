using static TestAssert;

// ListScope2 单元测试
// 创建 scope 并验证两个列表
public static class ListScope2Test
{
	public static void Run()
	{
		testCreateScope();
		testExplicitDispose();
		testDisposeIdempotent();
		testUseBothLists();
		testMultipleScopes();
		testLargeUse();
	}

	// ─── 创建作用域 ──────────────────────────────────────────────────────

	private static void testCreateScope()
	{
		using (new ListScope2<int>(out var list0, out var list1))
		{
			assertNotNull(list0, "list0 不应为空");
			assertNotNull(list1, "list1 不应为空");
		}
	}

	// ─── 深度组合 ──────────────────────────────────────────────────────

	// 显式 Dispose(非 using)
	private static void testExplicitDispose()
	{
		var scope = new ListScope2<int>(out var list0, out var list1);
		assertNotNull(list0, "list0 非空");
		assertNotNull(list1, "list1 非空");
		scope.Dispose();
		// 无异常即通过
	}

	// Dispose 幂等
	private static void testDisposeIdempotent()
	{
		var scope = new ListScope2<int>(out var list0, out var list1);
		scope.Dispose();
		scope.Dispose();
		// 无异常即通过
	}

	// 双列表各自独立使用
	private static void testUseBothLists()
	{
		using (new ListScope2<string>(out var list0, out var list1))
		{
			list0.Add("a");
			list1.Add("b");
			list0.Add("c");
			assertEqual(2, list0.Count, "list0 2 个");
			assertEqual(1, list1.Count, "list1 1 个");
			assertFalse(ReferenceEquals(list0, list1), "双列表不同实例");
		}
	}

	// 多 scope 并行
	private static void testMultipleScopes()
	{
		using (new ListScope2<int>(out var a0, out var a1))
		{
			using (new ListScope2<int>(out var b0, out var b1))
			{
				a0.Add(1);
				b1.Add(2);
				assertEqual(1, a0[0], "a0 内容");
				assertEqual(2, b1[0], "b1 内容");
			}
		}
	}

	// 大量元素
	private static void testLargeUse()
	{
		using (new ListScope2<int>(out var list0, out var list1))
		{
			for (int i = 0; i < 500; ++i)
			{
				list0.Add(i);
				list1.Add(i * 2);
			}
			assertEqual(500, list0.Count, "list0 500 个");
			assertEqual(500, list1.Count, "list1 500 个");
			assertEqual(998, list1[499], "list1 末尾");
		}
	}
}