using UnityEngine;

// 相交信息
public struct TriangleIntersectResult3
{
	public Vector3 mIntersectPoint; // 交点
	public Vector3 mLinePoint0;     // 交点所在的三角形的一条边的起点
	public Vector3 mLinePoint1;     // 交点所在的三角形的一条边的终点
}