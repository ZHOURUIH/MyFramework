using UnityEngine;

// Vector2扩展方法
public static class Vector2Extension
{
	public static Vector2 ceil(this Vector2 value) { return new(value.x.ceil(), value.y.ceil()); }
	public static bool isNaN(this Vector2 vec) { return float.IsNaN(vec.x) || float.IsNaN(vec.y); }
	public static Vector3 round(this Vector2 value) { return new(value.x.round(), value.y.round()); }
	public static Vector3 floor(this Vector2 value) { return new(value.x.floor(), value.y.floor()); }
	public static Vector3 abs(this Vector2 value) { return new(value.x.abs(), value.y.abs()); }
	// 将向量的X设置为0
	public static Vector2 resetX(this Vector2 v) { return new(0.0f, v.y); }
	// 将向量的Y设置为0
	public static Vector2 resetY(this Vector2 v) { return new(v.x, 0.0f); }
	// 将向量的X替换为指定值
	public static Vector2 replaceX(this Vector2 v, float x) { return new(x, v.y); }
	// 将向量的Y替换为指定值
	public static Vector2 replaceY(this Vector2 v, float y) { return new(v.x, y); }
	// 构造出Vector3,将向量的Z替换为指定值
	public static Vector3 replaceZ(this Vector2 v, float z) { return new(v.x, v.y, z); }
	public static bool isZero(this Vector2 vec, float precision = 0.0001f) { return vec.x.isZero(precision) && vec.y.isZero(precision); }
	public static float getLength(this Vector2 vec) { return (vec.x * vec.x + vec.y * vec.y).sqrt(); }
	public static float getSquaredLength(this Vector2 vec) { return vec.x * vec.x + vec.y * vec.y; }
	public static bool lengthLess(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y < vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLess(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y < length * length; }
	public static bool lengthLessEqual(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y <= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthLessEqual(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y <= length * length; }
	public static bool lengthGreater(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y > length * length; }
	public static bool lengthGreater(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y > vec1.x * vec1.x + vec1.y * vec1.y; }
	public static bool lengthGreaterEqual(this Vector2 vec, float length) { return vec.x * vec.x + vec.y * vec.y >= length * length; }
	public static bool lengthGreaterEqual(this Vector2 vec0, Vector2 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y >= vec1.x * vec1.x + vec1.y * vec1.y; }
	public static Vector2 setLength(this Vector2 vec, float length)
	{
		float scale = vec.getLength().inverse() * length;
		return new(vec.x * scale, vec.y * scale);
	}
	// vec0的2个分量是否都小于vec1的2个分量
	public static bool isLess(this Vector2 vec0, Vector2 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y; }
	// vec0的2个分量是否都大于vec1的2个分量
	public static bool isGreater(this Vector2 vec0, Vector2 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y; }
	public static bool isEqual(this Vector2 vec0, Vector2 vec1, float precision = 0.0001f)
	{
		return (vec0.x - vec1.x).isZero(precision) && (vec0.y - vec1.y).isZero(precision);
	}
	public static Vector2 normalize(this Vector2 vec2)
	{
		float inverseLen = vec2.getLength().inverse();
		return new(vec2.x * inverseLen, vec2.y * inverseLen);
	}
	public static Vector2 clampMin(this Vector2 value, float min = 0.0f) { return new(value.x.clampMin(min), value.y.clampMin(min)); }
	public static Vector2 clampMax(this Vector2 value, float min = 0.0f) { return new(value.x.clampMax(min), value.y.clampMax(min)); }
	public static Vector2 clampMax(this Vector2 value, Vector2 min) { return new(value.x.clampMax(min.x), value.y.clampMax(min.y)); }
	public static bool inRange(this Vector2 value, Vector2 point0, Vector2 point1, float precision = 0.001f)
	{
		return value.x.inRange(point0.x, point1.x, precision) &&
			   value.y.inRange(point0.y, point1.y, precision);
	}
	public static Vector2 multi(this Vector2 v1, Vector2 v2) { return new(v1.x * v2.x, v1.y * v2.y); }
	public static Vector2 divide(this Vector2 v1, Vector2 v2) { return new(v1.x.divide(v2.x), v1.y.divide(v2.y)); }
	public static Vector2 divide(this Vector2 v1, float scale) { return new(v1.x.divide(scale), v1.y.divide(scale)); }
	public static float getAngle(this Vector2 vec, ANGLE radian = ANGLE.RADIAN)
	{
		return new Vector3(vec.x, 0.0f, vec.y).getAngle(radian);
	}
	public static float dot(this Vector2 v0, Vector2 v1) { return v0.x * v1.x + v0.y * v1.y; }
}