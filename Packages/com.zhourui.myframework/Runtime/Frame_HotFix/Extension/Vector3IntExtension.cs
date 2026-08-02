using UnityEngine;

// Vector3Int扩展方法
public static class Vector3IntExtension
{
	public static Vector3Int abs(this Vector3Int value) { return new(value.x.abs(), value.y.abs(), value.z.abs()); }
	public static Vector3Int clampMin(this Vector3Int value, int min = 0)
	{
		return new(value.x.clampMin(min), value.y.clampMin(min), value.z.clampMin(min));
	}
	public static Vector3Int clampMax(this Vector3Int value, int min = 0)
	{
		return new(value.x.clampMax(min), value.y.clampMax(min), value.z.clampMax(min));
	}
}