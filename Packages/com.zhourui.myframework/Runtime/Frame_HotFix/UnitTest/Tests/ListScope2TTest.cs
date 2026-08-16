using System.Collections.Generic;
using static TestAssert;

// ListScope2T 单元测试 — 双类型双列表作用域(自动从对象池获取, Dispose 归还)
public static class ListScope2TTest
{
	public static void Run()
	{
		testCreateScope();
		testUseDifferentTypes();
		testExplicitDispose();
		testDisposeIdempotent();
	}

	// 创建双类型 scope
	private static void testCreateScope()
	{
		using (new ListScope2T<int, string>(out var list0, out var list1))
		{
			assertNotNull(list0, "list0(int) 非空");
			assertNotNull(list1, "list1(string) 非空");
		}
	}

	// 双类型各自使用
	private static void testUseDifferentTypes()
	{
		using (new ListScope2T<int, string>(out var list0, out var list1))
		{
			list0.Add(42);
			list1.Add("hello");
			assertEqual(42, list0[0], "int 列表内容");
			assertEqual("hello", list1[0], "string 列表内容");
		}
	}

	// 显式 Dispose
	private static void testExplicitDispose()
	{
		var scope = new ListScope2T<int, float>(out var list0, out var list1);
		assertNotNull(list0, "list0 非空");
		assertNotNull(list1, "list1 非空");
		scope.Dispose();
		// 无异常即通过
	}

	// Dispose 幂等
	private static void testDisposeIdempotent()
	{
		var scope = new ListScope2T<int, int>(out var list0, out var list1);
		scope.Dispose();
		scope.Dispose();
		// 无异常即通过
	}
}
