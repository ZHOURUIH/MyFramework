using System.Collections.Generic;
using static TestAssert;

// Frame_Game 精简层 DictionaryExtension 测试
// 注意: set 在编辑器下对不存在的 key 会 logErrorBase, 只测已存在 key 的覆盖
public static class DictionaryExtensionTest
{
	public static void Run()
	{
		testSetExisting();
		testGetMissingDefault();
		testGetExisting();
		testAdd();
		testAddDuplicateThrows();
		testGetOrAddNewExisting();
		testGetOrAddNewCreate();
		testAddNotNullKeyNull();
		testAddNotNullKeyValid();
		testIsEmpty();
		testSetRangeReplace();
		testSetRangeNullClears();
	}

	// set 覆盖已存在 key(先 add 再 set 避免编辑器 logError)
	static void testSetExisting()
	{
		Dictionary<string, int> dic = new() { ["a"] = 1 };
		dic.set("a", 100);
		assertEqual(100, dic["a"], "set 覆盖");
	}

	// get 缺失 key → default
	static void testGetMissingDefault()
	{
		Dictionary<string, int> dic = new() { ["a"] = 1 };
		assertEqual(0, dic.get("missing"), "缺失返回 default 0");
	}

	// get 已有 key
	static void testGetExisting()
	{
		Dictionary<string, int> dic = new() { ["a"] = 42 };
		assertEqual(42, dic.get("a"), "已有 key 取值");
	}

	// add 返回 value
	static void testAdd()
	{
		Dictionary<string, int> dic = new();
		int r = dic.add("a", 7);
		assertEqual(7, r, "add 返回值");
		assertEqual(7, dic["a"], "add 后取值");
	}

	// add 重复 key → ArgumentException
	static void testAddDuplicateThrows()
	{
		Dictionary<string, int> dic = new() { ["a"] = 1 };
		bool threw = false;
		try
		{
			dic.add("a", 2);
		}
		catch (System.ArgumentException)
		{
			threw = true;
		}
		assertTrue(threw, "重复 add 抛 ArgumentException");
	}

	// getOrAddNew 已有 key
	static void testGetOrAddNewExisting()
	{
		Dictionary<string, List<int>> dic = new() { ["a"] = new List<int> { 1 } };
		var v = dic.getOrAddNew("a");
		assertEqual(1, v.Count, "已有 key 返回原值");
		assertEqual(1, dic.Count, "不新增");
	}

	// getOrAddNew 创建新值
	static void testGetOrAddNewCreate()
	{
		Dictionary<string, List<int>> dic = new();
		var v = dic.getOrAddNew("b");
		assertNotNull(v, "新建非 null");
		v.Add(5);
		assertEqual(1, dic["b"].Count, "新 key 已入字典");
	}

	// addNotNullKey(null) 不加入
	static void testAddNotNullKeyNull()
	{
		Dictionary<string, int> dic = new();
		dic.addNotNullKey(null, 1);
		assertEqual(0, dic.Count, "null key 不加入");
	}

	// addNotNullKey 有效 key 加入
	static void testAddNotNullKeyValid()
	{
		Dictionary<string, int> dic = new();
		dic.addNotNullKey("k", 9);
		assertEqual(9, dic["k"], "有效 key 加入");
	}

	// isEmpty
	static void testIsEmpty()
	{
		Dictionary<string, int> empty = new();
		Dictionary<string, int> nonEmpty = new() { ["a"] = 1 };
		assertTrue(empty.isEmpty(), "空字典 isEmpty");
		assertFalse(nonEmpty.isEmpty(), "非空 not isEmpty");
	}

	// setRange: 覆盖原字典
	static void testSetRangeReplace()
	{
		Dictionary<string, int> map = new() { ["a"] = 1, ["b"] = 2 };
		Dictionary<string, int> other = new() { ["c"] = 3 };
		map.setRange(other);
		assertEqual(1, map.Count, "覆盖后 1 项");
		assertEqual(3, map["c"], "新值");
	}

	// setRange(null): 清空
	static void testSetRangeNullClears()
	{
		Dictionary<string, int> map = new() { ["a"] = 1 };
		map.setRange(null);
		assertEqual(0, map.Count, "null 清空");
	}
}
