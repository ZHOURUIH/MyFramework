#pragma once

#include "Vector3.h"

struct Quaternion
{
public:
	float x = 0.0f;
	float y = 0.0f;
	float z = 0.0f;
	float w = 1.0f;
public:
	Quaternion() = default;
	Quaternion(const Quaternion& other);
	Quaternion(float xx, float yy, float zz, float ww);
	// 根据转动轴和角度构建四元数
	Quaternion(const Vector3& v, float angleRadian);
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
		z = 0.0f;
		w = 1.0f;
	}
	Quaternion conjugate() const { return { -x, -y, -z, w }; }
	Quaternion inverse() const { return conjugate() / dot(*this, *this); }
	float length() const { return sqrt(dot(*this, *this)); }
	Quaternion normalize() const;
	Vector3 eulerAngles() const { return { pitch(), yaw(), roll() }; }
	float roll() const { return atan2(2 * (x * y + w * z), w * w + x * x - y * y - z * z); }
	float pitch() const { return atan2(2.0f * (y * z + w * x), w * w - x * x - y * y + z * z); }
	float yaw() const { return asin(-2.0f * (x * z - w * y)); }
	float angle() const { return acos(w) * 2.0f; }
	Vector3 axis();
	Quaternion operator*(const Quaternion& that) const;
	Vector3 operator*(const Vector3& that);
	Quaternion operator*(const float scale) const { return { x * scale, y * scale, z * scale, w * scale }; }
	Quaternion operator/(const float that) const
	{
		const float inverse = 1.0f / that;
		return { x * inverse, y * inverse, z * inverse, w * inverse };
	}
	Quaternion operator+(const Quaternion& that) const { return { x + that.x, y + that.y, z + that.z, w + that.w }; }
	Quaternion& operator+=(const Quaternion& that)
	{
		x += that.x;
		y += that.y;
		z += that.z;
		w += that.w;
		return *this;
	}
	Quaternion& operator*=(const float scale)
	{
		x *= scale;
		y *= scale;
		z *= scale;
		w *= scale;
		return *this;
	}
	Quaternion& operator/=(const float scale)
	{
		const float inverseScale = 1.0f / scale;
		x *= inverseScale;
		y *= inverseScale;
		z *= inverseScale;
		w *= inverseScale;
		return *this;
	}
	Quaternion& operator*=(const Quaternion& that);
	Quaternion operator-() const { return { -x, -y, -z, -w }; }
public:
	static Quaternion eulerAnglesToQuaterion(const Vector3& eulerAngles);
	static float dot(const Quaternion& q1, const Quaternion& q2) { return q1.x * q2.x + q1.y * q2.y + q1.z * q2.z + q1.w * q2.w; }
	static Quaternion cross(const Quaternion& q1, const Quaternion& q2);
	static Quaternion lerp(const Quaternion& x, const Quaternion& y, const float a) { return x * (1.0f - a) + (y * a); }
};