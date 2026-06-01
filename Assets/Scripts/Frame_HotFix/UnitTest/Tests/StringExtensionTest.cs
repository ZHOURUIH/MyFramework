#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using static TestAssert;

// StringExtension 所有扩展方法测试（46 个方法）
public static class StringExtensionTest
{
	public static void Run()
	{
		testBasicChecks();
		testRangeMethods();
		testSubstringMethods();
		testRemoveMethods();
		testStartEndWith();
		testReplaceMethods();
		testEnsureAndTrim();
	}

	private static void testBasicChecks()
	{
		assertEqual(0, ((string)null).length(), "null.length=0");
		assertEqual(4, "test".length(), "test.length=4");
		assertTrue(((string)null).isEmpty(), "null.isEmpty");
		assertTrue("".isEmpty(), "empty.isEmpty");
		assertFalse("a".isEmpty(), "'a' not empty");
		assertTrue("hello".contains('e'), "contains char true");
		assertFalse("hello".contains('z'), "contains char false");
	}

	private static void testRangeMethods()
	{
		// range 基本切分
		assertEqual("hel", "hello".range(0, 3), "range(0,3)");
		assertEqual("", "hello".range(3, 3), "range(3,3)=empty");
		assertEqual("llo", "hello".rangeFromFirstToEnd('l'), "rangeFromFirstToEnd");
		assertEqual("ello", "hello".rangeFromFirstToEndExcept('h'), "rangeFromFirstToEndExcept");
		assertEqual("llo", "hello".rangeFromFirstInclude('l'), "rangeFromFirstInclude");

		// rangeToFirst
		assertEqual("he", "hello".rangeToFirst('l'), "rangeToFirst l");
		assertEqual("hello", "hello.world".rangeToLast('.'), "rangeToLast .");
		assertEqual("hello.", "hello.world".rangeToLastInclude('.'), "rangeToLastInclude .");

		// rangeBetweenKeyToKey
		assertEqual("lo.wo", "hello.world".rangeBetweenKeyToKey('l', 'r'), "rangeBetweenKeyToKey char,char");
		assertEqual("test", "prefix[test]suffix".rangeBetweenKeyToKey("[", "]"), "rangeBetweenKeyToKey string,string");
	}

	private static void testSubstringMethods()
	{
		assertEqual("wor", "hello.world".substr(6, 3), "substr(6,3)=wor");
		assertEqual("orld", "hello.world".endString(4), "endString(4)=orld");
		assertEqual("hell", "hello.world".startString(4), "startString(4)=hell");
		assertEqual("hello.w", "hello.world".removeEndCount(4), "removeEndCount(4)=hello.w");
		assertEqual("world", "hello.world".removeStartCount(6), "removeStartCount(6)=world");
	}

	private static void testRemoveMethods()
	{
		// removeStartString
		assertEqual("world", "helloworld".removeStartString("hello"), "removeStartString prefix");
		assertEqual("helloworld", "helloworld".removeStartString("xyz"), "removeStartString no match");

		// removeEndString
		assertEqual("hello", "helloworld".removeEndString("world"), "removeEndString suffix");
		assertEqual("", "hello".removeEndString("hello"), "removeEndString whole");

		// removeStartAll
		assertEqual("c", "aaac".removeStartAll('a'), "removeStartAll");
		assertEqual("", "aaaa".removeStartAll('a'), "removeStartAll all");
		assertEqual("cd", "abcd".removeStartAll('a', 'b'), "removeStartAll multi chars");

		// removeEndAll
		assertEqual("a", "accc".removeEndAll('c'), "removeEndAll");
		assertEqual("a", "abccc".removeEndAll('b', 'c'), "removeEndAll multi");

		// removeAll by string/char
		assertEqual("heo", "hello".removeAll("ll"), "removeAll string");
		assertEqual("hello", "h.e.l.l.o".removeAll('.'), "removeAll char");
		assertEqual("abc", "a,b,c".removeAll(new char[] { ',' }), "removeAll char array");

		// removeStartEmpty / removeEndEmpty / removeAllEmpty
		assertEqual("a b", " a b".removeStartEmpty(), "removeStartEmpty");
		assertEqual("a b", "a b ".removeEndEmpty(), "removeEndEmpty");
		assertEqual("ab", "a b".removeAllEmpty(), "removeAllEmpty");
	}

	private static void testStartEndWith()
	{
		assertTrue("hello".startWith("he"), "startWith true");
		assertFalse("hello".startWith("HE"), "startWith case");
		assertTrue("hello".startWith("HE", false), "startWith insensitive");
		assertTrue("hello".endWith("lo"), "endWith true");
		assertFalse("hello".endWith("LO"), "endWith case");
		assertTrue("hello".endWith("LO", false), "endWith insensitive");
	}

	private static void testReplaceMethods()
	{
		assertEqual("abxyzfg", "abcdefg".replace(2, 5, "xyz"), "replace by index");
		assertEqual("hello,world!", "hello world!".replace(" ", ","), "replace string");
		assertEqual("a-a-a", "a b a b a".replaceAll(" b ", "-"), "replaceAll");
		assertEqual("x x x", "a a a".replaceAll('a', 'x'), "replaceAll char");
	}

	private static void testEnsureAndTrim()
	{
		assertEqual("http://test", "test".ensurePrefix("http://"), "ensurePrefix added");
		assertEqual("http://test", "http://test".ensurePrefix("http://"), "ensurePrefix already");
		assertEqual("file.txt", "file".ensureSuffix(".txt"), "ensureSuffix added");
		assertEqual("file.txt", "file.txt".ensureSuffix(".txt"), "ensureSuffix already");
	}
}
#endif
