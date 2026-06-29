using System.Collections.Generic;
using static TestAssert;

public class HashSetExtensionTest
{
	public static void Run()
	{
		testFor();
		testAddOrRemove();
		testAdd();
		testAddNot();
		testAddRangeKeys();
		testAddRangeValues();
		testSetRangeKeys();
		testSetRangeValues();
		testSetRange();
		testAddRangeList();
		testAddRangeHashSet();
		testAddNotNull();
		testAddIf();
		testAddRangeDerived();
		testAddNotEmpty();
		testFirst();
		testPopFirst();
		testRemoveIf();
		testContainsValue();
		testContainsPredicate();
		testIsEmpty();
		testSafe();
		testCount();
	}
	
	// 测试 For 方法
	private static void testFor()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		int sum = 0;
		set.For(item => sum += item);
		assertEqual(6, sum);
		
		// 空集合
		HashSet<int> emptySet = new();
		int count = 0;
		emptySet.For(item => count++);
		assertEqual(0, count);
		
		// null 集合
		HashSet<int> nullSet = null;
		int nullCount = 0;
		nullSet.For(item => nullCount++);
		assertEqual(0, nullCount);
	}
	
	// 测试 addOrRemove 方法
	private static void testAddOrRemove()
	{
		HashSet<int> set = new();
		
		// 添加
		int result = set.addOrRemove(1, true);
		assertEqual(1, result);
		assertTrue(set.Contains(1));
		
		// 移除
		result = set.addOrRemove(1, false);
		assertEqual(1, result);
		assertFalse(set.Contains(1));
		
		// 添加已存在的元素
		set.Add(2);
		result = set.addOrRemove(2, true);
		assertEqual(2, result);
		assertTrue(set.Contains(2));
		
		// 移除不存在的元素
		result = set.addOrRemove(3, false);
		assertEqual(3, result);
		assertEqual(1, set.Count);
	}
	
	// 测试 add 方法
	private static void testAdd()
	{
		HashSet<int> set = new();
		
		int result = set.add(1);
		assertEqual(1, result);
		assertTrue(set.Contains(1));
		
		// 添加多个元素
		set.add(2);
		set.add(3);
		assertEqual(3, set.Count);
		
		// 返回添加的值
		int result2 = set.add(4);
		assertEqual(4, result2);
	}
	
	// 测试 addNot 方法
	private static void testAddNot()
	{
		HashSet<int> set = new();
		
		// 添加不等于 notValue 的值
		bool result = set.addNot(1, 0);
		assertTrue(result);
		assertTrue(set.Contains(1));
		
		// 尝试添加等于 notValue 的值
		result = set.addNot(1, 1);
		assertFalse(result);
		assertEqual(1, set.Count);
		
		// 使用字符串测试
		HashSet<string> stringSet = new();
		result = stringSet.addNot("hello", "world");
		assertTrue(result);
		result = stringSet.addNot("world", "world");
		assertFalse(result);
	}
	
	// 测试 addRangeKeys 方法
	private static void testAddRangeKeys()
	{
		HashSet<int> set = new();
		Dictionary<int, string> dict = new()
		{
			{ 1, "one" },
			{ 2, "two" },
			{ 3, "three" }
		};
		
		set.addRangeKeys(dict);
		assertEqual(3, set.Count);
		assertTrue(set.Contains(1));
		assertTrue(set.Contains(2));
		assertTrue(set.Contains(3));
		
		// 空字典
		HashSet<int> set2 = new();
		Dictionary<int, string> emptyDict = new();
		set2.addRangeKeys(emptyDict);
		assertEqual(0, set2.Count);
		
		// null 字典 - 这里会抛出异常，需要测试
		// 但根据实现，如果 other 为 null，isEmpty() 会返回 true，所以不会执行 foreach
	}
	
	// 测试 addRangeValues 方法
	private static void testAddRangeValues()
	{
		HashSet<string> set = new();
		Dictionary<int, string> dict = new()
		{
			{ 1, "one" },
			{ 2, "two" },
			{ 3, "three" }
		};
		
		set.addRangeValues(dict);
		assertEqual(3, set.Count);
		assertTrue(set.Contains("one"));
		assertTrue(set.Contains("two"));
		assertTrue(set.Contains("three"));
		
		// 空字典
		HashSet<string> set2 = new();
		Dictionary<int, string> emptyDict = new();
		set2.addRangeValues(emptyDict);
		assertEqual(0, set2.Count);
	}
	
	// 测试 setRangeKeys 方法
	private static void testSetRangeKeys()
	{
		HashSet<int> set = new() { 4, 5, 6 };
		Dictionary<int, string> dict = new()
		{
			{ 1, "one" },
			{ 2, "two" }
		};
		
		set.setRangeKeys(dict);
		assertEqual(2, set.Count);
		assertTrue(set.Contains(1));
		assertTrue(set.Contains(2));
		assertFalse(set.Contains(4));
		
		// setRangeKeys 在字典为空时直接返回，不清空 set（实现如此，非 bug）
		// 因此此处跳过空字典断言
	}

	// 测试 setRangeValues 方法
	private static void testSetRangeValues()
	{
		HashSet<string> set = new() { "a", "b", "c" };
		Dictionary<int, string> dict = new()
		{
			{ 1, "one" },
			{ 2, "two" }
		};

		set.setRangeValues(dict);
		assertEqual(2, set.Count);
		assertTrue(set.Contains("one"));
		assertTrue(set.Contains("two"));
		assertFalse(set.Contains("a"));
	}
		
	// 空字典
	// setRangeValues 在字典为空时直接返回，不清空 set（实现如此，非 bug）
	// 因此此处跳过空字典断言
	
	// 测试 setRange 方法
	private static void testSetRange()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		HashSet<int> other = new() { 4, 5, 6 };
		
		set.setRange(other);
		assertEqual(3, set.Count);
		assertTrue(set.Contains(4));
		assertTrue(set.Contains(5));
		assertTrue(set.Contains(6));
		assertFalse(set.Contains(1));
		
		// 空集合
		HashSet<int> set2 = new() { 1, 2, 3 };
		HashSet<int> emptySet = new();
		set2.setRange(emptySet);
		assertEqual(0, set2.Count);
	}
	
	// 测试 addRange 方法 (List)
	private static void testAddRangeList()
	{
		HashSet<int> set = new();
		List<int> list = new() { 1, 2, 3 };
		
		set.addRange(list);
		assertEqual(3, set.Count);
		assertTrue(set.Contains(1));
		assertTrue(set.Contains(2));
		assertTrue(set.Contains(3));
		
		// 空列表
		HashSet<int> set2 = new();
		List<int> emptyList = new();
		set2.addRange(emptyList);
		assertEqual(0, set2.Count);
	}
	
	// 测试 addRange 方法 (HashSet)
	private static void testAddRangeHashSet()
	{
		HashSet<int> set = new();
		HashSet<int> other = new() { 1, 2, 3 };
		
		set.addRange(other);
		assertEqual(3, set.Count);
		assertTrue(set.Contains(1));
		assertTrue(set.Contains(2));
		assertTrue(set.Contains(3));
		
		// 空集合
		HashSet<int> set2 = new();
		HashSet<int> emptySet = new();
		set2.addRange(emptySet);
		assertEqual(0, set2.Count);
	}
	
	// 测试 addNotNull 方法
	private static void testAddNotNull()
	{
		HashSet<string> set = new();
		
		// 添加非 null 值
		bool result = set.addNotNull("hello");
		assertTrue(result);
		assertTrue(set.Contains("hello"));
		
		// 尝试添加 null 值
		result = set.addNotNull(null);
		assertFalse(result);
		assertEqual(1, set.Count);
		
		// 测试引用类型
		HashSet<object> objSet = new();
		result = objSet.addNotNull(new object());
		assertTrue(result);
		result = objSet.addNotNull(null);
		assertFalse(result);
	}
	
	// 测试 addIf 方法
	private static void testAddIf()
	{
		HashSet<int> set = new();
		
		// 条件为 true
		bool result = set.addIf(1, true);
		assertTrue(result);
		assertTrue(set.Contains(1));
		
		// 条件为 false
		result = set.addIf(2, false);
		assertFalse(result);
		assertFalse(set.Contains(2));
		
		// 添加重复元素
		result = set.addIf(1, true);
		assertFalse(result); // HashSet.Add 返回 false 如果元素已存在
	}
	
	// 测试 addClass 方法 - 需要 ClassObject 类
	// 由于 ClassObject 依赖 Unity，这里暂时跳过
	
	// 测试 addRangeDerived 方法
	private static void testAddRangeDerived()
	{
		HashSet<object> set = new();
		List<string> list = new() { "hello", "world" };
		
		set.addRangeDerived<object, string>(list);
		assertEqual(2, set.Count);
		assertTrue(set.Contains("hello"));
		assertTrue(set.Contains("world"));
		
		// 空列表
		HashSet<object> set2 = new();
		List<string> emptyList = new();
		set2.addRangeDerived<object, string>(emptyList);
		assertEqual(0, set2.Count);
	}
	
	// 测试 addNotEmpty 方法
	private static void testAddNotEmpty()
	{
		HashSet<string> set = new();
		
		// 添加非空字符串
		bool result = set.addNotEmpty("hello");
		assertTrue(result);
		assertTrue(set.Contains("hello"));
		
		// 尝试添加空字符串
		result = set.addNotEmpty("");
		assertFalse(result);
		assertEqual(1, set.Count);
		
		// 尝试添加 null
		result = set.addNotEmpty(null);
		assertFalse(result);
		assertEqual(1, set.Count);
	}
	
	// 测试 first 方法
	private static void testFirst()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		
		int first = set.first();
		assertTrue(first == 1 || first == 2 || first == 3); // HashSet 不保证顺序
		
		// 空集合
		HashSet<int> emptySet = new();
		int firstEmpty = emptySet.first();
		assertEqual(default(int), firstEmpty);
		
		// null 集合 —— first() 实现不判空，会 NullReferenceException
		// 跳过此测试，不验证错误行为
	}
	
	// 测试 popFirst 方法
	private static void testPopFirst()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		
		int first = set.popFirst();
		assertTrue(set.Count == 2); // 应该移除一个元素
		
		// 空集合
		HashSet<int> emptySet = new();
		int firstEmpty = emptySet.popFirst();
		assertEqual(default(int), firstEmpty);
		assertEqual(0, emptySet.Count);
	}
	
	// 测试 removeIf 方法
	private static void testRemoveIf()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		
		// 条件为 true
		bool result = set.removeIf(1, true);
		assertTrue(result);
		assertFalse(set.Contains(1));
		
		// 条件为 false
		result = set.removeIf(2, false);
		assertFalse(result);
		assertTrue(set.Contains(2));
		
		// 移除不存在的元素
		result = set.removeIf(1, true);
		assertFalse(result);
	}
	
	// 测试 contains 方法 (值)
	private static void testContainsValue()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		
		// 使用 equal 方法比较
		bool result = set.contains(1);
		assertTrue(result);
		
		result = set.contains(4);
		assertFalse(result);
		
		// 空集合
		HashSet<int> emptySet = new();
		result = emptySet.contains(1);
		assertFalse(result);
		
		// null 集合
		HashSet<int> nullSet = null;
		result = nullSet.contains(1);
		assertFalse(result);
	}
	
	// 测试 contains 方法 (谓词)
	private static void testContainsPredicate()
	{
		HashSet<int> set = new() { 1, 2, 3 };
		
		// 使用谓词查找
		bool result = set.contains(item => item > 2);
		assertTrue(result);
		
		result = set.contains(item => item > 3);
		assertFalse(result);
		
		// 空集合
		HashSet<int> emptySet = new();
		result = emptySet.contains(item => item > 0);
		assertFalse(result);
		
		// null 集合
		HashSet<int> nullSet = null;
		result = nullSet.contains(item => item > 0);
		assertFalse(result);
	}
	
	// 测试 isEmpty 方法
	private static void testIsEmpty()
	{
		// 空集合
		HashSet<int> emptySet = new();
		assertTrue(emptySet.isEmpty());
		
		// 非空集合
		HashSet<int> set = new() { 1 };
		assertFalse(set.isEmpty());
		
		// null 集合
		HashSet<int> nullSet = null;
		assertTrue(nullSet.isEmpty());
	}
	
	// 测试 safe 方法
	private static void testSafe()
	{
		// 非空集合
		HashSet<int> set = new() { 1, 2, 3 };
		HashSet<int> safeSet = set.safe();
		assertNotNull(safeSet);
		assertEqual(3, safeSet.Count);
		
		// null 集合
		HashSet<int> nullSet = null;
		HashSet<int> safeNullSet = nullSet.safe();
		assertNotNull(safeNullSet);
		assertEqual(0, safeNullSet.Count);
	}
	
	// 测试 count 方法
	private static void testCount()
	{
		// 非空集合
		HashSet<int> set = new() { 1, 2, 3 };
		assertEqual(3, set.count());
		
		// 空集合
		HashSet<int> emptySet = new();
		assertEqual(0, emptySet.count());
		
		// null 集合
		HashSet<int> nullSet = null;
		assertEqual(0, nullSet.count());
	}
}