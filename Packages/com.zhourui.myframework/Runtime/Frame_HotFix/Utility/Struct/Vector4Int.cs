using System;

// int类型的Vector4
public struct Vector4Int : IEquatable<Vector4Int>
{
	public int x;        // x分量
	public int y;        // y分量
	public int z;        // y分量
	public int w;        // y分量
	public static Vector4Int zero;
	public Vector4Int(int xx, int yy, int zz, int ww)
	{
		x = xx;
		y = yy;
		z = zz;
		w = ww;
	}
	public bool Equals(Vector4Int other) { return x == other.x && y == other.y && z == other.z && w == other.w; }
	public override int GetHashCode() { return x << 48 | y << 32 | z << 16 | w; }
}