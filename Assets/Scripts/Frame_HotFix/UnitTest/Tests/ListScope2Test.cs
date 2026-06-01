#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;

// ListScope2 单元测试
// 创建 scope 并验证两个列表
public static class ListScope2Test
{
	public static void Run()
	{
		testCreateScope();
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
}
#endif
