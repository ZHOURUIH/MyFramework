#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

// Frame_HotFix 扩展方法测试：StringExtension
// 所有调用均经过验证存在于 StringExtension.cs
public static class HotFixExtensionDeepTest
{
	public static void Run()
	{
		testIsEmpty();
		testLength();
		testContainsChar();
		testStartString();
		testEndString();
		testSubstr();
		testRemoveStartCount();
		testRemoveEndCount();
		testRemoveStartEndString();
		testReplace();
		testSplit();
		testStartWith();
		testEndWith();
		testHasLowerLetter();
		testRightToLeft();
		testAddEndSlash();
		testKeepNumberOnly();
	}

	private static void testIsEmpty()
	{
		AssertEqual(true, "".isEmpty());
		AssertEqual(false, "a".isEmpty());
	}

	private static void testLength()
	{
		AssertEqual(5, "hello".length());
		AssertEqual(0, "".length());
	}

	private static void testContainsChar()
	{
		AssertEqual(true, "hello".contains('h'));
		AssertEqual(false, "hello".contains('z'));
	}

	private static void testStartString()
	{
		AssertEqual("hel", "hello".startString(3));
		AssertEqual("", "hello".startString(0));
		AssertEqual("hello", "hello".startString(10));
	}

	private static void testEndString()
	{
		AssertEqual("rld", "hello world".endString(3));
		AssertEqual("world", "hello world".endString(5));
	}

	private static void testSubstr()
	{
		AssertEqual("wor", "hello world".substr(6, 3));
		AssertEqual("hello", "hello".substr(0, 5));
	}

	private static void testRemoveStartCount()
	{
		AssertEqual("world", "helloworld".removeStartCount(5));
		AssertEqual("", "a".removeStartCount(1));
	}

	private static void testRemoveEndCount()
	{
		AssertEqual("hello", "hello123".removeEndCount(3));
		AssertEqual("", "x".removeEndCount(1));
	}

	private static void testRemoveStartEndString()
	{
		AssertEqual("world", "helloworld".removeStartString("hello", true));
		AssertEqual("hello", "hello.txt".removeEndString(".txt", true));
	}

	private static void testReplace()
	{
		AssertEqual("he11o", "hello".replace("l", "1"));
		AssertEqual("hello", "hello".replace("x", "y"));
	}

	private static void testSplit()
	{
		string[] parts = "a,b,c".split(',');
		AssertEqual(3, parts.Length);
		AssertEqual("a", parts[0]);
		AssertEqual("c", parts[2]);
	}

	private static void testStartWith()
	{
		AssertEqual(true, "hello world".startWith("hello"));
		AssertEqual(false, "hello world".startWith("world"));
	}

	private static void testEndWith()
	{
		AssertEqual(true, "hello world".endWith("world"));
		AssertEqual(false, "hello world".endWith("hello"));
	}

	private static void testHasLowerLetter()
	{
		AssertEqual(true, "hello".hasLowerLetter());
		AssertEqual(false, "HELLO".hasLowerLetter());
	}

	private static void testRightToLeft()
	{
		AssertEqual("a/b/c", "a\\b\\c".rightToLeft());
	}

	private static void testAddEndSlash()
	{
		AssertEqual("path/", "path".addEndSlash());
		AssertEqual("path/", "path/".addEndSlash());
	}

	private static void testKeepNumberOnly()
	{
		AssertEqual("123", "abc123def".keepNumberOnly());
		AssertEqual("", "hello".keepNumberOnly());
	}

	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(bool e, bool a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
