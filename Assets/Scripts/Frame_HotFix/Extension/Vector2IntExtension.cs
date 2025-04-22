using UnityEngine;

public static class Vector2IntExtension
{
	public static Vector3 toVec3(this Vector2Int vec) { return new(vec.x, vec.y); }
}