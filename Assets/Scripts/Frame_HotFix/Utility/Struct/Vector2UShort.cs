using System;
using UnityEngine;

// ushort类型的Vector2
public struct Vector2UShort : IEquatable<Vector2UShort>
{
	public ushort x;        // x分量
	public ushort y;        // y分量
	public Vector2UShort(ushort xx, ushort yy)
	{
		x = xx;
		y = yy;
	}
	public bool Equals(Vector2UShort other) { return x == other.x && y == other.y; }
	public override int GetHashCode() { return x << 16 | y; }
	public Vector2 toVec2() { return new(x, y); }
	public Vector2Int toVec2Int() { return new(x, y); }
}