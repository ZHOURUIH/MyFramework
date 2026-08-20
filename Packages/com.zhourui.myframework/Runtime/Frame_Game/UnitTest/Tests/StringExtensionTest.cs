using static TestAssert;

// Frame_Game 精简层 StringExtension 测试(纯字符串逻辑)
public static class StringExtensionTest
{
	public static void Run()
	{
		testIsEmpty();
		testStartString();
		testStartStringEdge();
		testRemoveStartCount();
		testRemoveStartCountEdge();
		testStartWith();
		testStartWithCaseInsensitive();
		testEndString();
		testEndWith();
		testEndWithCaseInsensitive();
		testRemoveAll();
		testEnsurePrefix();
		testAddEndSlash();
		testRightToLeft();
		testRemoveStartString();
		testRemoveStartStringCaseInsensitive();
	}

	// isEmpty
	static void testIsEmpty()
	{
		assertTrue(((string)null).isEmpty(), "null isEmpty");
		assertTrue("".isEmpty(), "空串 isEmpty");
		assertFalse("a".isEmpty(), "非空 not isEmpty");
	}

	// startString 截取前 N 位
	static void testStartString()
	{
		assertEqual("abc", "abcdef".startString(3), "前 3 位");
		assertEqual("", "abcdef".startString(0), "0 位空串");
	}

	// startString 边界(null/负)
	static void testStartStringEdge()
	{
		assertNull(((string)null).startString(2), "null 返回 null");
		assertEqual("abc", "abc".startString(-1), "负长度返回原串");
	}

	// removeStartCount 移除前 N 位
	static void testRemoveStartCount()
	{
		assertEqual("cdef", "abcdef".removeStartCount(2), "移除前 2 位");
	}

	// removeStartCount 边界(0/负/null)
	static void testRemoveStartCountEdge()
	{
		assertEqual("abc", "abc".removeStartCount(0), "0 不移除");
		assertEqual("abc", "abc".removeStartCount(-3), "负不移除");
		assertNull(((string)null).removeStartCount(1), "null 返回 null");
	}

	// startWith
	static void testStartWith()
	{
		assertTrue("abcdef".startWith("abc"), "匹配开头");
		assertFalse("abcdef".startWith("bcd"), "不匹配");
		assertFalse("ab".startWith("abc"), "短于 pattern 返回 false");
	}

	// startWith 大小写不敏感
	static void testStartWithCaseInsensitive()
	{
		assertTrue("ABCDEF".startWith("abc", false), "不敏感匹配");
		assertFalse("ABCDEF".startWith("abc"), "敏感不匹配");
	}

	// endString 截取后 N 位
	static void testEndString()
	{
		assertEqual("ef", "abcdef".endString(2), "后 2 位");
		assertEqual("abcdef", "abcdef".endString(10), "超过长度返回原串");
		assertNull(((string)null).endString(2), "null 返回 null");
	}

	// endWith
	static void testEndWith()
	{
		assertTrue("abcdef".endWith("def"), "匹配结尾");
		assertFalse("abcdef".endWith("cde"), "不匹配");
		assertFalse("ab".endWith("abc"), "短于 pattern 返回 false");
	}

	// endWith 大小写不敏感
	static void testEndWithCaseInsensitive()
	{
		assertTrue("ABCDEF".endWith("def", false), "不敏感匹配");
		assertFalse("ABCDEF".endWith("def"), "敏感不匹配");
	}

	// removeAll 移除所有指定字符
	static void testRemoveAll()
	{
		assertEqual("bcd", "abacad".removeAll('a'), "移除所有 a");
		assertEqual("abc", "abc".removeAll('x'), "无匹配不变");
	}

	// ensurePrefix 保证前缀
	static void testEnsurePrefix()
	{
		assertEqual("preabc", "abc".ensurePrefix("pre"), "无前缀添加");
		assertEqual("preabc", "preabc".ensurePrefix("pre"), "有前缀不变");
	}

	// addEndSlash
	static void testAddEndSlash()
	{
		assertEqual("a/b/", "a/b".addEndSlash(), "无结尾斜杠添加");
		assertEqual("a/b/", "a/b/".addEndSlash(), "有结尾斜杠不变");
		assertEqual("", "".addEndSlash(), "空串不变");
	}

	// rightToLeft 反斜杠转斜杠
	static void testRightToLeft()
	{
		assertEqual("a/b/c", "a\\b\\c".rightToLeft(), "反斜杠转斜杠");
		assertNull(((string)null).rightToLeft(), "null 返回 null");
	}

	// removeStartString 移除开头匹配
	static void testRemoveStartString()
	{
		assertEqual("def", "abcdef".removeStartString("abc"), "移除开头");
		assertEqual("abcdef", "abcdef".removeStartString("xyz"), "不匹配不变");
		assertEqual("abc", "abc".removeStartString(null), "null pattern 不变");
	}

	// removeStartString 大小写不敏感(移除后保留原大小写)
	static void testRemoveStartStringCaseInsensitive()
	{
		assertEqual("DEF", "ABCDEF".removeStartString("abc", false), "不敏感移除保留原大小写");
		assertEqual("ABCDEF", "ABCDEF".removeStartString("abc"), "敏感不移除");
	}
}
