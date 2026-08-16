using static TestAssert;

// ArrayScope 单元测试
// 创建 scope / dispose (using)
public static class ArrayScopeTest
{
	public static void Run()
	{
		testCreateScope();
		testExplicitDispose();
		testDisposeIdempotent();
		testMultipleScopes();
		testLargeCount();
		testZeroCount();
		testCreateReadWrite();
		testDisposeAfterUsing();
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

	// ─── 深度组合 ──────────────────────────────────────────────────────

	// 显式 Dispose(非 using)
	private static void testExplicitDispose()
	{
		var scope = new ArrayScope<int>(out var arr, 16);
		assertNotNull(arr, "arr 非空");
		scope.Dispose();
		// 无异常即通过
	}

	// Dispose 幂等(第二次 mValue 已 null → 安全)
	private static void testDisposeIdempotent()
	{
		var scope = new ArrayScope<int>(out var arr, 16);
		scope.Dispose();
		scope.Dispose();
		// 无异常即通过
	}

	// 多个 scope 并行分配
	private static void testMultipleScopes()
	{
		using (new ArrayScope<int>(out var a, 8))
		{
			using (new ArrayScope<int>(out var b, 16))
			{
				using (new ArrayScope<int>(out var c, 32))
				{
					assertNotNull(a, "a 非空");
					assertNotNull(b, "b 非空");
					assertNotNull(c, "c 非空");
					assert(a.Length >= 8, "a 长度");
					assert(b.Length >= 16, "b 长度");
					assert(c.Length >= 32, "c 长度");
				}
			}
		}
	}

	// 大 count 分配
	private static void testLargeCount()
	{
		using (new ArrayScope<byte>(out var arr, 65536))
		{
			assertNotNull(arr, "大数组非空");
			assert(arr.Length >= 65536, "大数组长度");
		}
	}

	// count=0 分配
	private static void testZeroCount()
	{
		using (new ArrayScope<int>(out var arr, 0))
		{
			assertNotNull(arr, "零长度数组非空");
		}
	}

	// 分配后写读
	private static void testCreateReadWrite()
	{
		using (new ArrayScope<int>(out var arr, 32))
		{
			arr[0] = 42;
			arr[1] = -7;
			assertEqual(42, arr[0], "写读 arr[0]");
			assertEqual(-7, arr[1], "写读 arr[1]");
		}
	}

	// using 结束后再次分配(池复用不残留)
	private static void testDisposeAfterUsing()
	{
		using (new ArrayScope<int>(out var arr1, 8))
		{
			arr1[0] = 123;
		}
		using (new ArrayScope<int>(out var arr2, 8))
		{
			assertNotNull(arr2, "二次分配非空");
			// 不依赖池内残留值(可能复用也可能新分配)
		}
	}
}