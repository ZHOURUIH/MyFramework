using System.Collections.Generic;
using static TestAssert;

// Frame_Game 精简层 ListExtension 测试
public static class ListExtensionTest
{
	public static void Run()
	{
		testAddUniqueNew();
		testAddUniqueDuplicate();
		testAddNotEmptyValid();
		testAddNotEmptyEmpty();
		testSetRangeReplace();
		testSetRangeNullClears();
		testIsEmpty();
		testCount();
	}

	// 新值加入返回 true
	static void testAddUniqueNew()
	{
		List<int> list = new() { 1, 2 };
		assertTrue(list.addUnique(3), "新值加入");
		assertEqual(3, list.Count, "数量 3");
	}

	// 重复值不加入返回 false
	static void testAddUniqueDuplicate()
	{
		List<int> list = new() { 1, 2 };
		assertFalse(list.addUnique(2), "重复值不加入");
		assertEqual(2, list.Count, "数量不变");
	}

	// 非空字符串加入
	static void testAddNotEmptyValid()
	{
		List<string> list = new();
		assertTrue(list.addNotEmpty("abc"), "非空加入");
		assertEqual(1, list.Count, "数量 1");
	}

	// 空/空串不加入
	static void testAddNotEmptyEmpty()
	{
		List<string> list = new();
		assertFalse(list.addNotEmpty(""), "空串不加入");
		assertFalse(list.addNotEmpty(null), "null 不加入");
		assertEqual(0, list.Count, "数量 0");
	}

	// setRange 覆盖原列表
	static void testSetRangeReplace()
	{
		List<int> list = new() { 1, 2, 3 };
		List<int> other = new() { 4, 5 };
		list.setRange(other);
		assertEqual(2, list.Count, "覆盖后数量 2");
		assertEqual(4, list[0], "首元素 4");
	}

	// setRange(null) 清空
	static void testSetRangeNullClears()
	{
		List<int> list = new() { 1, 2 };
		list.setRange(null);
		assertEqual(0, list.Count, "null 清空");
	}

	// isEmpty
	static void testIsEmpty()
	{
		List<int> empty = new();
		List<int> nonEmpty = new() { 1 };
		assertTrue(empty.isEmpty(), "空列表 isEmpty");
		assertFalse(nonEmpty.isEmpty(), "非空列表 not isEmpty");
	}

	// count
	static void testCount()
	{
		List<int> list = new() { 1, 2, 3 };
		assertEqual(3, list.count(), "count 3");
	}
}
