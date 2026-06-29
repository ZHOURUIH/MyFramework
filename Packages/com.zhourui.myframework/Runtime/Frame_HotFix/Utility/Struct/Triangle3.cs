using UnityEngine;

// 3D的三角形
public struct Triangle3
{
	public Vector3 mPoint0;		// 第一个点
	public Vector3 mPoint1;     // 第二个点
	public Vector3 mPoint2;     // 第三个点
	public Triangle3(Vector3 point0, Vector3 point1, Vector3 point2)
	{
		mPoint0 = point0;
		mPoint1 = point1;
		mPoint2 = point2;
	}
}