#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;

// ArrayScope 单元测试
// 创建 scope / dispose (using)
public static class ArrayScopeTest
{
	public static void Run()
	{
		testCreateScope();
	}

	// ─── 创建作用域 ──────────────────────────────────────────────────────

	private static void testCreateScope()
	{
		using (new ArrayScope<int>(out var arr, 8))
		{
			assertNotNull(arr, "arr 不应为空");
			assert(arr.Length >= 8, "arr.Length 应为 2^n >= 8");
		}
	}
}
#endif
