#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// MyStringBuilder 基本操作测试
public static class MyStringBuilderExtendedTest2
{
	public static void Run()
	{
		testAdd();
		testClear();
		testToString();
	}

	private static void testAdd()
	{
		var sb = new MyStringBuilder();
		sb.add("hello");
		sb.add(" ");
		sb.add("world");
		AssertEqual("hello world", sb.ToString());
	}

	private static void testClear()
	{
		var sb = new MyStringBuilder();
		sb.add("test");
		sb.clear();
		AssertEqual(0, sb.Length);
	}

	private static void testToString()
	{
		var sb = new MyStringBuilder();
		sb.add("abc");
		sb.add("123");
		AssertEqual("abc123", sb.ToString());
	}

	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
