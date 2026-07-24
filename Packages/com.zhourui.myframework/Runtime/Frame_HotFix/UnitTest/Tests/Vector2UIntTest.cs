using UnityEngine;
using static TestAssert;

// Vector2UInt 单元测试
public static class Vector2UIntTest
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
		Vector2UInt v = new(10u, 20u);
		assertEqual(10u, v.x, "x");
		assertEqual(20u, v.y, "y");

		Vector2UInt v2 = new(0u, uint.MaxValue);
		assertEqual(0u, v2.x, "0 x");
		assertEqual(uint.MaxValue, v2.y, "MaxValue y");
	}

	private static void testEquals()
	{
		Vector2UInt a = new(100u, 200u);
		Vector2UInt b = new(100u, 200u);
		assertTrue(a.Equals(b), "相等");

		Vector2UInt c = new(100u, 201u);
		assertFalse(a.Equals(c), "不等");
	}

	private static void testGetHashCode()
	{
		Vector2UInt a = new(1u, 2u);
		Vector2UInt b = new(1u, 2u);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "相等对象应一致");

		int expected = (int)(1u << 16 | 2u);
		assertEqual(expected, a.GetHashCode(), "实现");
	}

	private static void testToVec2()
	{
		Vector2UInt v = new(30u, 40u);
		Vector2 result = v.toVec2();
		assertEqual(30.0f, result.x, "x");
		assertEqual(40.0f, result.y, "y");
	}

	private static void testToVec2Int()
	{
		Vector2UInt v = new(50u, 60u);
		Vector2Int result = v.toVec2Int();
		assertEqual(50, result.x, "x");
		assertEqual(60, result.y, "y");
	}
}
