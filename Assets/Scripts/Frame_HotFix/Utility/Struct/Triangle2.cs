using UnityEngine;

// 2D的三角形
public struct Triangle2
{
	public Vector2 mPoint0;		// 第一个点
	public Vector2 mPoint1;     // 第二个点
	public Vector2 mPoint2;     // 第三个点
	public Triangle2(Vector2 point0, Vector2 point1, Vector2 point2)
	{
		mPoint0 = point0;
		mPoint1 = point1;
		mPoint2 = point2;
	}
	public Triangle3 toTriangle3()
	{
		return new(mPoint0, mPoint1, mPoint2);
	}
}