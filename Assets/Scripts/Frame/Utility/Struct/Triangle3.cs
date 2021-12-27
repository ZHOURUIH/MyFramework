using System;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle3
{
	public Vector3 mPoint0;
	public Vector3 mPoint1;
	public Vector3 mPoint2;
	public Triangle3(Vector3 point0, Vector3 point1, Vector3 point2)
	{
		mPoint0 = point0;
		mPoint1 = point1;
		mPoint2 = point2;
	}
}