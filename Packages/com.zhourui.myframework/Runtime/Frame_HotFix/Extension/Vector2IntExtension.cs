using UnityEngine;

// Vector2Int扩展方法,提供对Vector2Int的便捷操作
public static class Vector2IntExtension
{
	public static Vector3 toVec3(this Vector2Int vec) { return new(vec.x, vec.y); }
	public static Vector2Int clampMax(this Vector2Int value, int min = 0) { return new(value.x.clampMax(min), value.y.clampMax(min)); }
	public static Vector2Int clampMin(this Vector2Int value, int min = 0) { return new(value.x.clampMin(min), value.y.clampMin(min)); }
	public static float getAngle(this Vector2Int vec, ANGLE radian = ANGLE.RADIAN)
	{
		return new Vector3(vec.x, 0.0f, vec.y).getAngle(radian);
	}
	public static int intPosToIndex(this Vector2Int pos, int width) { return pos.x + pos.y * width; }
	public static Vector2Int abs(this Vector2Int value) { return new(value.x.abs(), value.y.abs()); }
}