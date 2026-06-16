using System.Collections.Generic;
using static TestAssert;

// Scope 系列在没有对象池/框架实例时必须退化为普通 new，并且 Dispose 不应抛异常。
public static class ScopeFallbackTest
{
	public static void Run()
	{
		testByteArrayScope();
		testListScope();
		testMultiListScope();
		testDifferentTypeListScope();
		testDictionaryScope();
		testHashSetScope();
		testClassScope();
		testMyStringBuilderScope();
	}
	private static void testByteArrayScope()
	{
		byte[] bytes;
		using (new ByteArrayScope(out bytes, 4))
		{
			assertNotNull(bytes, "ByteArrayScope 应返回数组");
			assertEqual(4, bytes.Length, "ByteArrayScope 数组长度");
			bytes[0] = 7;
			assertEqual((byte)7, bytes[0], "ByteArrayScope 数组可写");
		}
	}
	private static void testListScope()
	{
		List<int> list;
		using (new ListScope<int>(out list))
		{
			assertNotNull(list, "ListScope 应返回 List");
			assertEqual(0, list.Count, "ListScope 默认空列表");
			list.Add(1);
		}
		using (new ListScope<int>(out list, new List<int>{ 1, 2, 3 }))
		{
			assertEqual(3, list.Count, "ListScope List 初始化数量");
			assertEqual(2, list[1], "ListScope List 初始化内容");
		}
		using (new ListScope<int>(out list, new[]{ 4, 5 }))
		{
			assertEqual(2, list.Count, "ListScope array 初始化数量");
			assertEqual(5, list[1], "ListScope array 初始化内容");
		}
	}
	private static void testMultiListScope()
	{
		using (new ListScope2<int>(out var a, out var b))
		{
			assertNotNull(a); 
			assertNotNull(b);
			a.Add(1); 
			b.Add(2);
			assertEqual(1, a[0]); 
			assertEqual(2, b[0]);
		}
		using (new ListScope3<int>(out var a, out var b, out var c))
		{
			assertNotNull(a); 
			assertNotNull(b); 
			assertNotNull(c);
			a.Add(1); 
			b.Add(2); 
			c.Add(3);
			assertEqual(3, c[0]);
		}
		using (new ListScope4<int>(out var a, out var b, out var c, out var d))
		{
			assertNotNull(a); 
			assertNotNull(b); 
			assertNotNull(c); 
			assertNotNull(d);
			d.Add(4);
			assertEqual(4, d[0]);
		}
	}
	private static void testDifferentTypeListScope()
	{
		using (new ListScope2T<int, string>(out var a, out var b))
		{
			a.Add(1); 
			b.Add("b");
			assertEqual(1, a[0]); 
			assertEqual("b", b[0]);
		}
		using (new ListScope3T<int, string, float>(out var a, out var b, out var c))
		{
			a.Add(1); 
			b.Add("b"); 
			c.Add(3.5f);
			assertEqual(3.5f, c[0]);
		}
		using (new ListScope4T<int, string, float, bool>(out var a, out var b, out var c, out var d))
		{
			a.Add(1); 
			b.Add("b"); 
			c.Add(3.5f); 
			d.Add(true);
			assertTrue(d[0]);
		}
	}
	private static void testDictionaryScope()
	{
		Dictionary<string, int> dic;
		using (new DicScope<string, int>(out dic))
		{
			assertNotNull(dic, "DicScope 应返回 Dictionary");
			dic.Add("a", 1);
			assertEqual(1, dic["a"]);
		}
	}
	private static void testHashSetScope()
	{
		HashSet<int> set;
		using (new HashSetScope<int>(out set, new List<int>{ 1, 2, 2 }))
		{
			assertNotNull(set, "HashSetScope 应返回 HashSet");
			assertEqual(2, set.Count, "HashSetScope List 初始化去重");
			assertTrue(set.Contains(1));
		}
		using (new HashSetScope<int>(out set, new HashSet<int>{ 3, 4 }))
		{
			assertEqual(2, set.Count, "HashSetScope HashSet 初始化数量");
			assertTrue(set.Contains(4));
		}
		using (new HashSetScope2<int>(out var a, out var b))
		{
			a.Add(1); 
			b.Add(2);
			assertTrue(a.Contains(1)); 
			assertTrue(b.Contains(2));
		}
	}
	private static void testClassScope()
	{
		TestClassObj obj;
		using (new ClassScope<TestClassObj>(out obj))
		{
			assertNotNull(obj, "ClassScope 应返回对象");
			obj.mCustomData = 9;
			assertEqual(9, obj.mCustomData);
		}
		using (new ClassScope2<TestClassObj>(out var a, out var b))
		{
			assertNotNull(a); 
			assertNotNull(b);
			assertFalse(a.Equals(b), "ClassScope2 应返回两个不同实例");
		}
	}
	private static void testMyStringBuilderScope()
	{
		using (new MyStringBuilderScope(out var str))
		{
			assertNotNull(str, "MyStringBuilderScope 应返回对象");
			str.add("abc");
			assertEqual("abc", str.ToString());
		}
		using (new MyStringBuilderScope2(out var a, out var b))
		{
			assertNotNull(a); 
			assertNotNull(b);
			a.add("a"); 
			b.add("b");
			assertEqual("a", a.ToString());
			assertEqual("b", b.ToString());
		}
	}
}