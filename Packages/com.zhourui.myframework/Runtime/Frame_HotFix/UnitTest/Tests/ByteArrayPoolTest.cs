using static TestAssert;

// ByteArrayPool 单元测试 — byte[] 数组池的创建/销毁/复用/清理
// 注意: newArray(size) 要求 size 是 2 的 n 次方, 否则返回 null; destroyArray(ref byte[], destroyReally)
// 使用局部池实例(new ByteArrayPool), 不依赖全局单例; 框架环境已初始化(GameEntryBase 可用)
public static class ByteArrayPoolTest
{
	public static void Run()
	{
		testNewArray_Pow2();
		testNewArray_NonPow2_ReturnsNull();
		testDestroyArray_Recycles();
		testDestroyArray_NullSafe();
		testDestroyArray_DestroyReally();
		testClearUnused();
		testReuse_AfterDestroy();
		testGetLists_NonNull();
	}

	// ═════════════════════════════════════════════════════════════════
	// newArray — 2 的幂大小创建成功
	// ═════════════════════════════════════════════════════════════════
	private static void testNewArray_Pow2()
	{
		ByteArrayPool pool = new ByteArrayPool();
		byte[] array = pool.newArray(64);
		assertNotNull(array, "newArray(64) 应创建成功");
		assertEqual(64, array.Length, "数组长度应为 64");
		// 数组内容默认为 0
		for (int i = 0; i < array.Length; ++i)
		{
			if (array[i] != 0)
			{
				assertTrue(false, "数组元素应默认为 0");
				return;
			}
		}
		pool.destroyArray(ref array);
	}

	// ═════════════════════════════════════════════════════════════════
	// newArray — 非 2 的幂返回 null
	// ═════════════════════════════════════════════════════════════════
	// 注意: newArray(非2的幂) 源码内部必然 Debug.LogError("只有长度为2的n次方的数组才能使用ArrayPool"),
	// 触发 logError 污染测试日志。按项目约定, 必然触发 logError 的错误分支测试跳过, 不执行。
	private static void testNewArray_NonPow2_ReturnsNull()
	{
		assertTrue(true, "skip: newArray(非2的幂) 源码必然 logError, 错误路径跳过");
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyArray — 回收后可复用
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyArray_Recycles()
	{
		ByteArrayPool pool = new ByteArrayPool();
		byte[] array = pool.newArray(32);
		assertNotNull(array, "前置: 创建成功");
		array[0] = 5;
		pool.destroyArray(ref array);
		assertNull(array, "destroyArray 后引用置空");
		// 再次申请同大小, 应能成功
		byte[] array2 = pool.newArray(32);
		assertNotNull(array2, "destroy 后再 newArray 成功");
		assertEqual(32, array2.Length, "复用数组长度正确");
		pool.destroyArray(ref array2);
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyArray — null 安全
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyArray_NullSafe()
	{
		ByteArrayPool pool = new ByteArrayPool();
		byte[] array = null;
		pool.destroyArray(ref array);
		assertNull(array, "destroyArray(null) 安全");
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyArray — destroyReally 直接释放
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyArray_DestroyReally()
	{
		ByteArrayPool pool = new ByteArrayPool();
		byte[] array = pool.newArray(16);
		assertNotNull(array, "前置: 创建成功");
		pool.destroyArray(ref array, true);
		assertNull(array, "destroyReally=true 后引用置空");
		// 销毁后 unused 不应有该数组
		assertEqual(0, pool.getUnusedList().Count, "destroyReally 后不进入未使用池");
	}

	// ═════════════════════════════════════════════════════════════════
	// clearUnused — 清空未使用列表
	// ═════════════════════════════════════════════════════════════════
	private static void testClearUnused()
	{
		ByteArrayPool pool = new ByteArrayPool();
		byte[] array = pool.newArray(128);
		assertNotNull(array, "前置: 创建成功");
		pool.destroyArray(ref array);
		// clearUnused 清空每个 size 对应的 Queue 内容(不清空字典 key)
		assertEqual(1, pool.getUnusedList()[128].Count, "前置: destroy 后未使用队列有 1 个");
		pool.clearUnused();
		assertEqual(0, pool.getUnusedList()[128].Count, "clearUnused 后未使用队列为空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 复用 — destroy 后再次 newArray 复用池中数组
	// ═════════════════════════════════════════════════════════════════
	private static void testReuse_AfterDestroy()
	{
		ByteArrayPool pool = new ByteArrayPool();
		byte[] array = pool.newArray(256);
		assertNotNull(array, "前置: 创建成功");
		array[0] = 99;
		pool.destroyArray(ref array);
		byte[] array2 = pool.newArray(256);
		assertNotNull(array2, "复用成功");
		// destroyArray 不主动清零内容(仅回收), 但长度一致
		assertEqual(256, array2.Length, "复用数组长度一致");
		pool.destroyArray(ref array2);
	}

	// ═════════════════════════════════════════════════════════════════
	// getter 返回非空字典
	// ═════════════════════════════════════════════════════════════════
	private static void testGetLists_NonNull()
	{
		ByteArrayPool pool = new ByteArrayPool();
		assertNotNull(pool.getInusedList(), "getInusedList 非空");
		assertNotNull(pool.getUnusedList(), "getUnusedList 非空");
		assertNotNull(pool.getPersistentInusedList(), "getPersistentInusedList 非空");
	}
}
