using System;
using System.Collections.Generic;
using UnityEngine;

public struct Line3
{
	public Vector3 mStart;
	public Vector3 mEnd;
	public Line3(Vector3 start, Vector3 end)
	{
		mStart = start;
		mEnd = end;
	}
	public Line2 toLine2IgnoreY()
	{
		return new Line2(new Vector2(mStart.x, mStart.z), new Vector2(mEnd.x, mEnd.z));
	}
	public Line2 toLine2IgnoreX()
	{
		return new Line2(new Vector2(mStart.z, mStart.y), new Vector2(mEnd.z, mEnd.y));
	}
}