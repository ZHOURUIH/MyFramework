using System;
using UnityEngine;
using static MathUtility;

// 2D的点
public struct Point : IEquatable<Point>
{
	public int x;	// x
	public int y;	// y
	public Point(int xx, int yy)
	{
		x = xx;
		y = yy;
	}
	public Point(Vector2Int vec)
	{
		x = vec.x;
		y = vec.y;
	}
	public int toIndex(int width)
	{
		return y * width + x;
	}
	public static Point fromIndex(int index, int width)
	{
		return new(index % width, divideInt(index, width));
	}
	public bool Equals(Point obj)
	{
		return x == obj.x && y == obj.y;
	}
	public override int GetHashCode()
	{
		return (x << 0xFFFF) & y;
	}
}