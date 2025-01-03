#ifndef _QUATERNION_H_
#define _QUATERNION_H_

#include "Vector3.h"

struct Quaternion
{
public:
	float x;
	float y;
	float z;
	float w;
public:
	Quaternion();
	Quaternion(const Quaternion& other);
	Quaternion(float xx, float yy, float zz, float ww);
	// 根据转动轴和角度构建四元数
	Quaternion(float angleRadian, const Vector3& v);
	void clear();
	Quaternion conjugate() const;
	Quaternion inverse() const;
	float length() const;
	Quaternion normalize() const;
	Vector3 eulerAngles();
	float roll();
	float pitch();
	float yaw();
	float angle();
	Vector3 axis();
	Quaternion operator*(const Quaternion& that) const;
	Vector3 operator*(const Vector3& that);
	Quaternion operator*(float scale) const;
	Quaternion operator/(float that) const;
	Quaternion operator+(const Quaternion& that) const;
	Quaternion& operator+=(const Quaternion& that);
	Quaternion& operator*=(float scale);
	Quaternion& operator/=(float scale);
	Quaternion& operator*=(const Quaternion& that);
	Quaternion operator-() const;
public:
	static Quaternion eulerAnglesToQuaterion(const Vector3& eulerAngles);
	static float dot(const Quaternion& q1, const Quaternion& q2);
	static Quaternion cross(const Quaternion& q1, const Quaternion& q2);
	static Quaternion lerp(const Quaternion& x, const Quaternion& y, float a);
};

#endif