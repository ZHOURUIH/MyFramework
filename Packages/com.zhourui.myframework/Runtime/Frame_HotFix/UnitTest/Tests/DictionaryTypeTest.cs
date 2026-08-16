using static TestAssert;

public class DictionaryTypeTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testGetHashCode();
		testEqualsSelf();
		testToString();
		testHashCodeDifferentCombination();
		testKeyValueSwapNotEqual();
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

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// 自身相等
	private static void testEqualsSelf()
	{
		DictionaryType dt = new DictionaryType(typeof(int), typeof(string));
		assertTrue(dt.Equals(dt), "与自身相等");
	}

	// ToString 格式
	private static void testToString()
	{
		DictionaryType dt = new DictionaryType(typeof(int), typeof(string));
		assertEqual("System.Int32,System.String", dt.ToString(), "ToString 格式");
	}

	// 不同组合哈希码大概率不同(int+float vs int+string)
	private static void testHashCodeDifferentCombination()
	{
		DictionaryType a = new DictionaryType(typeof(int), typeof(string));
		DictionaryType b = new DictionaryType(typeof(int), typeof(float));
		assertFalse(a.GetHashCode() == b.GetHashCode(), "不同值类型哈希码不同");
	}

	// 键值类型对换 → 不相等
	private static void testKeyValueSwapNotEqual()
	{
		DictionaryType a = new DictionaryType(typeof(int), typeof(string));
		DictionaryType b = new DictionaryType(typeof(string), typeof(int));
		assertFalse(a.Equals(b), "键值对换不相等");
	}
}