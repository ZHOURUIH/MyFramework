using UnityEngine;
using static TestAssert;

// Point 单元测试
public static class PointTest
{
	public static void Run()
	{
		testConstructor();
		testConstructorFromVector2Int();
		testToIndex();
		testFromIndex();
		testEquals();
		testGetHashCode();
	}

	private static void testConstructor()
	{
		Point p = new(3, 5);
		assertEqual(3, p.x, "x");
		assertEqual(5, p.y, "y");
	}

	private static void testConstructorFromVector2Int()
	{
		Point p = new(new Vector2Int(7, 9));
		assertEqual(7, p.x, "Vector2Int x");
		assertEqual(9, p.y, "Vector2Int y");
	}

	private static void testToIndex()
	{
		Point p = new(2, 3);
		assertEqual(3 * 10 + 2, p.toIndex(10), "toIndex");

		Point p2 = new(0, 0);
		assertEqual(0, p2.toIndex(10), "toIndex zero");
	}

	private static void testFromIndex()
	{
		Point p = Point.fromIndex(32, 10);
		assertEqual(2, p.x, "fromIndex x");
		assertEqual(3, p.y, "fromIndex y");

		Point p2 = Point.fromIndex(0, 10);
		assertEqual(0, p2.x, "fromIndex zero x");
		assertEqual(0, p2.y, "fromIndex zero y");
	}

	private static void testEquals()
	{
		Point a = new(1, 2);
		Point b = new(1, 2);
		assertTrue(a.Equals(b), "相等");
		assertTrue(b.Equals(a), "相等对称");

		Point c = new(3, 4);
		assertFalse(a.Equals(c), "不等");
	}

	private static void testGetHashCode()
	{
		Point a = new(1, 2);
		Point b = new(1, 2);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "相等对象应一致");

		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2, "幂等");
	}
}
