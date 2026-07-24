using UnityEngine;
using static TestAssert;

// Rect3 单元测试
public static class Rect3Test
{
	public static void Run()
	{
		testConstructor();
		testToRect();
	}

	private static void testConstructor()
	{
		Rect3 rect = new(new Vector3(10, 20, 30), Vector3.up, Vector3.forward, 100f, 50f);
		assertEqual(new Vector3(10, 20, 30), rect.mCenter, "center");
		assertEqual(Vector3.up, rect.mUp, "up");
		assertEqual(Vector3.forward, rect.mNormal, "normal");
		assertEqual(100f, rect.mWidth, "width");
		assertEqual(50f, rect.mHeight, "height");
	}

	private static void testToRect()
	{
		Rect3 rect = new(new Vector3(10, 0, 20), Vector3.up, Vector3.forward, 100f, 50f);
		Rect r = rect.toRect();
		assertEqual(-40.0f, r.x, 0.001f, "x");
		assertEqual(-5.0f, r.y, 0.001f, "y");
		assertEqual(100.0f, r.width, 0.001f, "width");
		assertEqual(50.0f, r.height, 0.001f, "height");
	}
}
