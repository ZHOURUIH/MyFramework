using UnityEngine;
using static TestAssert;

// Vector2Short 穷举测试
public static class Vector2ShortTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testGetHashCode();
		testToVec2();
		testToVec2Int();
		testZero();
		testMinMax();
	}

	private static void testConstructor()
	{
		Vector2Short v = new(10, 20);
		assertEqual((short)10, v.x, "x");
		assertEqual((short)20, v.y, "y");

		Vector2Short v2 = new(-5, -10);
		assertEqual((short)(-5), v2.x, "负数 x");
		assertEqual((short)(-10), v2.y, "负数 y");
	}

	private static void testEquals()
	{
		Vector2Short a = new(100, 200);
		Vector2Short b = new(100, 200);
		assertTrue(a.Equals(b), "相等");

		Vector2Short c = new(100, 201);
		assertFalse(a.Equals(c), "不等");
	}

	private static void testGetHashCode()
	{
		Vector2Short a = new(1, 2);
		Vector2Short b = new(1, 2);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "相等对象应一致");

		int expected = ((ushort)(short)1 << 16) | (ushort)(short)2;
		assertEqual(expected, a.GetHashCode(), "实现");
	}

	private static void testToVec2()
	{
		Vector2Short v = new(30, 40);
		Vector2 result = v.toVec2();
		assertEqual(30.0f, result.x, "x");
		assertEqual(40.0f, result.y, "y");
	}

	private static void testToVec2Int()
	{
		Vector2Short v = new(50, 60);
		Vector2Int result = v.toVec2Int();
		assertEqual(50, result.x, "x");
		assertEqual(60, result.y, "y");
	}

	private static void testZero()
	{
		Vector2Short v = new(0, 0);
		assertEqual((short)0, v.x);
		assertEqual((short)0, v.y);
		Vector2 f = v.toVec2();
		assertEqual(0.0f, f.x);
		assertEqual(0.0f, f.y);
	}

	private static void testMinMax()
	{
		Vector2Short vMin = new(short.MinValue, short.MinValue);
		assertEqual(short.MinValue, vMin.x);
		assertEqual(short.MinValue, vMin.y);

		Vector2Short vMax = new(short.MaxValue, short.MaxValue);
		assertEqual(short.MaxValue, vMax.x);
		assertEqual(short.MaxValue, vMax.y);
	}

}
