#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TestAssert;

public class DictionaryTypeTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testGetHashCode();
	}

	// 测试构造函数
	private static void testConstructor()
	{
		DictionaryType dt = new DictionaryType(typeof(int), typeof(string));
		assertNotNull(dt);
	}

	// 测试 Equals 方法
	private static void testEquals()
	{
		DictionaryType dt1 = new DictionaryType(typeof(int), typeof(string));
		DictionaryType dt2 = new DictionaryType(typeof(int), typeof(string));
		DictionaryType dt3 = new DictionaryType(typeof(int), typeof(float));
		DictionaryType dt4 = new DictionaryType(typeof(string), typeof(string));

		assertTrue(dt1.Equals(dt2));
		assertFalse(dt1.Equals(dt3));
		assertFalse(dt1.Equals(dt4));
	}

	// 测试 GetHashCode 方法
	private static void testGetHashCode()
	{
		DictionaryType dt1 = new DictionaryType(typeof(int), typeof(string));
		DictionaryType dt2 = new DictionaryType(typeof(int), typeof(string));

		// 相同的类型组合应该产生相同的哈希码
		assertEqual(dt1.GetHashCode(), dt2.GetHashCode());
	}
}
#endif