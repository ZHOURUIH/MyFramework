using UnityEngine;

// Vector2Int扩展方法,提供对Vector2Int的便捷操作
public static class Vector2IntExtension
{
	public static Vector3 toVec3(this Vector2Int vec) { return new(vec.x, vec.y); }
}