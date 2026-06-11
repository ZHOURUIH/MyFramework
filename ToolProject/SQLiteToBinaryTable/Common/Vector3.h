#pragma once

#include "FrameDefine.h"

struct Vector3
{
public:
	float x;
	float y;
	float z;
	static Vector3 ZERO;
public:
	Vector3() :
		x(0.0f),
		y(0.0f),
		z(0.0f)
	{}
	Vector3(const float xx, const float yy, const float zz) :
		x(xx),
		y(yy),
		z(zz)
	{}
	void clear()
	{
		x = 0.0f;
		y = 0.0f;
		z = 0.0f;
	}
	Vector3 operator*(const float that) const { return { x * that, y * that, z * that }; }
	Vector3 operator+(const Vector3& that) const { return { x + that.x, y + that.y, z + that.z }; }
	Vector3 operator-(const Vector3& that) const { return { x - that.x, y - that.y, z - that.z }; }
	Vector3& operator+=(const Vector3& that)
	{
		x += that.x;
		y += that.y;
		z += that.z;
		return *this;
	}
	Vector3& operator-=(const Vector3& that)
	{
		x -= that.x;
		y -= that.y;
		z -= that.z;
		return *this;
	}
	Vector3 operator-() const { return { -x, -y, -z }; }
	bool operator<(const Vector3& other) const { return x + y + z < other.x + other.y + other.z; }
	bool operator==(const Vector3& other) const { return x == other.x && y == other.y && z == other.z; }
	bool operator!=(const Vector3& other) const { return x != other.x || y != other.y || z != other.z; }
};