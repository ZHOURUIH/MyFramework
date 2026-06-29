using UnityEngine;

// 3D空间中的线段
public struct Line3
{
	public Vector3 mStart;	// 起点
	public Vector3 mEnd;	// 终点
	public Line3(Vector3 start, Vector3 end)
	{
		mStart = start;
		mEnd = end;
	}
	public Line2 toLine2IgnoreY()
	{
		return new(new(mStart.x, mStart.z), new(mEnd.x, mEnd.z));
	}
	public Line2 toLine2IgnoreX()
	{
		return new(new(mStart.z, mStart.y), new(mEnd.z, mEnd.y));
	}
}