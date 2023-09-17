using System;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2Short : IEquatable<Vector2Short>
{
	public short x;
	public short y;
	public Vector2Short(short xx, short yy)
	{
		x = xx;
		y = yy;
	}
	public bool Equals(Vector2Short other)
	{
		return x == other.x && y == other.y;
	}
	public static implicit operator Vector2Int(Vector2Short v)
	{
		return new Vector2Int(v.x, v.y);
	}
	public static implicit operator Vector2(Vector2Short v)
	{
		return new Vector2(v.x, v.y);
	}
}