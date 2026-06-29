using System;
using UnityEngine;

// int类型的Vector2,因为UnityEngine的Vector2Int无法的xy分量无法作为out参数,所以自定义一个
public struct Vector2IntMy : IEquatable<Vector2IntMy>
{
	public int x;        // x分量
	public int y;        // y分量
	public Vector2IntMy(int xx, int yy)
	{
		x = xx;
		y = yy;
	}
	public bool Equals(Vector2IntMy other) { return x == other.x && y == other.y; }
	public override int GetHashCode() { return x << 16 | y; }
	public Vector2 toVec2() { return new(x, y); }
	public Vector2Int toVec2Int() { return new(x, y); }
}