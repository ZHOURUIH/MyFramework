using System;
using System.Collections.Generic;
using UnityEngine;

public struct TriangleIntersectResult
{
	public Vector2 mIntersectPoint; // 交点
	public Vector2 mLinePoint0;     // 交点所在的三角形的一条边的起点
	public Vector2 mLinePoint1;     // 交点所在的三角形的一条边的终点
}