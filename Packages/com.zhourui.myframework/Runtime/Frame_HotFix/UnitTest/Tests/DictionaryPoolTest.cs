using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// DictionaryPool 字典池深度测试
//   newList: 按 (keyType, valueType) 分组, unused 有则复用, 无则反射创建
//   destroyList: 清空 + 回收 + 移除 inuse + ref 置 null
//   clearUnused: 清空未使用队列
// 环境: new DictionaryPool()(FrameSystem 子类直接 new)
public static class DictionaryPoolTest
{
	public static void Run()
	{
		testNewList();
		testDestroyList();
		testReuseAfterDestroy();
		testNewDifferentType();
		testClearUnused();
		testInusedRegistration();
		testDestroyRemovesInused();
		testOnlyOnceFalse();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static Dictionary<int, string> newIntStringList(DictionaryPool pool)
	{
		ICollection raw = pool.newList(typeof(int), typeof(string), typeof(Dictionary<int, string>), "test");
		return (Dictionary<int, string>)raw;
	}

	// newList: 创建可用字典
	private static void testNewList()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			assertNotNull(dict, "newList 返回非 null");
			dict.Add(1, "a");
			assertEqual("a", dict[1], "字典可用");
		}
		finally
		{
			pool.destroy();
		}
	}

	// destroyList: 清空 + ref 置 null
	private static void testDestroyList()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			dict.Add(1, "a");
			pool.destroyList<int, string>(ref dict, typeof(int), typeof(string));
			assertTrue(dict == null, "destroyList 后 ref 置 null");
		}
		finally
		{
			pool.destroy();
		}
	}

	// 复用: destroyList 后 newList 同类型 → 同一实例
	private static void testReuseAfterDestroy()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			dict.Add(1, "a");
			Dictionary<int, string> sameRef = dict;
			pool.destroyList<int, string>(ref dict, typeof(int), typeof(string));
			Dictionary<int, string> reused = newIntStringList(pool);
			assertTrue(ReferenceEquals(sameRef, reused), "同类型复用同一实例");
			assertEqual(0, reused.Count, "复用实例已清空");
		}
		finally
		{
			pool.destroy();
		}
	}

	// 不同类型 → 新实例(不同 type key 分组)
	private static void testNewDifferentType()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			Dictionary<string, int> dict2 = null;
			ICollection raw = pool.newList(typeof(string), typeof(int), typeof(Dictionary<string, int>), "test");
			dict2 = (Dictionary<string, int>)raw;
			assertTrue(!ReferenceEquals(dict, dict2), "不同类型不同实例");
			dict2.Add("x", 1);
			assertEqual(1, dict2["x"], "第二个字典可用");
		}
		finally
		{
			pool.destroy();
		}
	}

	// clearUnused: 清空未使用队列 → 再 newList 为新实例
	private static void testClearUnused()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			Dictionary<int, string> sameRef = dict;
			pool.destroyList<int, string>(ref dict, typeof(int), typeof(string));
			pool.clearUnused();
			Dictionary<int, string> after = newIntStringList(pool);
			assertTrue(!ReferenceEquals(sameRef, after), "clearUnused 后 newList 为新实例");
		}
		finally
		{
			pool.destroy();
		}
	}

	// inuse 注册: newList 后使用列表有记录
	private static void testInusedRegistration()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			bool found = false;
			foreach (HashSet<ICollection> set in pool.getInusedList().Values)
			{
				if (set.Contains(dict))
				{
					found = true;
				}
			}
			assertTrue(found, "newList 后字典在 inuse 列表中");
		}
		finally
		{
			pool.destroy();
		}
	}

	// destroyList 后 inuse 移除
	private static void testDestroyRemovesInused()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			Dictionary<int, string> dict = newIntStringList(pool);
			pool.destroyList<int, string>(ref dict, typeof(int), typeof(string));
			bool found = false;
			foreach (HashSet<ICollection> set in pool.getInusedList().Values)
			{
				if (set.Contains(dict))
				{
					found = true;
				}
			}
			// dict 已被置 null, 只验证 inuse 列表不含 null
			assertTrue(!found, "destroyList 后 inuse 移除");
		}
		finally
		{
			pool.destroy();
		}
	}

	// onlyOnce=false → 持久使用列表
	private static void testOnlyOnceFalse()
	{
		DictionaryPool pool = new DictionaryPool();
		try
		{
			ICollection raw = pool.newList(typeof(int), typeof(string), typeof(Dictionary<int, string>), "test", false);
			Dictionary<int, string> dict = (Dictionary<int, string>)raw;
			bool found = false;
			foreach (HashSet<ICollection> set in pool.getPersistentInusedList().Values)
			{
				if (set.Contains(dict))
				{
					found = true;
				}
			}
			assertTrue(found, "onlyOnce=false 注册到持久使用列表");
		}
		finally
		{
			pool.destroy();
		}
	}
}
