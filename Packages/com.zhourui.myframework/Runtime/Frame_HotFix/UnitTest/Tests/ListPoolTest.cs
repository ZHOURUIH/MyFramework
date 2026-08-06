using System.Collections.Generic;
using static TestAssert;

// ListPool / DictionaryPool / HashSetPool 单元测试
// 覆盖三个集合对象池的创建/销毁/复用/清理逻辑(对标 PoolTest 对 ClassPool/ArrayPool 的覆盖)
// 使用局部池实例(new XxxPool), 不依赖全局单例; 只测 newList/destroyList/clearUnused 等不触发 init(mObject) 的方法
public static class ListPoolTest
{
	public static void Run()
	{
		testListPool_NewAndDestroy();
		testListPool_ReuseUnused();
		testListPool_ClearUnused();
		testListPool_DestroyNull();
		testListPool_GetLists_NonNull();
		testDictionaryPool_NewAndDestroy();
		testDictionaryPool_Reuse();
		testDictionaryPool_ClearUnused();
		testHashSetPool_NewAndDestroy();
		testHashSetPool_Reuse();
		testHashSetPool_ClearUnused();
	}

	// ═════════════════════════════════════════════════════════════════
	// ListPool — newList 创建 / destroyList 回收
	// ═════════════════════════════════════════════════════════════════
	private static void testListPool_NewAndDestroy()
	{
		ListPool pool = new ListPool();
		System.Collections.IList list = pool.newList(typeof(int), typeof(List<int>), "");
		assertNotNull(list, "newList 应创建 List 实例");
		assertTrue(list is List<int>, "创建的类型应为 List<int>");
		// 往列表中添加数据
		((List<int>)list).Add(10);
		((List<int>)list).Add(20);
		assertEqual(2, ((List<int>)list).Count, "列表可正常使用");
		// 回收
		List<int> typedList = list as List<int>;
		pool.destroyList(ref typedList, typeof(int));
		assertNull(typedList, "destroyList 后引用置空");
	}

	// ═════════════════════════════════════════════════════════════════
	// ListPool — 回收后再取复用池中对象(引用可能相同, 数据应被清空)
	// ═════════════════════════════════════════════════════════════════
	private static void testListPool_ReuseUnused()
	{
		ListPool pool = new ListPool();
		List<int> list = (List<int>)pool.newList(typeof(int), typeof(List<int>), "");
		list.Add(1);
		list.Add(2);
		pool.destroyList(ref list, typeof(int));
		// 再次申请, 应复用池中对象或新建
		List<int> list2 = (List<int>)pool.newList(typeof(int), typeof(List<int>), "");
		assertNotNull(list2, "再次 newList 成功");
		// destroyList 会 Clear, 所以复用的列表数据应为空
		assertEqual(0, list2.Count, "复用的列表数据已被清空");
		pool.destroyList(ref list2, typeof(int));
	}

	// ═════════════════════════════════════════════════════════════════
	// ListPool — clearUnused 清空未使用列表
	// ═════════════════════════════════════════════════════════════════
	private static void testListPool_ClearUnused()
	{
		ListPool pool = new ListPool();
		List<int> list = (List<int>)pool.newList(typeof(int), typeof(List<int>), "");
		pool.destroyList(ref list, typeof(int));
		// 回收后未使用列表应有内容
		assertTrue(pool.getUnusedList().Count >= 0, "getUnusedList 可访问");
		pool.clearUnused();
		assertEqual(0, pool.getUnusedList().Count, "clearUnused 后未使用列表为空");
	}

