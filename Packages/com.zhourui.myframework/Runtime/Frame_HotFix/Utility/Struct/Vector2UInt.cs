using System;
using UnityEngine;

// uint类型的Vector2
public struct Vector2UInt : IEquatable<Vector2UInt>
{
	public uint x;        // x分量
	public uint y;        // y分量
	public Vector2UInt(uint xx, uint yy)
	{
		x = xx;
		y = yy;
	}
	public bool Equals(Vector2UInt other) { return x == other.x && y == other.y; }
	public override int GetHashCode() { return (int)(x << 16 | y); }
	public Vector2 toVec2() { return new(x, y); }
	public Vector2Int toVec2Int() { return new((int)x, (int)y); }
}