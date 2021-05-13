using System;
using System.Collections.Generic;
using UnityEngine;

public struct PolygonIntersectResult
{
	public Vector2 mIntersectPoint0;// 第一个交点
	public Vector2 mIntersectPoint1;// 第二个交点
	public Line2 mLine0;			// 第一个交点所在线段
	public Line2 mLine1;			// 第二个交点所在线段
}