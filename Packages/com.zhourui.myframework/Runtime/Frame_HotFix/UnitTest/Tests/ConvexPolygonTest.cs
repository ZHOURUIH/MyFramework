using UnityEngine;
using static TestAssert;

// ConvexPolygon 数据结构单元测试
public static class ConvexPolygonTest
{
	public static void Run()
	{
		testResetPropertyClearsPoints();
		testResetPropertyRegeneratesColor();
		testPointsStorable();
		testDrawNoThrow();
	}

	private static void testResetPropertyClearsPoints()
	{
		ConvexPolygon polygon = new();
		polygon.mPoints.Add(new Vector2(1, 1));
		polygon.mPoints.Add(new Vector2(2, 2));
		assertEqual(2, polygon.mPoints.Count, "添加前应有两个点");
		polygon.resetProperty();
		assertEqual(0, polygon.mPoints.Count, "reset 后应清空点列表");
	}

	private static void testResetPropertyRegeneratesColor()
	{
		ConvexPolygon polygon = new();
		Color before = polygon.mColor;
		polygon.resetProperty();
		// reset 后颜色分量应保持在 [0,1] 合法范围
		Color after = polygon.mColor;
		assertTrue(after.r >= 0f && after.r <= 1f, "reset 后 R 应在 [0,1]");
		assertTrue(after.g >= 0f && after.g <= 1f, "reset 后 G 应在 [0,1]");
		assertTrue(after.b >= 0f && after.b <= 1f, "reset 后 B 应在 [0,1]");
		// 两次 randomFloat 结果通常不同 (伪随机)
		bool likelyDifferent = !(Mathf.Abs(before.r - after.r) < 0.0001f &&
								 Mathf.Abs(before.g - after.g) < 0.0001f &&
								 Mathf.Abs(before.b - after.b) < 0.0001f);
		assertTrue(likelyDifferent, "reset 后颜色应重新随机生成");
	}

	private static void testPointsStorable()
	{
		ConvexPolygon polygon = new();
		Vector2[] pts = { new(0, 0), new(2, 0), new(2, 2), new(0, 2) };
		foreach (Vector2 p in pts)
		{
			polygon.mPoints.Add(p);
		}
		assertEqual(4, polygon.mPoints.Count, "应能存4个点");
		assertEqual(new Vector2(2, 2), polygon.mPoints[2], "点数据应正确保留");
	}

	private static void testDrawNoThrow()
	{
		ConvexPolygon polygon = new();
		polygon.mPoints.Add(new Vector2(0, 0));
		polygon.mPoints.Add(new Vector2(1, 0));
		polygon.mPoints.Add(new Vector2(1, 1));
		// draw 使用 Debug.DrawLine, 不应抛异常
		polygon.draw();
		assertTrue(true, "draw 应正常执行不抛异常");
	}
}
