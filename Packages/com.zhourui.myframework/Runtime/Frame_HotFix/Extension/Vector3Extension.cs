using UnityEngine;

// Vector3扩展方法
public static class Vector3Extension
{
	public static Vector3 ceil(this Vector3 value) { return new(value.x.ceil(), value.y.ceil(), value.z.ceil()); }
	public static bool isNaN(this Vector3 vec) { return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z); }
	public static Vector3 saturate(this Vector3 value) { return new(value.x.saturate(), value.y.saturate(), value.z.saturate()); }
	public static Vector3 round(this Vector3 value) { return new(value.x.round(), value.y.round(), value.z.round()); }
	public static Vector3 floor(this Vector3 value) { return new(value.x.floor(), value.y.floor(), value.z.floor()); }
	public static Vector3 abs(this Vector3 value) { return new(value.x.abs(), value.y.abs(), value.z.abs()); }
	public static Vector3 checkFloat(this Vector3 value, int precision = 4)
	{
		return new(value.x.checkFloat(precision), value.y.checkFloat(precision), value.z.checkFloat(precision));
	}
	public static Vector3 checkInt(this Vector3 vec, float precision = 0.0001f)
	{
		return new(vec.x.checkInt(precision), vec.y.checkInt(precision), vec.z.checkInt(precision));
	}
	// 将vec的长度限定到maxLength,如果长度未超过,则不作修改
	public static Vector3 clampLength(this Vector3 vec, float maxLength)
	{
		if (vec.lengthGreater(maxLength))
		{
			return vec.normalize() * maxLength;
		}
		return vec;
	}
	// 将向量的X设置为0
	public static Vector3 resetX(this Vector3 v) { return new(0.0f, v.y, v.z); }
	// 将向量的Y设置为0
	public static Vector3 resetY(this Vector3 v) { return new(v.x, 0.0f, v.z); }
	// 将向量的Z设置为0
	public static Vector3 resetZ(this Vector3 v) { return new(v.x, v.y, 0.0f); }
	// 将向量的X替换为指定值
	public static Vector3 replaceX(this Vector3 v, float x) { return new(x, v.y, v.z); }
	// 将向量的Y替换为指定值
	public static Vector3 replaceY(this Vector3 v, float y) { return new(v.x, y, v.z); }
	// 将向量的Z替换为指定值
	public static Vector3 replaceZ(this Vector3 v, float z) { return new(v.x, v.y, z); }
	public static bool isZero(this Vector3 vec, float precision = 0.0001f)
	{
		return vec.x.isZero(precision) && vec.y.isZero(precision) && vec.z.isZero(precision);
	}
	public static float getLength(this Vector3 vec) { return (vec.x * vec.x + vec.y * vec.y + vec.z * vec.z).sqrt(); }
	public static float getLengthIgnoreY(this Vector3 vec) { return (vec.x * vec.x + vec.z * vec.z).sqrt(); }
	public static float getSquaredLength(this Vector3 vec) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z; }
	public static float getSquaredLengthIgnoreY(this Vector3 vec) { return vec.x * vec.x + vec.z * vec.z; }
	public static bool lengthLess(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z < vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLess(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z < length * length; }
	public static bool lengthLessIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z < length * length; }
	public static bool lengthLessEqual(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z <= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthLessEqual(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z <= length * length; }
	public static bool lengthLessEqualIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z <= length * length; }
	public static bool lengthGreater(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z > length * length; }
	public static bool lengthGreaterIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z > length * length; }
	public static bool lengthGreater(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z > vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqual(this Vector3 vec, float length) { return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z >= length * length; }
	public static bool lengthGreaterEqual(this Vector3 vec0, Vector3 vec1) { return vec0.x * vec0.x + vec0.y * vec0.y + vec0.z * vec0.z >= vec1.x * vec1.x + vec1.y * vec1.y + vec1.z * vec1.z; }
	public static bool lengthGreaterEqualIgnoreY(this Vector3 vec, float length) { return vec.x * vec.x + vec.z * vec.z >= length * length; }
	public static Vector3 setLength(this Vector3 vec, float length)
	{
		float scale = vec.getLength().inverse() * length;
		return new(vec.x * scale, vec.y * scale, vec.z * scale);
	}
	// vec0的3个分量是否都小于vec1的3个分量
	public static bool isLess(this Vector3 vec0, Vector3 vec1) { return vec0.x < vec1.x && vec0.y < vec1.y && vec0.z < vec1.z; }
	// vec0的3个分量是否都大于vec1的3个分量
	public static bool isGreater(this Vector3 vec0, Vector3 vec1) { return vec0.x > vec1.x && vec0.y > vec1.y && vec0.z > vec1.z; }
	public static bool isEqual(this Vector3 vec0, Vector3 vec1, float precision = 0.0001f)
	{
		return (vec0.x - vec1.x).isZero(precision) && (vec0.y - vec1.y).isZero(precision) && (vec0.z - vec1.z).isZero(precision);
	}
	public static Vector3 normalize(this Vector3 vec3)
	{
		float inverseLen = vec3.getLength().inverse();
		return new(vec3.x * inverseLen, vec3.y * inverseLen, vec3.z * inverseLen);
	}
	public static Vector3 toRadian(this Vector3 degree) { return degree * Mathf.Deg2Rad; }
	public static Vector3 toDegree(this Vector3 radian) { return radian * Mathf.Rad2Deg; }
	public static Vector3 clampMin(this Vector3 value, float min = 0.0f)
	{
		return new(value.x.clampMin(min), value.y.clampMin(min), value.z.clampMin(min));
	}
	public static Vector3 clampMax(this Vector3 value, float min = 0.0f)
	{
		return new(value.x.clampMax(min), value.y.clampMax(min), value.z.clampMax(min));
	}
	public static bool inRange(this Vector3 value, Vector3 point0, Vector3 point1, bool ignoreY = true, float precision = 0.001f)
	{
		return value.x.inRange(point0.x, point1.x, precision) &&
				(ignoreY || value.y.inRange(point0.y, point1.y, precision)) &&
				value.z.inRange(point0.z, point1.z, precision);
	}
	public static Vector3 multi(this Vector3 v1, Vector3 v2) { return new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
	public static Vector3 divide(this Vector3 v1, Vector3 v2) { return new(v1.x.divide(v2.x), v1.y.divide(v2.y), v1.z.divide(v2.z)); }
	public static Vector3 divide(this Vector3 v1, float scale) { return new(v1.x.divide(scale), v1.y.divide(scale), v1.z.divide(scale)); }
	public static Vector3 adjustRadian180(this Vector3 radian)
	{
		return new(radian.x.adjustRadian180(), radian.y.adjustRadian180(), radian.z.adjustRadian180());
	}
	public static Vector3 adjustAngle180(this Vector3 degree)
	{
		return new(degree.x.adjustAngle180(), degree.y.adjustAngle180(), degree.z.adjustAngle180());
	}
	public static Vector3 adjustRadian360(this Vector3 radian)
	{
		return new(radian.x.adjustRadian360(), radian.y.adjustRadian360(), radian.z.adjustRadian360());
	}
	public static Vector3 adjustAngle360(this Vector3 degree)
	{
		return new(degree.x.adjustAngle360(), degree.y.adjustAngle360(), degree.z.adjustAngle360());
	}
	// 求从z轴到指定向量的水平方向上的顺时针角度,角度范围是-MATH_PI 到 MATH_PI
	public static float getAngle(this Vector3 vec, ANGLE radian = ANGLE.RADIAN)
	{
		vec.y = 0.0f;
		vec = vec.normalize();
		float angle = vec.z.acos();
		if (vec.x > 0.0f)
		{
			angle = -angle;
		}
		// 在unity的坐标系中航向角需要取反
		angle = -angle.adjustRadian180();
		if (radian == ANGLE.DEGREE)
		{
			angle = angle.toDegree();
		}
		return angle;
	}
	public static Vector3 rotate(this Vector3 vec, Matrix4x4 transMat3) { return transMat3 * vec; }
	// 使用一个四元数去旋转一个三维向量
	public static Vector3 rotate(this Vector3 vec, Quaternion transQuat) { return transQuat * vec; }
	// 求向量水平顺时针旋转一定角度后的向量,角度范围是-MATH_PI 到 MATH_PI
	public static Vector3 rotate(this Vector3 vec, float radian)
	{
		return vec.rotate(Quaternion.AngleAxis(radian.toDegree(), Vector3.up));
	}
	public static float dot(this Vector3 v0, Vector3 v1) { return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z; }
	public static Vector3 cross(this Vector3 v0, Vector3 v1) { return Vector3.Cross(v0, v1); }
}