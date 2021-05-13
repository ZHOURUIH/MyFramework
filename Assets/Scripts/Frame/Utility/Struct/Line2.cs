using System;
using System.Collections.Generic;
using UnityEngine;

public struct Line2
{
	public Vector2 mStart;
	public Vector2 mEnd;
	public Line2(Vector2 start, Vector2 end)
	{
		mStart = start;
		mEnd = end;
	}
	public Line3 toLine3()
	{
		return new Line3(mStart, mEnd);
	}
}