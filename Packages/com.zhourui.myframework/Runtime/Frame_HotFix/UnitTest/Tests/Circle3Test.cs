using UnityEngine;
using static TestAssert;

// Circle3 单元测试
public static class Circle3Test
{
	public static void Run()
	{
		testConstructor();
		testConstructorZero();
	}

	private static void testConstructor()
	{
		Circle3 circle = new(new Vector3(1, 2, 3), 5.0f);
		assertEqual(new Vector3(1, 2, 3), circle.mCenter, "center");
		assertEqual(5.0f, circle.mRadius, "radius");
	}

	private static void testConstructorZero()
	{
		Circle3 zero = new(Vector3.zero, 0.0f);
		assertEqual(Vector3.zero, zero.mCenter, "zero center");
		assertEqual(0.0f, zero.mRadius, "zero radius");
	}
}
