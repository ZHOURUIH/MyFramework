using System;
using UnityEngine;

// short类型的Vector2
public struct Vector2Short : IEquatable<Vector2Short>
{
	public short x;		// x分量
	public short y;     // y分量
	public Vector2Short(short xx, short yy)
	{
		x = xx;
		y = yy;
	}
	public bool Equals(Vector2Short other) { return x == other.x && y == other.y; }
	public override int GetHashCode() { return (ushort)x << 16 | (ushort)y; }
	public Vector2 toVec2() { return new(x, y); }
	public Vector2Int toVec2Int() { return new(x, y); }
}