using UnityEngine;

// 数学相关工具函数,所有与数学计算相关的函数都在这里
public class MathUtility
{
	public static Vector3 round(Vector3 value)
	{
		value.x = Mathf.RoundToInt(value.x);
		value.y = Mathf.RoundToInt(value.y);
		value.z = Mathf.RoundToInt(value.z);
		return value;
	}
	public static Vector2 multiVector2(Vector2 v1, Vector2 v2) { return new(v1.x * v2.x, v1.y * v2.y); }
	public static Vector3 multiVector3(Vector3 v1, Vector3 v2) { return new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
}