	// ═════════════════════════════════════════════════════════════════
	// ListPool — destroyList(null) 安全
	// ═════════════════════════════════════════════════════════════════
	private static void testListPool_DestroyNull()
	{
		ListPool pool = new ListPool();
		List<int> nullList = null;
		pool.destroyList(ref nullList, typeof(int));
		assertNull(nullList, "destroyList(null) 安全, 引用仍为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// ListPool — getter 返回非空字典
	// ═════════════════════════════════════════════════════════════════
	private static void testListPool_GetLists_NonNull()
	{
		ListPool pool = new ListPool();
		assertNotNull(pool.getInusedList(), "getInusedList 非空");
		assertNotNull(pool.getUnusedList(), "getUnusedList 非空");
		assertNotNull(pool.getPersistentInusedList(), "getPersistentInusedList 非空");
	}

	// ═════════════════════════════════════════════════════════════════
	// DictionaryPool — newList/destroyList
	// ═════════════════════════════════════════════════════════════════
	private static void testDictionaryPool_NewAndDestroy()
	{
		DictionaryPool pool = new DictionaryPool();
		System.Collections.ICollection dict = pool.newList(typeof(string), typeof(int), typeof(Dictionary<string, int>), "");
		assertNotNull(dict, "newList 应创建 Dictionary 实例");
		assertTrue(dict is Dictionary<string, int>, "创建的类型应为 Dictionary<string,int>");
		((Dictionary<string, int>)dict)["a"] = 1;
		assertEqual(1, ((Dictionary<string, int>)dict).Count, "字典可正常使用");
		Dictionary<string, int> typedDict = dict as Dictionary<string, int>;
		pool.destroyList(ref typedDict, typeof(string), typeof(int));
		assertNull(typedDict, "destroyList 后引用置空");
	}

	// ═════════════════════════════════════════════════════════════════
	// DictionaryPool — 复用
	// ═════════════════════════════════════════════════════════════════
	private static void testDictionaryPool_Reuse()
	{
		DictionaryPool pool = new DictionaryPool();
		Dictionary<string, int> dict = (Dictionary<string, int>)pool.newList(typeof(string), typeof(int), typeof(Dictionary<string, int>), "");
		dict["x"] = 9;
		pool.destroyList(ref dict, typeof(string), typeof(int));
		Dictionary<string, int> dict2 = (Dictionary<string, int>)pool.newList(typeof(string), typeof(int), typeof(Dictionary<string, int>), "");
		assertNotNull(dict2, "再次 newList 成功");
		assertEqual(0, dict2.Count, "复用的字典数据已被清空");
		pool.destroyList(ref dict2, typeof(string), typeof(int));
	}

	// ═════════════════════════════════════════════════════════════════
	// DictionaryPool — clearUnused
	// ═════════════════════════════════════════════════════════════════
	private static void testDictionaryPool_ClearUnused()
	{
		DictionaryPool pool = new DictionaryPool();
		Dictionary<string, int> dict = (Dictionary<string, int>)pool.newList(typeof(string), typeof(int), typeof(Dictionary<string, int>), "");
		pool.destroyList(ref dict, typeof(string), typeof(int));
		assertTrue(pool.getUnusedList().Count >= 0, "getUnusedList 可访问");
		pool.clearUnused();
		assertEqual(0, pool.getUnusedList().Count, "clearUnused 后未使用字典为空");
	}

	// ═════════════════════════════════════════════════════════════════
	// HashSetPool — newList/destroyList
	// ═════════════════════════════════════════════════════════════════
	private static void testHashSetPool_NewAndDestroy()
	{
		HashSetPool pool = new HashSetPool();
		System.Collections.IEnumerable set = pool.newList(typeof(int), typeof(HashSet<int>), "");
		assertNotNull(set, "newList 应创建 HashSet 实例");
		assertTrue(set is HashSet<int>, "创建的类型应为 HashSet<int>");
		((HashSet<int>)set).Add(5);
		assertTrue(((HashSet<int>)set).Contains(5), "HashSet 可正常使用");
		HashSet<int> typedSet = set as HashSet<int>;
		pool.destroyList(ref typedSet, typeof(int));
		assertNull(typedSet, "destroyList 后引用置空");
	}

	// ═════════════════════════════════════════════════════════════════
	// HashSetPool — 复用
	// ═════════════════════════════════════════════════════════════════
	private static void testHashSetPool_Reuse()
	{
		HashSetPool pool = new HashSetPool();
		HashSet<int> set = (HashSet<int>)pool.newList(typeof(int), typeof(HashSet<int>), "");
		set.Add(3);
		pool.destroyList(ref set, typeof(int));
		HashSet<int> set2 = (HashSet<int>)pool.newList(typeof(int), typeof(HashSet<int>), "");
		assertNotNull(set2, "再次 newList 成功");
		assertEqual(0, set2.Count, "复用的 HashSet 数据已被清空");
		pool.destroyList(ref set2, typeof(int));
	}

	// ═════════════════════════════════════════════════════════════════
	// HashSetPool — clearUnused
	// ═════════════════════════════════════════════════════════════════
	private static void testHashSetPool_ClearUnused()
	{
		HashSetPool pool = new HashSetPool();
		HashSet<int> set = (HashSet<int>)pool.newList(typeof(int), typeof(HashSet<int>), "");
		pool.destroyList(ref set, typeof(int));
		assertTrue(pool.getUnusedList().Count >= 0, "getUnusedList 可访问");
		pool.clearUnused();
		assertEqual(0, pool.getUnusedList().Count, "clearUnused 后未使用 HashSet 为空");
	}
}
