using UnityEngine;
using static TestAssert;

// Vector2UShort 单元测试
public static class Vector2UShortTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testGetHashCode();
		testToVec2();
		testToVec2Int();
	}

	private static void testConstructor()
	{
		Vector2UShort v = new(10, 20);
		assertEqual((ushort)10, v.x, "x");
		assertEqual((ushort)20, v.y, "y");

		Vector2UShort v2 = new(0, ushort.MaxValue);
		assertEqual((ushort)0, v2.x, "0 x");
		assertEqual(ushort.MaxValue, v2.y, "MaxValue y");
	}

	private static void testEquals()
	{
		Vector2UShort a = new(100, 200);
		Vector2UShort b = new(100, 200);
		assertTrue(a.Equals(b), "相等");

		Vector2UShort c = new(100, 201);
		assertFalse(a.Equals(c), "不等");
	}

	private static void testGetHashCode()
	{
		Vector2UShort a = new(1, 2);
		Vector2UShort b = new(1, 2);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "相等对象应一致");

		int expected = ((ushort)1 << 16) | (ushort)2;
		assertEqual(expected, a.GetHashCode(), "实现");
	}

	private static void testToVec2()
	{
		Vector2UShort v = new(30, 40);
		Vector2 result = v.toVec2();
		assertEqual(30.0f, result.x, "x");
		assertEqual(40.0f, result.y, "y");
	}

	private static void testToVec2Int()
	{
		Vector2UShort v = new(50, 60);
		Vector2Int result = v.toVec2Int();
		assertEqual(50, result.x, "x");
		assertEqual(60, result.y, "y");
	}
}
