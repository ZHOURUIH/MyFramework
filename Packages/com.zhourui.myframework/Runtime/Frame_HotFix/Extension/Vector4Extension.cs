using UnityEngine;

// Vector4扩展方法
public static class Vector4Extension
{
	public static Vector4 abs(this Vector4 value) { return new(value.x.abs(), value.y.abs(), value.z.abs(), value.w.abs()); }
	public static float getLength(this Vector4 vec) { return (vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w).sqrt(); }
	public static float getSquaredLength(this Vector4 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w; }
	public static bool lengthLess(this Vector4 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w < length * length; }
	public static Vector4 clampMin(this Vector4 value, float min = 0.0f)
	{
		return new(value.x.clampMin(min), value.y.clampMin(min), value.z.clampMin(min), value.w.clampMin(min));
	}
	public static Vector4 clampMax(this Vector4 value, float min = 0.0f)
	{
		return new(value.x.clampMax(min), value.y.clampMax(min), value.z.clampMax(min), value.w.clampMax(min));
	}
}