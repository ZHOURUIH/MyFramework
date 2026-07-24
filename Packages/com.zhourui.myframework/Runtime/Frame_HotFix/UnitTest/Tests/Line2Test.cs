using UnityEngine;
using static TestAssert;

// Line2 单元测试
public static class Line2Test
{
	public static void Run()
	{
		testConstructorNormal();
		testConstructorVertical();
		testConstructorHorizontal();
		testGetDirection();
		testLength();
		testToLine3();
		testGetPointYOnLine();
		testGetPointXOnLine();
		testGetPointXOnLineVertical();
		testGetPointXOnLineHorizontal();
	}

	private static void testConstructorNormal()
	{
		Line2 line = new(new Vector2(0, 0), new Vector2(10, 10));
		assertEqual(new Vector2(0, 0), line.mStart, "start");
		assertEqual(new Vector2(10, 10), line.mEnd, "end");
		assertTrue(line.mHasK, "斜率存在");
		assertEqual(1.0f, line.mK, 0.001f, "斜率=1");
		assertEqual(0.0f, line.mB, 0.001f, "截距=0");
	}

	private static void testConstructorVertical()
	{
		Line2 line = new(new Vector2(5, 0), new Vector2(5, 10));
		assertFalse(line.mHasK, "垂直线 mHasK=false");
		assertEqual(0.0f, line.mK, 0.001f, "垂直线 k=0");
		assertEqual(0.0f, line.mB, 0.001f, "垂直线 b=0");
	}

	private static void testConstructorHorizontal()
	{
		Line2 line = new(new Vector2(0, 3), new Vector2(10, 3));
		assertTrue(line.mHasK, "水平线 mHasK=true");
		assertEqual(0.0f, line.mK, 0.001f, "水平线 k=0");
		assertEqual(3.0f, line.mB, 0.001f, "水平线 b=3");
	}

	private static void testGetDirection()
	{
		Line2 line = new(new Vector2(1, 2), new Vector2(4, 6));
		assertEqual(new Vector2(3, 4), line.getDirection(), "direction");
	}

	private static void testLength()
	{
		Line2 line = new(new Vector2(0, 0), new Vector2(3, 4));
		assertEqual(5.0f, line.length(), 0.001f, "length 3-4-5");
	}

	private static void testToLine3()
	{
		Line2 line = new(new Vector2(1, 2), new Vector2(3, 4));
		Line3 line3 = line.toLine3();
		assertEqual(new Vector3(1, 2, 0), line3.mStart, "start");
		assertEqual(new Vector3(3, 4, 0), line3.mEnd, "end");
	}

	private static void testGetPointYOnLine()
	{
		Line2 line = new(new Vector2(0, 0), new Vector2(10, 10));
		bool ret = line.getPointYOnLine(5.0f, out float y);
		assertTrue(ret, "有斜率返回true");
		assertEqual(5.0f, y, 0.001f, "x=5 -> y=5");

		ret = line.getPointYOnLine(0.0f, out y);
		assertTrue(ret, "x=0");
		assertEqual(0.0f, y, 0.001f, "x=0 -> y=0");
	}

	private static void testGetPointXOnLine()
	{
		Line2 line = new(new Vector2(0, 0), new Vector2(10, 10));
		bool ret = line.getPointXOnLine(5.0f, out float x);
		assertTrue(ret, "有斜率返回true");
		assertEqual(5.0f, x, 0.001f, "y=5 -> x=5");
	}

	private static void testGetPointXOnLineVertical()
	{
		Line2 line = new(new Vector2(5, 0), new Vector2(5, 10));
		bool ret = line.getPointXOnLine(7.0f, out float x);
		assertTrue(ret, "垂直线返回true");
		assertEqual(5.0f, x, 0.001f, "垂直线 x=5");
	}

	private static void testGetPointXOnLineHorizontal()
	{
		Line2 line = new(new Vector2(0, 3), new Vector2(10, 3));
		bool ret = line.getPointXOnLine(3.0f, out float x);
		assertFalse(ret, "水平线返回false");
		assertEqual(0.0f, x, 0.001f, "水平线 x=0");
	}
}
