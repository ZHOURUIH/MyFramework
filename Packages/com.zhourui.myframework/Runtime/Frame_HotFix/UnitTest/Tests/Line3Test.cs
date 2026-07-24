using UnityEngine;
using static TestAssert;

// Line3 单元测试
public static class Line3Test
{
	public static void Run()
	{
		testConstructor();
		testToLine2IgnoreY();
		testToLine2IgnoreX();
	}

	private static void testConstructor()
	{
		Line3 line = new(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
		assertEqual(new Vector3(1, 2, 3), line.mStart, "start");
		assertEqual(new Vector3(4, 5, 6), line.mEnd, "end");
	}

	private static void testToLine2IgnoreY()
	{
		Line3 line = new(new Vector3(1, 999, 3), new Vector3(4, 888, 6));
		Line2 line2 = line.toLine2IgnoreY();
		assertEqual(new Vector2(1, 3), line2.mStart, "start (x,z)");
		assertEqual(new Vector2(4, 6), line2.mEnd, "end (x,z)");
	}

	private static void testToLine2IgnoreX()
	{
		Line3 line = new(new Vector3(999, 2, 3), new Vector3(777, 5, 6));
		Line2 line2 = line.toLine2IgnoreX();
		assertEqual(new Vector2(3, 2), line2.mStart, "start (z,y)");
		assertEqual(new Vector2(6, 5), line2.mEnd, "end (z,y)");
	}
}
