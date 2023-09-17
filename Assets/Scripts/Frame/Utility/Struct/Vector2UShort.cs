using System;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2UShort : IEquatable<Vector2UShort>
{
	public ushort x;
	public ushort y;
	public Vector2UShort(ushort xx, ushort yy)
	{
		x = xx;
		y = yy;
	}
	public bool Equals(Vector2UShort other)
	{
		return x == other.x && y == other.y;
	}
	public static implicit operator Vector2Int(Vector2UShort v)
	{
		return new Vector2Int(v.x, v.y);
	}
	public static implicit operator Vector2(Vector2UShort v)
	{
		return new Vector2(v.x, v.y);
	}
}