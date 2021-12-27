using System;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle2
{
	public Vector2 mPoint0;
	public Vector2 mPoint1;
	public Vector2 mPoint2;
	public Triangle2(Vector2 point0, Vector2 point1, Vector2 point2)
	{
		mPoint0 = point0;
		mPoint1 = point1;
		mPoint2 = point2;
	}
	public Triangle3 toTriangle3()
	{
		return new Triangle3(mPoint0, mPoint1, mPoint2);
	}
}